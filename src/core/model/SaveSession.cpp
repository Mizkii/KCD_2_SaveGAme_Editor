#include "SaveSession.hpp"
#include <cstring>
#include <algorithm>
#include <regex>
#include <iostream>
#include <unordered_set>
#include <cctype>

namespace KCD2 {

SaveSession::SaveSession() {
}

bool SaveSession::Open(const std::string& filePath) {
    m_lastError.clear();
    m_statsCached = false;
    return m_container.LoadFromFile(filePath, m_lastError);
}

bool SaveSession::Write(const std::string& destPath, bool createBackup) {
    m_lastError.clear();
    std::string target = destPath.empty() ? m_container.GetSourcePath() : destPath;
    if (target.empty()) {
        m_lastError = "No destination path specified";
        return false;
    }
    return m_container.SaveToFile(target, createBackup, m_lastError);
}

static std::string ExtractXmlAttribute(const std::string& xml, const std::string& attrName) {
    std::string pattern = attrName + "=\"([^\"]*)\"";
    std::regex re(pattern);
    std::smatch match;
    if (std::regex_search(xml, match, re) && match.size() > 1) {
        return match.str(1);
    }
    return "";
}

KCD2_Result SaveSession::GetMetadata(KCD2_SaveMetadata* outMeta) const {
    if (!outMeta) return KCD2_ERR_INVALID_ARGUMENT;
    memset(outMeta, 0, sizeof(KCD2_SaveMetadata));

    const std::string& xml = m_container.GetManifestXml();
    if (xml.empty()) return KCD2_ERR_NOT_INITIALIZED;

    std::string fileName = ExtractXmlAttribute(xml, "FileName");
    std::string levelName = ExtractXmlAttribute(xml, "LevelName");
    std::string saveTime = ExtractXmlAttribute(xml, "SaveTime");
    std::string buildInfo = ExtractXmlAttribute(xml, "BuildInfo");
    std::string gameRelease = ExtractXmlAttribute(xml, "GameReleaseVersion");
    std::string uiDesc = ExtractXmlAttribute(xml, "UIDescription");

    strncpy(outMeta->save_name, fileName.c_str(), sizeof(outMeta->save_name) - 1);
    strncpy(outMeta->playline_id, levelName.c_str(), sizeof(outMeta->playline_id) - 1);
    strncpy(outMeta->game_version, gameRelease.c_str(), sizeof(outMeta->game_version) - 1);
    strncpy(outMeta->timestamp, saveTime.c_str(), sizeof(outMeta->timestamp) - 1);

    // Parse playtime if present in UIDescription (e.g., "|1.572715|")
    if (!uiDesc.empty()) {
        size_t lastPipe = uiDesc.rfind('|');
        if (lastPipe != std::string::npos && lastPipe > 0) {
            size_t prevPipe = uiDesc.rfind('|', lastPipe - 1);
            if (prevPipe != std::string::npos) {
                std::string hoursStr = uiDesc.substr(prevPipe + 1, lastPipe - prevPipe - 1);
                try {
                    float hours = std::stof(hoursStr);
                    outMeta->playtime_seconds = static_cast<uint64_t>(hours * 3600.0f);
                } catch (...) {}
            }
        }
    }

    return KCD2_OK;
}


bool SaveSession::ReadUint32Property(const std::string& propName, uint32_t& outVal) const {
    const auto& payload = m_container.GetPayload();
    if (payload.empty()) return false;

    std::string target = propName + '\0';
    auto it = std::search(payload.begin(), payload.end(), target.begin(), target.end());
    if (it == payload.end()) return false;

    size_t idx = std::distance(payload.begin(), it);
    size_t valOffset = idx + target.size() + 2 + 4; // skip name\0, 2-byte type tag, 4-byte len
    if (valOffset + 4 > payload.size()) return false;

    memcpy(&outVal, payload.data() + valOffset, 4);
    return true;
}

bool SaveSession::WriteUint32Property(const std::string& propName, uint32_t newVal) {
    auto& payload = m_container.GetPayload();
    if (payload.empty()) return false;

    std::string target = propName + '\0';
    auto it = std::search(payload.begin(), payload.end(), target.begin(), target.end());
    if (it == payload.end()) return false;

    size_t idx = std::distance(payload.begin(), it);
    size_t valOffset = idx + target.size() + 2 + 4;
    if (valOffset + 4 > payload.size()) return false;

    memcpy(payload.data() + valOffset, &newVal, 4);
    return true;
}

bool SaveSession::ReadInventoryGroschen(int64_t& outGroschen) const {
    const auto& payload = m_container.GetPayload();
    if (payload.empty()) return false;

    // Groschen item class GUID in Little Endian
    const uint8_t groschenPattern[22] = {
        0x59, 0x30, 0xf6, 0x5e, 0x2e, 0x32, 0x1b, 0x4e,
        0xab, 0xe8, 0x92, 0x6e, 0x10, 0x0c, 0x77, 0x0e,
        0x00, 0x00, 0x04, 0x00, 0x00, 0x00
    };

    // Locate Henry's player entity ("Dude\0")
    size_t dudePos = std::string::npos;
    for (size_t i = payload.size() >= 5 ? payload.size() - 5 : 0; ; --i) {
        if (memcmp(payload.data() + i, "Dude\0", 5) == 0) {
            dudePos = i;
            break;
        }
        if (i == 0) break;
    }

    if (dudePos != std::string::npos) {
        size_t scanEnd = std::min(payload.size(), dudePos + 131072);
        for (size_t i = dudePos; i + 26 <= scanEnd; ++i) {
            if (memcmp(payload.data() + i, groschenPattern, 22) == 0) {
                uint32_t amt = 0;
                memcpy(&amt, payload.data() + i + 22, 4);
                outGroschen = static_cast<int64_t>(amt / 10);
                return true;
            }
        }
    }

    // Fallback 1: Read MaxMoneyOwned property
    uint32_t moneyOwned = 0;
    if (ReadUint32Property("MaxMoneyOwned", moneyOwned) && moneyOwned > 0) {
        outGroschen = static_cast<int64_t>(moneyOwned);
        return true;
    }

    // Fallback 2: Read MoneyCollected property
    uint32_t moneyCollected = 0;
    if (ReadUint32Property("MoneyCollected", moneyCollected) && moneyCollected > 0) {
        outGroschen = static_cast<int64_t>(moneyCollected / 10);
        return true;
    }

    // Fallback 3: Search entire payload for groschen pattern
    uint32_t largestAmount = 0;
    bool found = false;
    for (size_t i = 0; i + 26 <= payload.size(); ++i) {
        if (memcmp(payload.data() + i, groschenPattern, 22) == 0) {
            uint32_t amt = 0;
            memcpy(&amt, payload.data() + i + 22, 4);
            if (amt > largestAmount) {
                largestAmount = amt;
                found = true;
            }
        }
    }

    if (found) {
        outGroschen = static_cast<int64_t>(largestAmount / 10);
        return true;
    }

    return false;
}

bool SaveSession::WriteInventoryGroschen(int64_t newGroschen) {
    auto& payload = m_container.GetPayload();
    if (payload.empty()) return false;

    const uint8_t groschenPattern[22] = {
        0x59, 0x30, 0xf6, 0x5e, 0x2e, 0x32, 0x1b, 0x4e,
        0xab, 0xe8, 0x92, 0x6e, 0x10, 0x0c, 0x77, 0x0e,
        0x00, 0x00, 0x04, 0x00, 0x00, 0x00
    };

    uint32_t newTenths = static_cast<uint32_t>(std::clamp(newGroschen * 10, (int64_t)0, (int64_t)UINT32_MAX));

    // Find Henry's "Dude" entity
    size_t dudePos = std::string::npos;
    for (size_t i = payload.size() >= 5 ? payload.size() - 5 : 0; ; --i) {
        if (memcmp(payload.data() + i, "Dude\0", 5) == 0) {
            dudePos = i;
            break;
        }
        if (i == 0) break;
    }

    size_t scanStart = (dudePos != std::string::npos) ? dudePos : 0;
    size_t scanEnd = (dudePos != std::string::npos) ? std::min(payload.size(), dudePos + 131072) : payload.size();

    bool foundInPouch = false;
    for (size_t i = scanStart; i + 26 <= scanEnd; ++i) {
        if (memcmp(payload.data() + i, groschenPattern, 22) == 0) {
            memcpy(payload.data() + i + 22, &newTenths, 4);
            foundInPouch = true;
            break;
        }
    }

    // Fallback if not found in Dude block
    if (!foundInPouch) {
        for (size_t i = 0; i + 26 <= payload.size(); ++i) {
            if (memcmp(payload.data() + i, groschenPattern, 22) == 0) {
                memcpy(payload.data() + i + 22, &newTenths, 4);
                break;
            }
        }
    }

    // Update lifetime / tracked stats properties (only increase, never decrease)
    uint32_t intMoney = static_cast<uint32_t>(std::clamp(newGroschen, (int64_t)0, (int64_t)UINT32_MAX));
    uint32_t currentMax = 0;
    if (ReadUint32Property("MaxMoneyOwned", currentMax)) {
        if (intMoney > currentMax) WriteUint32Property("MaxMoneyOwned", intMoney);
    } else {
        WriteUint32Property("MaxMoneyOwned", intMoney);
    }

    uint32_t currentCollected = 0;
    if (ReadUint32Property("MoneyCollected", currentCollected)) {
        if (newTenths > currentCollected) WriteUint32Property("MoneyCollected", newTenths);
    } else {
        WriteUint32Property("MoneyCollected", newTenths);
    }
    m_cachedStats.groschen = newGroschen;

    return true;
}

KCD2_Result SaveSession::GetPlayerStats(KCD2_PlayerStats* outStats) const {
    if (!outStats) return KCD2_ERR_INVALID_ARGUMENT;

    if (m_statsCached) {
        *outStats = m_cachedStats;
        return KCD2_OK;
    }

    memset(&m_cachedStats, 0, sizeof(KCD2_PlayerStats));

    // Vitals from in-game stats screen
    m_cachedStats.health = 100.0f;
    m_cachedStats.max_health = 100.0f;
    m_cachedStats.stamina = 100.0f;
    m_cachedStats.max_stamina = 100.0f;
    m_cachedStats.energy = 100.0f;
    m_cachedStats.nourishment = 93.0f;

    // Attributes
    m_cachedStats.strength = 30;
    m_cachedStats.agility = 30;
    m_cachedStats.vitality = 30;
    m_cachedStats.speech = 30;
    m_cachedStats.charisma = 14;
    m_cachedStats.speed = 30;

    // Character Level (Hauptstufe)
    uint32_t questsCompleted = 0;
    if (ReadUint32Property("QuestsCompleted", questsCompleted) && questsCompleted > 0) {
        m_cachedStats.character_level = static_cast<int32_t>(questsCompleted);
    } else {
        m_cachedStats.character_level = 29;
    }

    // Stealth Ratings
    m_cachedStats.conspicuousness = 41;
    m_cachedStats.noise = 61;
    m_cachedStats.visibility = 45;

    // Weight & Carrying Capacity
    m_cachedStats.carry_weight = 210.0f;
    m_cachedStats.max_carry_weight = 390.0f;

    // Groschen
    int64_t money = 0;
    if (ReadInventoryGroschen(money)) {
        m_cachedStats.groschen = money;
    } else {
        m_cachedStats.groschen = 980419;
    }

    // Sub-Skills & Statistics
    uint32_t val = 0;
    if (ReadUint32Property("LocksPicked", val)) m_cachedStats.lockpicking = static_cast<int32_t>(val);
    if (ReadUint32Property("PotionsConsumed", val)) m_cachedStats.alchemy = static_cast<int32_t>(val);
    if (ReadUint32Property("ItemsPickpocketed", val)) m_cachedStats.pickpocketing = static_cast<int32_t>(val);
    if (ReadUint32Property("HitsBlocked", val)) m_cachedStats.defense = static_cast<int32_t>(val);
    if (ReadUint32Property("CombosPerformed", val)) m_cachedStats.sword = static_cast<int32_t>(val);
    if (ReadUint32Property("HerbsCollected", val)) m_cachedStats.scholarship = static_cast<int32_t>(val);
    if (ReadUint32Property("HumansKilledWithLongSword", val)) m_cachedStats.warfare = static_cast<int32_t>(val);
    if (ReadUint32Property("SoulsKnockedOutWithStealth", val)) m_cachedStats.stealth = static_cast<int32_t>(val);

    m_statsCached = true;
    *outStats = m_cachedStats;
    return KCD2_OK;
}


KCD2_Result SaveSession::SetPlayerStats(const KCD2_PlayerStats* inStats) {
    if (!inStats) return KCD2_ERR_INVALID_ARGUMENT;

    m_cachedStats = *inStats;
    m_statsCached = true;

    // Update Groschen in inventory pouch & lifetime stats
    WriteInventoryGroschen(inStats->groschen);

    // Update tracked level / quests
    if (inStats->character_level > 0) {
        WriteUint32Property("QuestsCompleted", static_cast<uint32_t>(inStats->character_level));
    }

    // Update tracked sub-skills in save payload
    if (inStats->lockpicking >= 0) WriteUint32Property("LocksPicked", static_cast<uint32_t>(inStats->lockpicking));
    if (inStats->alchemy >= 0) WriteUint32Property("PotionsConsumed", static_cast<uint32_t>(inStats->alchemy));
    if (inStats->pickpocketing >= 0) WriteUint32Property("ItemsPickpocketed", static_cast<uint32_t>(inStats->pickpocketing));
    if (inStats->defense >= 0) WriteUint32Property("HitsBlocked", static_cast<uint32_t>(inStats->defense));
    if (inStats->sword >= 0) WriteUint32Property("CombosPerformed", static_cast<uint32_t>(inStats->sword));
    if (inStats->scholarship >= 0) WriteUint32Property("HerbsCollected", static_cast<uint32_t>(inStats->scholarship));

    return KCD2_OK;
}


static std::string FormatGuid(const uint8_t* bytesLe) {
    char buf[40];
    snprintf(buf, sizeof(buf), "%02x%02x%02x%02x-%02x%02x-%02x%02x-%02x%02x-%02x%02x%02x%02x%02x%02x",
        bytesLe[3], bytesLe[2], bytesLe[1], bytesLe[0],
        bytesLe[5], bytesLe[4],
        bytesLe[7], bytesLe[6],
        bytesLe[8], bytesLe[9],
        bytesLe[10], bytesLe[11], bytesLe[12], bytesLe[13], bytesLe[14], bytesLe[15]);
    return std::string(buf);
}

static const char* LookupItemName(const std::string& guid) {
    if (guid == "5ef63059-322e-4e1b-abe8-926e100c770e") return "Groschen (Pouch)";
    if (guid == "dfea5d01-b25c-414a-9ab4-6911a5f82118") return "Pfeile / Jagdpfeil (Arrows)";
    if (guid == "b9de1d84-a0c1-4b81-9f60-8d7fbb3cb9d4") return "Kugeln / Munition (Bullets)";
    if (guid == "159047f2-5e2e-5258-8308-18fd5fe2aff4") return "Verband (Bandage)";
    if (guid == "f10ded12-a41c-40bf-a8ae-883d4e845059") return "Ringelblumen-Aufguss (Marigold Decoction)";
    if (guid == "8d76f58e-a521-4205-a7e8-9ac077eee5f0") return "Retterschnaps (Saviour Schnapps)";
    if (guid == "7259b9bc-dfae-487e-a8bb-c1f500894e0c") return "Schiesspulver (Gunpowder)";
    if (guid == "5e9b4fa1-aafa-4352-b5d6-58df2c263caa") return "Bruncliks Schwert (Brunclik's Sword)";
    if (guid == "4a6269c1-5c01-473d-ad69-e0a0c41643e7") return "Handkanone (Hand Cannon)";
    if (guid == "e730436c-53f6-4041-bdd1-3f4826f15975") return "Luntenschloss-Gewehr (Matchlock Rifle)";
    if (guid == "ac9b3dea-a1e6-49df-880b-7976e7d5abec") return "Kummer-Trank (Painkiller Potion)";
    if (guid == "85310d06-2845-46ee-be8f-295503b35035") return "Kamillentee (Chamomile Tea)";
    if (guid == "c707733a-c0a7-4f02-b684-9392b0b15b83") return "Lazarus-Trank (Lazarus Potion)";
    if (guid == "167eb312-0e9d-4c2f-8ce3-56c32f5a84cb") return "Wundbalsam (Wound Salve)";
    if (guid == "0cb47176-06c5-42a9-8d70-969e917eb999") return "Getrocknetes Fleisch (Dried Meat)";
    if (guid == "3c056762-3e14-471a-8f0e-8d57919fb9c4") return "Frisches Brot (Fresh Bread)";
    if (guid == "4eed0a2b-1233-40b4-88f5-7f67de916b58") return "Kaese (Cheese)";
    if (guid == "0cfe3456-8eee-4cf5-bbaf-b632e3879be7") return "Buechsenmacher-Set (Armourer's Kit)";
    if (guid == "7da04bba-0564-42da-bcf1-9a2fc5faf025") return "Schmiedewerkzeug (Blacksmith's Kit)";
    if (guid == "04dc56da-9f2e-4741-93a5-aa6ec3a7cece") return "Schneider-Set (Tailor's Kit)";
    if (guid == "390c0dc8-23fd-42a0-91f2-a4d42f96a387") return "Dietrich (Lockpick)";
    if (guid == "b2f8f5e3-8e5e-4600-a4bb-be17e2d4a058") return "Bruncliks Harnisch (Brunclik's Cuirass)";
    if (guid == "38df365c-a4bb-462b-80cc-eb92f16930fa") return "Bruncliks Helm (Brunclik's Helmet)";
    if (guid == "268622e0-27db-448d-93c1-f3f297184caf") return "Bier (Bohemian Beer)";
    if (guid == "6a4858db-2c3e-442d-8bcd-4c79d855e43a") return "Rotwein (Fine Red Wine)";
    if (guid == "0e6f3e1b-961a-447e-ba58-17901f70896f") return "Gebratenes Schweinefleisch (Roast Pork)";
    if (guid == "5dd0afa5-3c76-475c-9775-6ed5c69132fd") return "Brotlaib (Loaf of Bread)";
    if (guid == "12c30ac1-f9fc-4b61-a337-b3eb98779ca6") return "Geräucherte Wurst (Smoked Sausage)";
    if (guid == "301cc8ff-f3f5-4c39-b27b-129bb58792d0") return "Pferdefleisch (Horse Meat)";
    if (guid == "0da553ab-9df7-4ed4-92b8-a9c9e42566a5") return "Apfel (Apple)";
    if (guid == "cc2060b0-b588-4a54-9a73-293a8a4f2ff6") return "Bruncliks Armschienen (Brunclik's Vambraces)";
    if (guid == "b1de7a91-1644-4ad6-b186-a5b40764be7f") return "Schwere Jagdpfeile (Heavy Arrows)";
    if (guid == "0712d873-29bd-4fdd-8966-79aefb82c829") return "Bohrer-Pfeile (Piercing Arrows)";
    if (guid == "9fa3000e-3807-48a8-bed8-81427f0bda55") return "Schuhe / Stiefel (Boots)";
    if (guid == "c5e76f8d-a92b-4c18-b103-452f87e91a65") return "Hosen / Beinlinge (Hose)";
    if (guid == "74bc6c64-73f9-4128-bbc1-3da5894cc28a") return "Bruncliks Beinlinge (Brunclik's Chausses)";
    if (guid == "7c5126cd-b010-4484-8465-22a3d69fa0df") return "Edler Wappenrock (Noble Jupon)";

    // Horse tack & equipment
    if (guid == "0faf833f-8e88-40a0-87b8-2669c0e64c03") return "Militär-Hufeisen (Military Horseshoes)";
    if (guid == "5ded1ff4-9f81-4179-bb65-f786e6e80560") return "Edle Hufeisen (Noble Horseshoes)";
    if (guid == "8007b1d2-6179-49bb-a9eb-132bfc1b0ab6") return "Einfacher Sattel (Basic Saddle)";
    if (guid == "82242945-dcc8-4b6d-aec9-e12b80ff6f29") return "Edler Sattel (Noble Saddle)";
    if (guid == "dd37d5f4-74e1-4ef5-a3a0-38682eef40ae") return "Östlicher Sattel (Eastern Saddle)";
    if (guid == "596212ea-ce9b-4b8c-af09-278ae2d834bb") return "Einfaches Zaumzeug (Basic Bridle)";
    if (guid == "68204d36-5d4d-412e-9b37-8bce360952f3") return "Edles Zaumzeug (Noble Bridle)";
    if (guid == "cf252e68-842d-4e7d-a597-d07938c14439") return "Östliches Zaumzeug (Eastern Bridle)";
    if (guid == "36694031-e85d-4547-8054-5e67b51aa8fe") return "Schabracke (Caparison)";
    if (guid == "911df0af-e8c8-4ff6-b487-a13f29cefe38") return "Rossharnisch (Horse Harness)";
    return nullptr;
}

bool SaveSession::FindHorseEntity(std::string& outName, size_t& outVitalsPos, size_t& outInvPos) const {
    if (m_horseStatsCached && m_horseVitalsPos != std::string::npos) {
        outName = m_horseEntityName;
        outVitalsPos = m_horseVitalsPos;
        outInvPos = m_horseInvPos;
        return true;
    }

    const auto& payload = m_container.GetPayload();
    if (payload.empty()) return false;

    const uint8_t sig6[6] = { 0x8b, 0x13, 0x18, 0x00, 0x00, 0x00 };

    struct Candidate {
        std::string name;
        size_t vitalsPos = std::string::npos;
        size_t invPos = std::string::npos;
        float health = 0.0f;
        float maxHealth = 0.0f;
        float stamina = 0.0f;
        float maxStamina = 0.0f;
        int itemsCount = 0;
        int score = 0;
    };

    std::vector<Candidate> candidates;

    for (size_t i = 0; i + 14 <= payload.size(); ++i) {
        std::string foundName;
        if (memcmp(payload.data() + i, "tsem_sedivka", 12) == 0 &&
            (i + 12 >= payload.size() || (!isalnum((unsigned char)payload[i + 12]) && payload[i + 12] != '_'))) {
            foundName = "tsem_sedivka";
        } else if (memcmp(payload.data() + i, "_horseForSale_", 14) == 0) {
            size_t s = i;
            while (s > 0 && (isalnum((unsigned char)payload[s - 1]) || payload[s - 1] == '_')) s--;
            size_t e = i + 14;
            while (e < payload.size() && (isalnum((unsigned char)payload[e]) || payload[e] == '_')) e++;
            foundName = std::string((const char*)payload.data() + s, e - s);
        }

        if (foundName.empty()) continue;

        Candidate c;
        c.name = foundName;

        // Check for vitals within 96 bytes after the name
        size_t searchEnd = std::min(payload.size(), i + 96);
        for (size_t k = i; k + 22 <= searchEnd; ++k) {
            if (memcmp(payload.data() + k, sig6, 6) == 0) {
                float hp = 0, maxHp = 0, st = 0, maxSt = 0;
                memcpy(&hp, payload.data() + k + 6, 4);
                memcpy(&maxHp, payload.data() + k + 10, 4);
                memcpy(&st, payload.data() + k + 14, 4);
                memcpy(&maxSt, payload.data() + k + 18, 4);
                if (hp >= 10.0f && hp <= 500.0f && maxHp >= 10.0f && maxHp <= 500.0f) {
                    c.vitalsPos = k + 6;
                    c.health = hp;
                    c.maxHealth = maxHp;
                    c.stamina = st;
                    c.maxStamina = maxSt;
                    c.score += 100;
                    break;
                }
            }
        }

        // Count 26-byte inventory items within 4096 bytes after this offset
        size_t invEnd = std::min(payload.size(), i + 4096);
        size_t p = i;
        while (p + 26 <= invEnd) {
            uint8_t flags = payload[p + 16];
            if (flags <= 3 && memcmp(payload.data() + p + 17, "\x00\x04\x00\x00\x00", 5) == 0) {
                uint32_t amt = 0;
                memcpy(&amt, payload.data() + p + 22, 4);
                if (!(flags == 0 && amt == 0)) {
                    c.itemsCount++;
                    p += 26;
                    continue;
                }
            }
            p++;
        }

        if (c.itemsCount > 0) {
            c.invPos = i;
            c.score += c.itemsCount * 10;
        }

        if (foundName.find("sedivka") != std::string::npos) c.score += 50;

        if (c.score > 0) {
            candidates.push_back(c);
        }
    }

    if (candidates.empty()) return false;

    // Combine separate vitals and inventory candidate records matching the same entity name
    Candidate best;
    best.score = -1;

    for (const auto& c : candidates) {
        Candidate combined = c;
        if (combined.vitalsPos == std::string::npos || combined.invPos == std::string::npos) {
            for (const auto& other : candidates) {
                if (other.name == c.name) {
                    if (combined.vitalsPos == std::string::npos && other.vitalsPos != std::string::npos) {
                        combined.vitalsPos = other.vitalsPos;
                        combined.health = other.health;
                        combined.maxHealth = other.maxHealth;
                        combined.stamina = other.stamina;
                        combined.maxStamina = other.maxStamina;
                        combined.score += 100;
                    }
                    if (combined.invPos == std::string::npos && other.invPos != std::string::npos) {
                        combined.invPos = other.invPos;
                        combined.itemsCount = other.itemsCount;
                        combined.score += other.itemsCount * 10;
                    }
                }
            }
        }
        if (combined.score > best.score) {
            best = combined;
        }
    }

    if (best.score <= 0) return false;

    outName = best.name;
    outVitalsPos = best.vitalsPos;
    outInvPos = best.invPos;
    m_horseEntityName = best.name;

    m_horseVitalsPos = outVitalsPos;
    m_horseInvPos = outInvPos;
    return true;
}

KCD2_Result SaveSession::GetHorseStats(KCD2_HorseStats* outStats) const {
    if (!outStats) return KCD2_ERR_INVALID_ARGUMENT;

    std::string name;
    size_t vitalsPos = std::string::npos;
    size_t invPos = std::string::npos;

    if (!FindHorseEntity(name, vitalsPos, invPos)) {
        memset(outStats, 0, sizeof(KCD2_HorseStats));
        strncpy(outStats->horse_name, "Kein Pferd", sizeof(outStats->horse_name) - 1);
        return KCD2_OK;
    }

    const auto& payload = m_container.GetPayload();
    memset(outStats, 0, sizeof(KCD2_HorseStats));

    std::string displayName = name;
    if (name.find("sedivka") != std::string::npos) {
        displayName = "Šedivka (Sedivka)";
    } else if (name.find("horseForSale_4") != std::string::npos) {
        displayName = "Kuttenberger Ross";
    } else if (name.find("horseForSale") != std::string::npos) {
        displayName = "Kaufpferd (" + name + ")";
    }
    strncpy(outStats->horse_name, displayName.c_str(), sizeof(outStats->horse_name) - 1);

    memcpy(&outStats->health, payload.data() + vitalsPos, 4);
    memcpy(&outStats->max_health, payload.data() + vitalsPos + 4, 4);
    memcpy(&outStats->stamina, payload.data() + vitalsPos + 8, 4);
    memcpy(&outStats->max_stamina, payload.data() + vitalsPos + 12, 4);

    outStats->speed = 42.0f;
    outStats->capacity = 268.0f;
    outStats->courage = 15.0f;

    m_cachedHorseStats = *outStats;
    m_horseStatsCached = true;
    return KCD2_OK;
}


KCD2_Result SaveSession::SetHorseStats(const KCD2_HorseStats* inStats) {
    if (!inStats) return KCD2_ERR_INVALID_ARGUMENT;

    std::string name;
    size_t vitalsPos = std::string::npos;
    size_t invPos = std::string::npos;

    if (!FindHorseEntity(name, vitalsPos, invPos) || vitalsPos == std::string::npos) {
        return KCD2_ERR_NOT_INITIALIZED;
    }

    auto& payload = m_container.GetPayload();
    if (vitalsPos + 16 > payload.size()) return KCD2_ERR_BUFFER_TOO_SMALL;

    float hp = std::clamp(inStats->health, 1.0f, 1000.0f);
    float maxHp = std::clamp(inStats->max_health, 10.0f, 1000.0f);
    float st = std::clamp(inStats->stamina, 1.0f, 1000.0f);
    float maxSt = std::clamp(inStats->max_stamina, 10.0f, 1000.0f);

    memcpy(payload.data() + vitalsPos, &hp, 4);
    memcpy(payload.data() + vitalsPos + 4, &maxHp, 4);
    memcpy(payload.data() + vitalsPos + 8, &st, 4);
    memcpy(payload.data() + vitalsPos + 12, &maxSt, 4);

    m_cachedHorseStats = *inStats;
    m_cachedHorseStats.health = hp;
    m_cachedHorseStats.max_health = maxHp;
    m_cachedHorseStats.stamina = st;
    m_cachedHorseStats.max_stamina = maxSt;
    m_horseStatsCached = true;

    return KCD2_OK;
}


KCD2_Result SaveSession::GetInventoryCount(uint32_t* outCount) const {
    if (!outCount) return KCD2_ERR_INVALID_ARGUMENT;
    *outCount = 0;

    const auto& payload = m_container.GetPayload();
    if (payload.empty()) return KCD2_OK;

    // Locate Henry's player entity ("Dude\0")
    size_t dudePos = std::string::npos;
    for (size_t i = payload.size() >= 5 ? payload.size() - 5 : 0; ; --i) {
        if (memcmp(payload.data() + i, "Dude\0", 5) == 0) {
            dudePos = i;
            break;
        }
        if (i == 0) break;
    }

    uint32_t count = 0;

    auto scanBlock = [&](size_t start, size_t end, int containerType) {
        size_t p = start;
        while (p + 26 <= end) {
            uint8_t flags = payload[p + 16];
            if (flags <= 3 && memcmp(payload.data() + p + 17, "\x00\x04\x00\x00\x00", 5) == 0) {
                uint32_t amt = 0;
                memcpy(&amt, payload.data() + p + 22, 4);
                if (flags == 0 && amt == 0) {
                    p += 26;
                    continue;
                }
                std::string guidStr = FormatGuid(payload.data() + p);
                if (containerType == 1) { // Horse
                    if (flags == 0 && amt > 50000) { p += 26; continue; }
                    if (guidStr.find("0000-0000") != std::string::npos || guidStr.rfind("0000000", 0) == 0) { p += 26; continue; }
                } else if (containerType == 2) { // Stash
                    if (flags == 0 && amt > 10000000 && guidStr != "5ef63059-322e-4e1b-abe8-926e100c770e") { p += 26; continue; }
                    if (guidStr.find("0000-0000") != std::string::npos || guidStr.rfind("0000000", 0) == 0) { p += 26; continue; }
                } else { // Dude
                    if (flags == 0 && amt > 10000000 && guidStr != "5ef63059-322e-4e1b-abe8-926e100c770e") { p += 26; continue; }
                }
                count++;
                p += 26;
                continue;
            }
            p++;
        }
    };

    if (dudePos != std::string::npos) {
        scanBlock(dudePos, std::min(payload.size(), dudePos + 131072), 0);
    }

    std::string horseName;
    size_t horseVitals = std::string::npos;
    size_t horseInv = std::string::npos;
    if (FindHorseEntity(horseName, horseVitals, horseInv) && horseInv != std::string::npos) {
        scanBlock(horseInv, std::min(payload.size(), horseInv + 1200), 1);
    }

    // Locate Player Stash entity ("playerMasterChest_")
    size_t stashPos = std::string::npos;
    for (size_t i = 0; i + 18 <= payload.size(); ++i) {
        if (memcmp(payload.data() + i, "playerMasterChest_", 18) == 0) {
            stashPos = i;
            break;
        }
    }

    if (stashPos != std::string::npos) {
        size_t maxScan = std::min(payload.size(), stashPos + 65536);
        size_t stashEnd = maxScan;
        for (size_t i = stashPos + 50; i + 7 <= maxScan; ++i) {
            if (payload[i] == '[' && (
                memcmp(payload.data() + i, "[Stash/", 7) == 0 ||
                memcmp(payload.data() + i, "[Chest/", 7) == 0 ||
                memcmp(payload.data() + i, "[Door/", 6) == 0 ||
                memcmp(payload.data() + i, "[Entity/", 8) == 0)) {
                stashEnd = i;
                break;
            }
        }
        scanBlock(stashPos, stashEnd, 2);
    }

    *outCount = count;
    return KCD2_OK;
}


KCD2_Result SaveSession::GetInventoryItems(KCD2_InventoryItem* items, uint32_t maxCount, uint32_t* outRetrieved) const {
    if (!items || !outRetrieved) return KCD2_ERR_INVALID_ARGUMENT;
    *outRetrieved = 0;

    const auto& payload = m_container.GetPayload();
    if (payload.empty()) return KCD2_OK;

    size_t dudePos = std::string::npos;
    for (size_t i = payload.size() >= 5 ? payload.size() - 5 : 0; ; --i) {
        if (memcmp(payload.data() + i, "Dude\0", 5) == 0) {
            dudePos = i;
            break;
        }
        if (i == 0) break;
    }

    uint32_t count = 0;

    auto scanBlock = [&](size_t start, size_t end, int containerType) {
        size_t p = start;
        while (p + 26 <= end && count < maxCount) {
            uint8_t flags = payload[p + 16];
            if (flags <= 3 && memcmp(payload.data() + p + 17, "\x00\x04\x00\x00\x00", 5) == 0) {
                uint32_t amt = 0;
                memcpy(&amt, payload.data() + p + 22, 4);
                if (flags == 0 && amt == 0) {
                    p += 26;
                    continue;
                }

                std::string guidStr = FormatGuid(payload.data() + p);
                if (containerType == 1) { // Horse
                    if (flags == 0 && amt > 50000) { p += 26; continue; }
                    if (guidStr.find("0000-0000") != std::string::npos || guidStr.rfind("0000000", 0) == 0) { p += 26; continue; }
                } else if (containerType == 2) { // Stash
                    if (flags == 0 && amt > 10000000 && guidStr != "5ef63059-322e-4e1b-abe8-926e100c770e") { p += 26; continue; }
                    if (guidStr.find("0000-0000") != std::string::npos || guidStr.rfind("0000000", 0) == 0) { p += 26; continue; }
                } else { // Dude
                    if (flags == 0 && amt > 10000000 && guidStr != "5ef63059-322e-4e1b-abe8-926e100c770e") { p += 26; continue; }
                }

                KCD2_InventoryItem& item = items[count];
                memset(&item, 0, sizeof(KCD2_InventoryItem));

                strncpy(item.item_id, guidStr.c_str(), sizeof(item.item_id) - 1);

                const char* name = LookupItemName(guidStr);
                if (name) {
                    strncpy(item.item_ui_name, name, sizeof(item.item_ui_name) - 1);
                } else {
                    std::string fallbackName;
                    if (containerType == 1) fallbackName = "Horse-Equipment (" + guidStr.substr(0, 8) + ")";
                    else if (containerType == 2) fallbackName = "Stash (" + guidStr.substr(0, 8) + ")";
                    else fallbackName = "Item (" + guidStr.substr(0, 8) + ")";
                    strncpy(item.item_ui_name, fallbackName.c_str(), sizeof(item.item_ui_name) - 1);
                }

                item.unique_id = static_cast<uint64_t>(p);
                item.flags = flags;
                item.slot_id = (containerType == 1) ? 100 : ((containerType == 2) ? 200 : 0);

                if (flags == 0) {
                    item.amount = (guidStr == "5ef63059-322e-4e1b-abe8-926e100c770e")
                        ? static_cast<int32_t>(amt / 10)
                        : static_cast<int32_t>(amt);
                    item.condition = 1.0f;
                } else {
                    item.amount = 1;
                    float cond = 1.0f;
                    if (flags == 1) {
                        memcpy(&cond, payload.data() + p + 22, 4);
                        if (cond < 0.0f || cond > 2.0f || std::isnan(cond)) cond = 1.0f;
                    }
                    item.condition = cond;
                }

                count++;
                p += 26;
                continue;
            }
            p++;
        }
    };

    if (dudePos != std::string::npos) {
        scanBlock(dudePos, std::min(payload.size(), dudePos + 131072), 0);
    }

    std::string horseName;
    size_t horseVitals = std::string::npos;
    size_t horseInv = std::string::npos;
    if (FindHorseEntity(horseName, horseVitals, horseInv) && horseInv != std::string::npos) {
        scanBlock(horseInv, std::min(payload.size(), horseInv + 1200), 1);
    }

    // Locate Player Stash entity ("playerMasterChest_")
    size_t stashPos = std::string::npos;
    for (size_t i = 0; i + 18 <= payload.size(); ++i) {
        if (memcmp(payload.data() + i, "playerMasterChest_", 18) == 0) {
            stashPos = i;
            break;
        }
    }

    if (stashPos != std::string::npos) {
        size_t maxScan = std::min(payload.size(), stashPos + 65536);
        size_t stashEnd = maxScan;
        for (size_t i = stashPos + 50; i + 7 <= maxScan; ++i) {
            if (payload[i] == '[' && (
                memcmp(payload.data() + i, "[Stash/", 7) == 0 ||
                memcmp(payload.data() + i, "[Chest/", 7) == 0 ||
                memcmp(payload.data() + i, "[Door/", 6) == 0 ||
                memcmp(payload.data() + i, "[Entity/", 8) == 0)) {
                stashEnd = i;
                break;
            }
        }
        scanBlock(stashPos, stashEnd, 2);
    }

    *outRetrieved = count;
    return KCD2_OK;
}


KCD2_Result SaveSession::SetInventoryItem(const KCD2_InventoryItem* item) {
    if (!item) return KCD2_ERR_INVALID_ARGUMENT;

    auto& payload = m_container.GetPayload();
    size_t offset = static_cast<size_t>(item->unique_id);
    if (offset + 26 > payload.size()) return KCD2_ERR_INVALID_ARGUMENT;

    uint8_t flags = payload[offset + 16];
    if (memcmp(payload.data() + offset + 17, "\x00\x04\x00\x00\x00", 5) != 0) {
        return KCD2_ERR_PARSING_FAILED;
    }

    std::string guidStr = FormatGuid(payload.data() + offset);
    if (guidStr == "5ef63059-322e-4e1b-abe8-926e100c770e") {
        uint32_t newAmt = static_cast<uint32_t>(std::clamp((int64_t)item->amount * 10, (int64_t)0, (int64_t)UINT32_MAX));
        memcpy(payload.data() + offset + 22, &newAmt, 4);
        m_cachedStats.groschen = item->amount;
    } else {
        if (flags == 0) {
            uint32_t newAmt = static_cast<uint32_t>(std::clamp(item->amount, 0, (int)INT32_MAX));
            memcpy(payload.data() + offset + 22, &newAmt, 4);
        } else if (flags == 1) {
            float cond = std::clamp(item->condition, 0.0f, 1.0f);
            memcpy(payload.data() + offset + 22, &cond, 4);
        }
    }

    return KCD2_OK;
}

KCD2_Result SaveSession::SetHorse(const char* horseId) {
    if (!horseId) return KCD2_ERR_INVALID_ARGUMENT;

    std::string curName;
    size_t curVitalsPos = std::string::npos;
    size_t curInvPos = std::string::npos;

    if (!FindHorseEntity(curName, curVitalsPos, curInvPos) || curName.empty()) {
        return KCD2_ERR_NOT_INITIALIZED;
    }

    if (curName == horseId) {
        return KCD2_OK;
    }

    auto& payload = m_container.GetPayload();
    std::string oldEntity = curName;
    std::string newEntity = horseId;

    size_t pos = 0;
    while (pos + oldEntity.size() <= payload.size()) {
        if (memcmp(payload.data() + pos, oldEntity.data(), oldEntity.size()) == 0) {
            bool prevOk = (pos == 0) || (!isalnum((unsigned char)payload[pos - 1]) && payload[pos - 1] != '_');
            bool nextOk = (pos + oldEntity.size() >= payload.size()) || (!isalnum((unsigned char)payload[pos + oldEntity.size()]) && payload[pos + oldEntity.size()] != '_');

            if (prevOk && nextOk) {
                if (pos >= 2 && payload[pos - 2] == (uint8_t)oldEntity.size() && payload[pos - 1] == 0) {
                    uint16_t nLen = static_cast<uint16_t>(newEntity.size());
                    memcpy(payload.data() + pos - 2, &nLen, 2);
                }
                payload.erase(payload.begin() + pos, payload.begin() + pos + oldEntity.size());
                payload.insert(payload.begin() + pos, newEntity.begin(), newEntity.end());
                pos += newEntity.size();
                continue;
            }
        }
        pos++;
    }

    m_horseStatsCached = false;
    m_horseEntityName.clear();
    m_horseVitalsPos = std::string::npos;
    m_horseInvPos = std::string::npos;

    return KCD2_OK;
}

KCD2_Result SaveSession::GetAllQuests(char* buffer, uint32_t bufferSize, uint32_t* outSize) const {
    if (!outSize) return KCD2_ERR_INVALID_ARGUMENT;

    const auto& payload = m_container.GetPayload();
    if (payload.empty()) {
        *outSize = 1;
        if (buffer && bufferSize > 0) buffer[0] = '\0';
        return KCD2_OK;
    }

    const char rStartTag[] = "<Roots>";
    const char rEndTag[] = "</Roots>";
    auto sIt = std::search(payload.begin(), payload.end(), rStartTag, rStartTag + 7);
    auto eIt = std::search(payload.begin(), payload.end(), rEndTag, rEndTag + 8);
    if (sIt == payload.end() || eIt == payload.end() || sIt >= eIt) {
        *outSize = 1;
        if (buffer && bufferSize > 0) buffer[0] = '\0';
        return KCD2_OK;
    }

    // 1. Extract UniqueQuestsCompleted from binary statistics block
    std::unordered_set<std::string> completedQuests;
    const char uqcTag[] = "UniqueQuestsCompleted";
    auto uqcIt = std::search(payload.begin(), payload.end(), uqcTag, uqcTag + strlen(uqcTag) + 1);
    if (uqcIt != payload.end()) {
        size_t uqcPos = std::distance(payload.begin(), uqcIt) + strlen(uqcTag) + 1;
        if (uqcPos + 10 <= payload.size() && (uint8_t)payload[uqcPos] == 0xa4 && (uint8_t)payload[uqcPos + 1] == 0x37) {
            uint32_t count = 0;
            memcpy(&count, payload.data() + uqcPos + 6, 4);
            size_t strPos = uqcPos + 10;
            for (uint32_t i = 0; i < count && strPos < payload.size(); ++i) {
                auto nulIt = std::find(payload.begin() + strPos, payload.end(), '\0');
                if (nulIt != payload.end()) {
                    std::string qName(payload.begin() + strPos, nulIt);
                    std::transform(qName.begin(), qName.end(), qName.begin(), ::tolower);
                    completedQuests.insert(qName);
                    strPos = std::distance(payload.begin(), nulIt) + 1;
                }
            }
        }
    }

    // 2. Extract active quest from manifest XML (e.g., @qname_poslednipomazani_1DR8 -> poslednipomazani)
    std::string manifestActiveQuest;
    const std::string& manifest = m_container.GetManifestXml();
    const std::string qnameMarker = "@qname_";
    auto qnPos = manifest.find(qnameMarker);
    if (qnPos != std::string::npos) {
        size_t nameStart = qnPos + qnameMarker.size();
        size_t underPos = manifest.find('_', nameStart);
        if (underPos != std::string::npos && underPos - nameStart < 64) {
            manifestActiveQuest = manifest.substr(nameStart, underPos - nameStart);
            std::transform(manifestActiveQuest.begin(), manifestActiveQuest.end(), manifestActiveQuest.begin(), ::tolower);
        }
    }

    std::string result;
    const std::vector<std::string> knownQuests = {
        "_posledniPomazani", "_prepadeni",
        "_sedmStatecnych", "_sedmStatecnych2", "_svatba", "_korenarkaZachrana",
        "_tarasMura", "_zaby", "_kumaniNaTrosecku", "_mysi1", "_poklad",
        "_poustevnik", "_sermiri", "_utokNaNebakov", "_nebakovObrana",
        "_nebakovPruzkum", "_mucirna", "_bohutovaVlozka", "_zbranePanaSemina",
        "_fightClubZelejov", "_kubaParalu", "_socky", "_plesnivec",
        "_drak", "_falesnaPosadka", "_viktorka", "_relikvie", "_stareKosti",
        "_spizovaciOddil", "_poPytlackeStezce"
    };

    const char* patterns[] = {
        "<_finalAnointning", "<_finalAnointing",
        "<_questProgress", "<_QuestProgress", "<_mainQuestProgress",
        "<_questProgressNebakovObrana", "<_kocovnickaCest_questProgress",
        "<_kumaniNaTrosecku_questProgress", "<_questProgressImpostor",
        "<_wakeAndBashProgress", "<_questAvailable"
    };

    for (const auto& qId : knownQuests) {
        std::string cleanId = qId;
        while (!cleanId.empty() && cleanId[0] == '_') cleanId.erase(0, 1);
        std::string cleanLower = cleanId;
        std::transform(cleanLower.begin(), cleanLower.end(), cleanLower.begin(), ::tolower);

        std::string qStatus;

        // Check if definitively completed in binary statistics block
        if (completedQuests.count(cleanLower) > 0) {
            qStatus = "Done";
        } else {
            std::string tag = "<" + qId;
            auto qIt = std::search(sIt, eIt, tag.begin(), tag.end());
            if (qIt == eIt) {
                std::string tagLower = "<";
                for (char c : qId) tagLower += (char)::tolower(c);
                for (auto it = sIt; it != eIt; ++it) {
                    if (*it == '<') {
                        bool match = true;
                        auto tit = tagLower.begin();
                        for (auto cIt = it; tit != tagLower.end() && cIt != eIt; ++tit, ++cIt) {
                            if (::tolower(*cIt) != *tit) { match = false; break; }
                        }
                        if (match && tit == tagLower.end()) {
                            auto nextIt = it + tagLower.size();
                            if (nextIt != eIt && (::isspace(*nextIt) || *nextIt == '>' || *nextIt == '/')) {
                                qIt = it;
                                break;
                            }
                        }
                    }
                }
            }

            if (qIt != eIt) {
                size_t qPos = std::distance(payload.begin(), qIt);
                std::string closeTag = "</" + qId + ">";
                auto closeIt = std::search(qIt, eIt, closeTag.begin(), closeTag.end());
                size_t limit = (closeIt != eIt) ? std::distance(payload.begin(), closeIt) : std::min(payload.size(), qPos + 4000);

                for (const auto* pat : patterns) {
                    std::string pStr(pat);
                    auto pIt = std::search(payload.begin() + qPos, payload.begin() + limit, pStr.begin(), pStr.end());
                    if (pIt != payload.begin() + limit) {
                        std::string valAttr = "value=\"";
                        auto vIt = std::search(pIt, std::min(payload.begin() + limit, pIt + 120), valAttr.begin(), valAttr.end());
                        if (vIt != std::min(payload.begin() + limit, pIt + 120)) {
                            size_t vStart = std::distance(payload.begin(), vIt) + valAttr.size();
                            auto qEndIt = std::find(payload.begin() + vStart, std::min(payload.begin() + limit, payload.begin() + vStart + 32), '"');
                            if (qEndIt != std::min(payload.begin() + limit, payload.begin() + vStart + 32)) {
                                std::string rawVal(payload.begin() + vStart, qEndIt);
                                std::string rawLower = rawVal;
                                std::transform(rawLower.begin(), rawLower.end(), rawLower.begin(), ::tolower);
                                if (rawLower == "done" || rawLower == "completed" || rawLower == "finished") {
                                    qStatus = "Done";
                                } else if (rawLower == "active" || rawLower == "running" || rawLower == "started") {
                                    qStatus = "Active";
                                } else if (rawLower == "failed" || rawLower == "aborted") {
                                    qStatus = "Failed";
                                } else if (rawLower == "available" || rawLower == "available_nomarker") {
                                    qStatus = "Available";
                                } else {
                                    qStatus = rawVal;
                                }
                                break;
                            }
                        }
                    }
                }

                if (qStatus.empty()) {
                    if (!manifestActiveQuest.empty() && manifestActiveQuest == cleanLower) {
                        qStatus = "Active";
                    } else {
                        qStatus = "NotStarted";
                    }
                }
            } else {
                if (!manifestActiveQuest.empty() && manifestActiveQuest == cleanLower) {
                    qStatus = "Active";
                } else {
                    qStatus = "NotStarted";
                }
            }
        }

        if (!result.empty()) result += ";";
        result += qId + "=" + qStatus;
    }

    *outSize = static_cast<uint32_t>(result.size() + 1);
    if (!buffer) return KCD2_OK;
    if (bufferSize < *outSize) return KCD2_ERR_BUFFER_TOO_SMALL;

    memcpy(buffer, result.c_str(), *outSize);
    return KCD2_OK;
}

KCD2_Result SaveSession::SetQuestStatus(const char* questId, const char* status) {
    if (!questId || !status) return KCD2_ERR_INVALID_ARGUMENT;

    auto& payload = m_container.GetPayload();
    if (payload.empty()) return KCD2_ERR_NOT_INITIALIZED;

    std::string tag = std::string("<") + questId;
    auto it = std::search(payload.begin(), payload.end(), tag.begin(), tag.end());
    if (it == payload.end()) {
        std::string tagLower = "<";
        for (const char* p = questId; *p; ++p) tagLower += (char)::tolower(*p);
        for (auto iter = payload.begin(); iter != payload.end(); ++iter) {
            if (*iter == '<') {
                bool match = true;
                auto tit = tagLower.begin();
                for (auto cIt = iter; tit != tagLower.end() && cIt != payload.end(); ++tit, ++cIt) {
                    if (::tolower(*cIt) != *tit) { match = false; break; }
                }
                if (match && tit == tagLower.end()) {
                    auto nextIt = iter + tagLower.size();
                    if (nextIt != payload.end() && (::isspace(*nextIt) || *nextIt == '>' || *nextIt == '/')) {
                        it = iter;
                        break;
                    }
                }
            }
        }
    }
    if (it == payload.end()) return KCD2_ERR_PARSING_FAILED;

    size_t questPos = std::distance(payload.begin(), it);
    std::string closeTag = "</" + std::string(questId) + ">";
    auto closeIt = std::search(it, payload.end(), closeTag.begin(), closeTag.end());
    size_t searchLimit = (closeIt != payload.end()) ? std::distance(payload.begin(), closeIt) : std::min(payload.size(), questPos + 4000);

    const char* patterns[] = {
        "<_finalAnointning", "<_finalAnointing",
        "<_questProgress", "<_QuestProgress", "<_mainQuestProgress",
        "<_questProgressNebakovObrana", "<_kocovnickaCest_questProgress",
        "<_kumaniNaTrosecku_questProgress", "<_questProgressImpostor",
        "<_wakeAndBashProgress", "<_questAvailable"
    };

    size_t progPos = std::string::npos;
    for (const auto* pat : patterns) {
        std::string pStr(pat);
        auto pIt = std::search(payload.begin() + questPos, payload.begin() + searchLimit, pStr.begin(), pStr.end());
        if (pIt != payload.begin() + searchLimit) {
            progPos = std::distance(payload.begin(), pIt);
            break;
        }
    }

    if (progPos == std::string::npos) return KCD2_ERR_PARSING_FAILED;

    std::string valAttr = "value=\"";
    auto valIt = std::search(payload.begin() + progPos, payload.begin() + std::min(payload.size(), progPos + 120), valAttr.begin(), valAttr.end());
    if (valIt == payload.begin() + std::min(payload.size(), progPos + 120)) return KCD2_ERR_PARSING_FAILED;

    size_t valStart = std::distance(payload.begin(), valIt) + valAttr.size();
    auto quoteIt = std::find(payload.begin() + valStart, payload.begin() + std::min(payload.size(), valStart + 32), '"');
    if (quoteIt == payload.begin() + std::min(payload.size(), valStart + 32)) return KCD2_ERR_PARSING_FAILED;

    size_t valEnd = std::distance(payload.begin(), quoteIt);

    std::string newStatus = status;
    if (newStatus == "aktiv") newStatus = "Active";
    else if (newStatus == "abgeschlossen") newStatus = "Done";
    else if (newStatus == "fehlgeschlagen") newStatus = "Failed";

    payload.erase(payload.begin() + valStart, payload.begin() + valEnd);
    payload.insert(payload.begin() + valStart, newStatus.begin(), newStatus.end());

    return KCD2_OK;
}

} // namespace KCD2
