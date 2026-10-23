#pragma once

#include "../../include/kcd2_save_api.h"
#include "../container/WhsContainer.hpp"
#include <string>
#include <memory>

namespace KCD2 {

class SaveSession {
public:
    SaveSession();
    ~SaveSession() = default;

    bool Open(const std::string& filePath);
    bool Write(const std::string& destPath, bool createBackup);

    KCD2_Result GetMetadata(KCD2_SaveMetadata* outMeta) const;
    KCD2_Result GetPlayerStats(KCD2_PlayerStats* outStats) const;
    KCD2_Result SetPlayerStats(const KCD2_PlayerStats* inStats);

    KCD2_Result GetInventoryCount(uint32_t* outCount) const;
    KCD2_Result GetInventoryItems(KCD2_InventoryItem* items, uint32_t maxCount, uint32_t* outRetrieved) const;
    KCD2_Result SetInventoryItem(const KCD2_InventoryItem* item);

    KCD2_Result GetHorseStats(KCD2_HorseStats* outStats) const;
    KCD2_Result SetHorseStats(const KCD2_HorseStats* inStats);
    KCD2_Result SetHorse(const char* horseId);

    KCD2_Result GetAllQuests(char* buffer, uint32_t bufferSize, uint32_t* outSize) const;
    KCD2_Result SetQuestStatus(const char* questId, const char* status);

    bool ReadUint32Property(const std::string& propName, uint32_t& outVal) const;
    bool WriteUint32Property(const std::string& propName, uint32_t newVal);

    const std::string& GetManifestXml() const { return m_container.GetManifestXml(); }
    void SetManifestXml(const std::string& xml) { m_container.SetManifestXml(xml); }

    const std::string& GetLastError() const { return m_lastError; }

private:
    WhsContainer m_container;
    std::string m_lastError;

    // Helper functions for reading/writing binary typed properties
    bool ReadFloatProperty(const std::string& propName, float& outVal) const;
    bool WriteFloatProperty(const std::string& propName, float newVal);

    bool ReadInventoryGroschen(int64_t& outGroschen) const;
    bool WriteInventoryGroschen(int64_t newGroschen);

    bool FindHorseEntity(std::string& outName, size_t& outVitalsPos, size_t& outInvPos) const;

    mutable KCD2_PlayerStats m_cachedStats{};
    mutable bool m_statsCached{false};

    mutable KCD2_HorseStats m_cachedHorseStats{};
    mutable bool m_horseStatsCached{false};
    mutable std::string m_horseEntityName{};
    mutable size_t m_horseVitalsPos{std::string::npos};
    mutable size_t m_horseInvPos{std::string::npos};
};

} // namespace KCD2
