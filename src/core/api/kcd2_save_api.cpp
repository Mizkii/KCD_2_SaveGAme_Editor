#ifndef KCD2_CORE_EXPORTS
#define KCD2_CORE_EXPORTS
#endif
#include "../../include/kcd2_save_api.h"
#include "../model/SaveSession.hpp"
#include <string>

static thread_local std::string g_lastErrorMessage;

static void SetLastError(const std::string& msg) {
    g_lastErrorMessage = msg;
}

extern "C" {

KCD2_API KCD2_Result kcd2_save_open(const char* file_path, kcd2_save_handle_t* out_handle) {
    if (!file_path || !out_handle) {
        SetLastError("Invalid null arguments provided to kcd2_save_open");
        return KCD2_ERR_INVALID_ARGUMENT;
    }

    auto session = new (std::nothrow) KCD2::SaveSession();
    if (!session) {
        SetLastError("Out of memory allocating SaveSession");
        return KCD2_ERR_UNKNOWN;
    }

    if (!session->Open(file_path)) {
        SetLastError(session->GetLastError());
        delete session;
        *out_handle = nullptr;
        return KCD2_ERR_FILE_READ_FAILED;
    }

    *out_handle = reinterpret_cast<kcd2_save_handle_t>(session);
    return KCD2_OK;
}

KCD2_API KCD2_Result kcd2_save_write(kcd2_save_handle_t handle, const char* dest_file_path, int create_backup) {
    if (!handle) {
        SetLastError("Invalid null handle passed to kcd2_save_write");
        return KCD2_ERR_INVALID_ARGUMENT;
    }

    auto session = reinterpret_cast<KCD2::SaveSession*>(handle);
    std::string dest = dest_file_path ? dest_file_path : "";

    if (!session->Write(dest, create_backup != 0)) {
        SetLastError(session->GetLastError());
        return KCD2_ERR_FILE_WRITE_FAILED;
    }

    return KCD2_OK;
}

KCD2_API void kcd2_save_close(kcd2_save_handle_t handle) {
    if (handle) {
        auto session = reinterpret_cast<KCD2::SaveSession*>(handle);
        delete session;
    }
}

KCD2_API KCD2_Result kcd2_save_get_metadata(kcd2_save_handle_t handle, KCD2_SaveMetadata* out_metadata) {
    if (!handle || !out_metadata) {
        SetLastError("Invalid arguments to kcd2_save_get_metadata");
        return KCD2_ERR_INVALID_ARGUMENT;
    }

    auto session = reinterpret_cast<KCD2::SaveSession*>(handle);
    return session->GetMetadata(out_metadata);
}

KCD2_API KCD2_Result kcd2_save_get_manifest_xml(kcd2_save_handle_t handle, char* buffer, uint32_t buffer_size, uint32_t* out_size) {
    if (!handle || !out_size) {
        SetLastError("Invalid arguments to kcd2_save_get_manifest_xml");
        return KCD2_ERR_INVALID_ARGUMENT;
    }

    auto session = reinterpret_cast<KCD2::SaveSession*>(handle);
    const std::string& xml = session->GetManifestXml();
    *out_size = static_cast<uint32_t>(xml.size() + 1);

    if (!buffer) {
        return KCD2_OK;
    }

    if (buffer_size < *out_size) {
        SetLastError("Buffer too small for manifest XML");
        return KCD2_ERR_BUFFER_TOO_SMALL;
    }

    memcpy(buffer, xml.c_str(), *out_size);
    return KCD2_OK;
}

KCD2_API KCD2_Result kcd2_save_set_manifest_xml(kcd2_save_handle_t handle, const char* xml) {
    if (!handle || !xml) {
        SetLastError("Invalid arguments to kcd2_save_set_manifest_xml");
        return KCD2_ERR_INVALID_ARGUMENT;
    }

    auto session = reinterpret_cast<KCD2::SaveSession*>(handle);
    session->SetManifestXml(xml);
    return KCD2_OK;
}

KCD2_API KCD2_Result kcd2_save_get_player_stats(kcd2_save_handle_t handle, KCD2_PlayerStats* out_stats) {
    if (!handle || !out_stats) {
        SetLastError("Invalid arguments to kcd2_save_get_player_stats");
        return KCD2_ERR_INVALID_ARGUMENT;
    }

    auto session = reinterpret_cast<KCD2::SaveSession*>(handle);
    return session->GetPlayerStats(out_stats);
}

KCD2_API KCD2_Result kcd2_save_set_player_stats(kcd2_save_handle_t handle, const KCD2_PlayerStats* in_stats) {
    if (!handle || !in_stats) {
        SetLastError("Invalid arguments to kcd2_save_set_player_stats");
        return KCD2_ERR_INVALID_ARGUMENT;
    }

    auto session = reinterpret_cast<KCD2::SaveSession*>(handle);
    return session->SetPlayerStats(in_stats);
}

KCD2_API KCD2_Result kcd2_save_get_horse_stats(kcd2_save_handle_t handle, KCD2_HorseStats* out_stats) {
    if (!handle || !out_stats) {
        SetLastError("Invalid arguments to kcd2_save_get_horse_stats");
        return KCD2_ERR_INVALID_ARGUMENT;
    }

    auto session = reinterpret_cast<KCD2::SaveSession*>(handle);
    return session->GetHorseStats(out_stats);
}

KCD2_API KCD2_Result kcd2_save_set_horse_stats(kcd2_save_handle_t handle, const KCD2_HorseStats* in_stats) {
    if (!handle || !in_stats) {
        SetLastError("Invalid arguments to kcd2_save_set_horse_stats");
        return KCD2_ERR_INVALID_ARGUMENT;
    }

    auto session = reinterpret_cast<KCD2::SaveSession*>(handle);
    return session->SetHorseStats(in_stats);
}

KCD2_API KCD2_Result kcd2_save_set_horse(kcd2_save_handle_t handle, const char* horse_id) {
    if (!handle || !horse_id) {
        SetLastError("Invalid arguments to kcd2_save_set_horse");
        return KCD2_ERR_INVALID_ARGUMENT;
    }

    auto session = reinterpret_cast<KCD2::SaveSession*>(handle);
    return session->SetHorse(horse_id);
}

KCD2_API KCD2_Result kcd2_save_get_all_quests(kcd2_save_handle_t handle, char* buffer, uint32_t buffer_size, uint32_t* out_size) {
    if (!handle || !out_size) {
        SetLastError("Invalid arguments to kcd2_save_get_all_quests");
        return KCD2_ERR_INVALID_ARGUMENT;
    }

    auto session = reinterpret_cast<KCD2::SaveSession*>(handle);
    return session->GetAllQuests(buffer, buffer_size, out_size);
}

KCD2_API KCD2_Result kcd2_save_set_quest_status(kcd2_save_handle_t handle, const char* quest_id, const char* status) {
    if (!handle || !quest_id || !status) {
        SetLastError("Invalid arguments to kcd2_save_set_quest_status");
        return KCD2_ERR_INVALID_ARGUMENT;
    }

    auto session = reinterpret_cast<KCD2::SaveSession*>(handle);
    return session->SetQuestStatus(quest_id, status);
}

KCD2_API KCD2_Result kcd2_save_get_inventory_count(kcd2_save_handle_t handle, uint32_t* out_count) {
    if (!handle || !out_count) return KCD2_ERR_INVALID_ARGUMENT;
    auto session = reinterpret_cast<KCD2::SaveSession*>(handle);
    return session->GetInventoryCount(out_count);
}

KCD2_API KCD2_Result kcd2_save_get_inventory_items(kcd2_save_handle_t handle, KCD2_InventoryItem* items, uint32_t max_count, uint32_t* out_retrieved) {
    if (!handle || !items || !out_retrieved) return KCD2_ERR_INVALID_ARGUMENT;
    auto session = reinterpret_cast<KCD2::SaveSession*>(handle);
    return session->GetInventoryItems(items, max_count, out_retrieved);
}

KCD2_API KCD2_Result kcd2_save_set_inventory_item(kcd2_save_handle_t handle, const KCD2_InventoryItem* item) {
    if (!handle || !item) return KCD2_ERR_INVALID_ARGUMENT;
    auto session = reinterpret_cast<KCD2::SaveSession*>(handle);
    return session->SetInventoryItem(item);
}

KCD2_API KCD2_Result kcd2_save_add_inventory_item(kcd2_save_handle_t handle, const KCD2_InventoryItem* new_item) {
    if (!handle || !new_item) return KCD2_ERR_INVALID_ARGUMENT;
    return KCD2_OK;
}

KCD2_API KCD2_Result kcd2_save_remove_inventory_item(kcd2_save_handle_t handle, uint64_t unique_id) {
    if (!handle) return KCD2_ERR_INVALID_ARGUMENT;
    return KCD2_OK;
}

KCD2_API KCD2_Result kcd2_save_get_uint32_prop(kcd2_save_handle_t handle, const char* prop_name, uint32_t* out_val) {
    if (!handle || !prop_name || !out_val) {
        SetLastError("Invalid arguments to kcd2_save_get_uint32_prop");
        return KCD2_ERR_INVALID_ARGUMENT;
    }
    auto session = reinterpret_cast<KCD2::SaveSession*>(handle);
    if (!session->ReadUint32Property(prop_name, *out_val)) {
        *out_val = 0;
        return KCD2_ERR_PARSING_FAILED;
    }
    return KCD2_OK;
}

KCD2_API KCD2_Result kcd2_save_set_uint32_prop(kcd2_save_handle_t handle, const char* prop_name, uint32_t new_val) {
    if (!handle || !prop_name) {
        SetLastError("Invalid arguments to kcd2_save_set_uint32_prop");
        return KCD2_ERR_INVALID_ARGUMENT;
    }
    auto session = reinterpret_cast<KCD2::SaveSession*>(handle);
    if (!session->WriteUint32Property(prop_name, new_val)) {
        return KCD2_ERR_PARSING_FAILED;
    }
    return KCD2_OK;
}

KCD2_API const char* kcd2_get_last_error_message(void) {
    return g_lastErrorMessage.c_str();
}

KCD2_API const char* kcd2_core_version(void) {
    return "1.0.0-kcd2-release";
}

} // extern "C"
