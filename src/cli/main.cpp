#include <iostream>
#include <iomanip>
#include <vector>
#include "kcd2_save_api.h"

void TestSave(const char* path) {
    std::cout << "\n====================================================\n";
    std::cout << "[+] Opening: " << path << "\n";
    kcd2_save_handle_t handle = nullptr;
    KCD2_Result res = kcd2_save_open(path, &handle);
    if (res != KCD2_OK) {
        std::cerr << "[-] Failed to open: " << kcd2_get_last_error_message() << "\n";
        return;
    }

    // Horse stats
    KCD2_HorseStats horse;
    if (kcd2_save_get_horse_stats(handle, &horse) == KCD2_OK) {
        std::cout << "--- Horse Stats ---\n";
        std::cout << "  Name:     " << horse.horse_name << "\n";
        std::cout << "  Health:   " << horse.health << " / " << horse.max_health << "\n";
        std::cout << "  Stamina:  " << horse.stamina << " / " << horse.max_stamina << "\n";
        std::cout << "  Speed:    " << horse.speed << " km/h\n";
        std::cout << "  Capacity: " << horse.capacity << " kg\n";
        std::cout << "  Courage:  " << horse.courage << "\n";
    }

    // Inventory items
    uint32_t count = 0;
    kcd2_save_get_inventory_count(handle, &count);
    std::cout << "\n--- Total Inventory Count: " << count << " ---\n";

    std::vector<KCD2_InventoryItem> items(count);
    uint32_t retrieved = 0;
    kcd2_save_get_inventory_items(handle, items.data(), count, &retrieved);
    std::cout << "[+] Retrieved " << retrieved << " items.\n";

    int horseItemCount = 0;
    std::cout << "\n--- Horse Items (Slot >= 100) ---\n";
    for (uint32_t i = 0; i < retrieved; ++i) {
        if (items[i].slot_id >= 100) {
            horseItemCount++;
            std::cout << "  #" << horseItemCount << " [Slot " << items[i].slot_id << "]: "
                      << items[i].item_ui_name
                      << " (" << items[i].item_id << ")"
                      << " amt=" << items[i].amount
                      << " cond=" << (int)(items[i].condition * 100) << "%\n";
        }
    }
    std::cout << "[+] Found " << horseItemCount << " horse items!\n";

    kcd2_save_close(handle);
}

void TestHorseRoundtrip() {
    std::cout << "\n====================================================\n";
    std::cout << "[+] Testing Horse Stats Roundtrip...\n";
    kcd2_save_handle_t handle = nullptr;
    if (kcd2_save_open("C:/Users/Mizkii/Downloads/kuttenberg-maksimalnye-kharakteristiki_1767132967_805988/KuttenbergSave/autosave178.whs", &handle) != KCD2_OK) {
        std::cerr << "[-] Failed to open test save\n";
        return;
    }

    KCD2_HorseStats hStats;
    kcd2_save_get_horse_stats(handle, &hStats);
    std::cout << "Original HP: " << hStats.health << "/" << hStats.max_health << "\n";

    hStats.health = 137.5f;
    hStats.max_health = 180.0f;
    kcd2_save_set_horse_stats(handle, &hStats);

    const char* testOut = "D:/Projects/KCD_2_SaveGAme_Editor/build/test_horse.whs";
    kcd2_save_write(handle, testOut, 0);
    kcd2_save_close(handle);

    // Reopen and verify
    kcd2_save_handle_t checkHandle = nullptr;
    if (kcd2_save_open(testOut, &checkHandle) == KCD2_OK) {
        KCD2_HorseStats readStats;
        kcd2_save_get_horse_stats(checkHandle, &readStats);
        std::cout << "Re-read HP: " << readStats.health << "/" << readStats.max_health << "\n";
        if (readStats.health == 137.5f && readStats.max_health == 180.0f) {
            std::cout << "[SUCCESS] Horse stats roundtrip verified!\n";
        } else {
            std::cout << "[FAILURE] Horse stats mismatch!\n";
        }
        kcd2_save_close(checkHandle);
    }
}

int main(int argc, char** argv) {
    if (argc > 1) {
        TestSave(argv[1]);
        return 0;
    }
    TestHorseRoundtrip();
    return 0;
}
