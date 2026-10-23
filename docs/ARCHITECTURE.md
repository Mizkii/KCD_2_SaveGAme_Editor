# Kingdom Come: Deliverance 2 — SaveGame Editor Architecture

## 1. System Overview

The KCD2 SaveGame Editor is split into two cleanly separated tiers:
1. **Core Engine (`KCD2SaveCore`)**: A high-performance, native C++20 dynamic library (`.dll`) responsible for container handling, decompression/recompression, binary and XML parsing, integrity verification, and byte manipulation.
2. **User Interface (`KCD2SaveEditor.UI`)**: A modern C# (.NET 8) desktop application (WPF or Avalonia UI) with the MVVM pattern, communicating with the core engine via a strongly-typed P/Invoke layer.

```
+-----------------------------------------------------------------------+
|                    KCD2SaveEditor.UI  (C# .NET 8)                     |
|                                                                       |
|  [Main Window]  [Player Tab]  [Inventory Tab]  [Perks Tab]  [Raw View]|
|         \              |             |              /                 |
|          +-------------+-------------+-------------+                  |
|                               |                                       |
|                    [MVVM ViewModels & Services]                       |
|                       - Auto-Save Backup Mgr                          |
|                       - Item Database Cache                           |
|                               |                                       |
|                    [KCD2SaveCoreInterop.cs]                           |
|                      (P/Invoke C# Wrapper)                            |
+-------------------------------|---------------------------------------+
                                |  (C ABI / DLL Boundary)
+-------------------------------v---------------------------------------+
|                   KCD2SaveCore.dll  (C++20 Native)                    |
|                                                                       |
|  [C API Interface] (extern "C" exported functions)                    |
|  - kcd2_open_save, kcd2_save_to_file, kcd2_free_save                  |
|  - kcd2_get_player_stats, kcd2_set_player_stats                        |
|  - kcd2_get_inventory, kcd2_set_item, kcd2_add_item                   |
|                                                                       |
|  [Container Subsystem]          [Serialization Subsystem]             |
|  - WhsArchive (Zip/Custom)      - BinaryXmlParser                     |
|  - Decompressor (Zlib/LZ4/Zstd) - RpgSerializer                       |
|                                 - InventorySerializer                 |
|                                                                       |
|  [Integrity & Checksum]                                               |
|  - Hardware CRC32 (SSE4.2/AVX)                                        |
|  - SHA256 / Adler / Custom Hash                                       |
+-----------------------------------------------------------------------+
```

---

## 2. Directory Structure

```
d:\Projects\KCD_2_SaveGAme_Editor\
|-- CMakeLists.txt                # Root CMake for C++ Core & Tests
|-- KCD2SaveEditor.sln            # Visual Studio Solution (.NET + C++)
|-- include/
|   `-- kcd2_save_api.h           # Public C ABI Header (C++ exports, C# imports)
|-- src/
|   |-- core/                     # C++20 Core Library (KCD2SaveCore)
|   |   |-- api/                  # Implementation of exported C functions
|   |   |-- container/            # Archive & stream management (.whs unpack/pack)
|   |   |-- crypto/               # Checksum & hash calculations
|   |   |-- parser/               # CryXml / Binary serialization visitor
|   |   `-- model/                # C++ domain models (Player, Item, Perk, Quest)
|   |-- cli/                      # C++ Headless CLI tool for rapid testing/scripting
|   |   `-- main.cpp
|   `-- ui/                       # C# .NET 8 UI Project (KCD2SaveEditor.UI)
|       |-- App.xaml
|       |-- Interop/              # P/Invoke bindings to KCD2SaveCore.dll
|       |-- ViewModels/           # MVVM ViewModels
|       |-- Views/                # XAML Views & Dialogs
|       `-- Services/             # Backup manager, Path detector, Settings
|-- docs/                         # Specifications & Reverse Engineering Notes
|   |-- ARCHITECTURE.md
|   |-- IDA_REVERSING_PLAYBOOK.md
|   `-- SAVE_FORMAT_SPEC.md
|-- tests/                        # Unit tests for parser, roundtrip packing, CRC
`-- tools/                        # Python / IDA scripts for asset and RTTI extraction
```

---

## 3. Data Integrity & Safety Invariants

1. **Immutable Original Files**: The original `.whs` file is never overwritten directly in-place without creating a timestamped backup first:
   - Example: `savegame_backup_20261019_120530.whs.bak`.
2. **Round-Trip Parity**: Unpacking and immediately repacking a save without modifications must yield a bit-identical or functionally identical binary file.
3. **Checksum Recalculation**: Any edit automatically triggers recalculation of container and payload checksums prior to writing to disk.
4. **Boundary Validation**: Values such as Player Health, Skill Levels (typically capped at 20 or 30), and Groschen (clamped to `INT32_MAX`) are bounded in the C++ core before serialization.

---

## 4. C# / C++ Interoperability Design

* **Memory Management**: All heap allocations made inside C++ are freed by C++ (`kcd2_free_buffer`, `kcd2_close_save`). C# never directly frees raw pointers allocated by the C++ runtime.
* **String Encoding**: UTF-8 strings passed across the boundary (`const char*` in C++, `[MarshalAs(UnmanagedType.LPUTF8Str)]` in C#).
* **Return Codes**: Every API function returns a `KCD2_RESULT` integer status code (`0` for success, negative for specific errors).
