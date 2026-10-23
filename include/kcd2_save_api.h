/**
 * @file kcd2_save_api.h
 * @brief Public C ABI for Kingdom Come: Deliverance 2 SaveGame Core Library
 * 
 * Exposes a stable C API for consumption by C# (.NET 8) via P/Invoke
 * and standalone C/C++ tools.
 */

#ifndef KCD2_SAVE_API_H
#define KCD2_SAVE_API_H

#include <stdint.h>
#include <stddef.h>

#ifdef __cplusplus
extern "C" {
#endif

#if defined(_WIN32) || defined(__CYGWIN__)
    #ifdef KCD2_CORE_EXPORTS
        #define KCD2_API __declspec(dllexport)
    #else
        #define KCD2_API __declspec(dllimport)
    #endif
#else
    #define KCD2_API __attribute__((visibility("default")))
#endif

/* Result status codes */
typedef enum {
    KCD2_OK                       = 0,
    KCD2_ERR_INVALID_ARGUMENT     = -1,
    KCD2_ERR_FILE_NOT_FOUND       = -2,
    KCD2_ERR_FILE_READ_FAILED     = -3,
    KCD2_ERR_FILE_WRITE_FAILED    = -4,
    KCD2_ERR_INVALID_CONTAINER    = -5,
    KCD2_ERR_COMPRESSION_FAILED   = -6,
    KCD2_ERR_DECOMPRESSION_FAILED = -7,
    KCD2_ERR_CHECKSUM_MISMATCH    = -8,
    KCD2_ERR_PARSING_FAILED       = -9,
    KCD2_ERR_BUFFER_TOO_SMALL     = -10,
    KCD2_ERR_NOT_INITIALIZED      = -11,
    KCD2_ERR_UNKNOWN              = -99
} KCD2_Result;

/* Opaque pointer to savegame session */
typedef void* kcd2_save_handle_t;

/* Header / Metadata representation */
#pragma pack(push, 8)
typedef struct {
    char save_name[128];
    char playline_id[64];
    char game_version[32];
    char timestamp[32];
    uint32_t save_version;
    uint32_t character_level;
    uint64_t playtime_seconds;
} KCD2_SaveMetadata;

/* Core Player Attributes and Vitals */
typedef struct {
    /* Vitals */
    float health;
    float max_health;
    float stamina;
    float max_stamina;
    float energy;
    float nourishment;

    /* Economy */
    int64_t groschen; // Money in Tenths of Groschen or Standard Groschen

    /* Base Attributes */
    int32_t strength;
    int32_t agility;
    int32_t vitality;
    int32_t speech;
    int32_t charisma;
    int32_t speed;

    /* Character Level (Hauptstufe) */
    int32_t character_level;

    /* Stealth Ratings */
    int32_t conspicuousness;
    int32_t noise;
    int32_t visibility;

    /* Weight & Carry Capacity */
    float carry_weight;
    float max_carry_weight;

    /* Combat & Sub-Skills */
    int32_t warfare;
    int32_t sword;
    int32_t defense;
    int32_t stealth;
    int32_t lockpicking;
    int32_t pickpocketing;
    int32_t alchemy;
    int32_t marksmanship;
    int32_t horsemanship;
    int32_t houndmaster;
    int32_t scholarship;
    int32_t blacksmithing;
} KCD2_PlayerStats;

/* Horse Attributes and Vitals */
typedef struct {
    char horse_name[64];
    float health;
    float max_health;
    float stamina;
    float max_stamina;
    float speed;
    float capacity;
    float courage;
} KCD2_HorseStats;

/* Single Inventory Item */
typedef struct {
    char item_id[64];       // Database GUID or identifier string
    char item_ui_name[128]; // Mapped localized display name (from game db)
    uint64_t unique_id;     // Instance unique ID
    int32_t amount;         // Stack quantity
    float condition;        // 0.0f to 1.0f (or 0 - 100%)
    int32_t slot_id;        // Equipped slot or backpack indicator (>= 100 for horse items)
    uint32_t flags;         // Quest item, stolen, equipped, etc.
} KCD2_InventoryItem;
#pragma pack(pop)

/* --- Session Lifecycle --- */

/**
 * Opens and parses a KCD2 savegame (.whs) file.
 * @param file_path Path to the .whs save file (UTF-8).
 * @param out_handle Pointer to receive the initialized handle.
 * @return KCD2_OK on success, error code otherwise.
 */
KCD2_API KCD2_Result kcd2_save_open(const char* file_path, kcd2_save_handle_t* out_handle);

/**
 * Saves modifications back to disk, creating an automatic backup if requested.
 * @param handle Valid save handle.
 * @param dest_file_path Target file path (or NULL to overwrite source).
 * @param create_backup If true, creates a timestamped .bak file before writing.
 * @return KCD2_OK on success, error code otherwise.
 */
KCD2_API KCD2_Result kcd2_save_write(kcd2_save_handle_t handle, const char* dest_file_path, int create_backup);

/**
 * Closes the savegame handle and frees internal memory buffers.
 * @param handle Valid save handle.
 */
KCD2_API void kcd2_save_close(kcd2_save_handle_t handle);

/* --- Metadata & Headers --- */

/**
 * Retrieves the header metadata of the open save.
 */
KCD2_API KCD2_Result kcd2_save_get_metadata(kcd2_save_handle_t handle, KCD2_SaveMetadata* out_metadata);

/**
 * Retrieves the raw XML manifest from the save file header.
 * @param handle Valid save handle.
 * @param buffer Output buffer (allocated by caller), or NULL to query size.
 * @param buffer_size Size of the output buffer.
 * @param out_size Written or required string length including null-terminator.
 */
KCD2_API KCD2_Result kcd2_save_get_manifest_xml(kcd2_save_handle_t handle, char* buffer, uint32_t buffer_size, uint32_t* out_size);

/**
 * Updates the raw XML manifest in the save file header.
 * @param handle Valid save handle.
 * @param xml Null-terminated UTF-8 XML manifest string.
 */
KCD2_API KCD2_Result kcd2_save_set_manifest_xml(kcd2_save_handle_t handle, const char* xml);

/* --- Player Stats & Attributes --- */

/**
 * Reads Henry's core attributes, vitals, and groschen.
 */
KCD2_API KCD2_Result kcd2_save_get_player_stats(kcd2_save_handle_t handle, KCD2_PlayerStats* out_stats);

/**
 * Updates Henry's core attributes, vitals, and groschen in memory.
 */
KCD2_API KCD2_Result kcd2_save_set_player_stats(kcd2_save_handle_t handle, const KCD2_PlayerStats* in_stats);

/* --- Horse Stats & Vitals --- */

/**
 * Reads Henry's horse attributes, vitals, and stats.
 */
KCD2_API KCD2_Result kcd2_save_get_horse_stats(kcd2_save_handle_t handle, KCD2_HorseStats* out_stats);

/**
 * Updates Henry's horse vitals (health, stamina) in memory.
 */
KCD2_API KCD2_Result kcd2_save_set_horse_stats(kcd2_save_handle_t handle, const KCD2_HorseStats* in_stats);

/**
 * Changes Henry's owned horse entity in the savegame.
 * @param horse_id Canonical horse entity ID (e.g. "tsem_sedivka", "kkut_horseForSale_4", "kgru_horseForSale_6").
 */
KCD2_API KCD2_Result kcd2_save_set_horse(kcd2_save_handle_t handle, const char* horse_id);

/* --- Quests Subsystem --- */

/**
 * Retrieves all quest progress states from the savegame XML.
 * Output format: "quest_id=status;quest_id=status;..."
 * @param buffer Output buffer (or NULL to query required size).
 * @param buffer_size Size of the output buffer.
 * @param out_size Written or required string length including null-terminator.
 */
KCD2_API KCD2_Result kcd2_save_get_all_quests(kcd2_save_handle_t handle, char* buffer, uint32_t buffer_size, uint32_t* out_size);

/**
 * Updates the progress status of a quest ("Active", "Done", "Failed").
 * @param quest_id Engine quest identifier (e.g. "_tarasMura", "_sedmStatecnych").
 * @param status Target status string ("Active", "Done", "Failed").
 */
KCD2_API KCD2_Result kcd2_save_set_quest_status(kcd2_save_handle_t handle, const char* quest_id, const char* status);

/* --- Inventory Subsystem --- */

/**
 * Retrieves the total count of inventory items in the save.
 */
KCD2_API KCD2_Result kcd2_save_get_inventory_count(kcd2_save_handle_t handle, uint32_t* out_count);

/**
 * Populates an array of inventory items.
 * @param handle Valid save handle.
 * @param items Buffer of KCD2_InventoryItem allocated by caller.
 * @param max_count Maximum items the buffer can hold.
 * @param out_retrieved Number of items written into the buffer.
 */
KCD2_API KCD2_Result kcd2_save_get_inventory_items(kcd2_save_handle_t handle, KCD2_InventoryItem* items, uint32_t max_count, uint32_t* out_retrieved);

/**
 * Updates a single item's amount, condition, or flags by its unique_id.
 */
KCD2_API KCD2_Result kcd2_save_set_inventory_item(kcd2_save_handle_t handle, const KCD2_InventoryItem* item);

/**
 * Adds a new item to Henry's inventory.
 */
KCD2_API KCD2_Result kcd2_save_add_inventory_item(kcd2_save_handle_t handle, const KCD2_InventoryItem* new_item);

/**
 * Removes an item from the inventory by its unique_id.
 */
KCD2_API KCD2_Result kcd2_save_remove_inventory_item(kcd2_save_handle_t handle, uint64_t unique_id);

/* --- Generic Properties & World State --- */

/**
 * Reads a 32-bit unsigned integer property from the savegame (e.g. "RaczekSwordReforged", "QuestsCompleted").
 */
KCD2_API KCD2_Result kcd2_save_get_uint32_prop(kcd2_save_handle_t handle, const char* prop_name, uint32_t* out_val);

/**
 * Writes a 32-bit unsigned integer property into the savegame (e.g. "RaczekSwordReforged").
 */
KCD2_API KCD2_Result kcd2_save_set_uint32_prop(kcd2_save_handle_t handle, const char* prop_name, uint32_t new_val);

/* --- Diagnostic & Error Handling --- */

/**
 * Returns a human-readable description of the last error encountered.
 */
KCD2_API const char* kcd2_get_last_error_message(void);

/**
 * Returns the version string of the KCD2SaveCore library.
 */
KCD2_API const char* kcd2_core_version(void);

#ifdef __cplusplus
}
#endif

#endif /* KCD2_SAVE_API_H */
