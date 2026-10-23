# Kingdom Come: Deliverance 2 — IDA Pro Reversing Playbook

This document is the actionable step-by-step guide for analyzing the KCD2 executable (`KingdomCome.exe` or companion DLLs) in IDA Pro as soon as the game binaries are acquired.

---

## 1. Initial Binary Triage & Loading in IDA Pro

### 1.1 Ingestion Settings
* **Target Binary**: Primary game executable (typically `Bin/Win64MasterMasterSteamPGO/KingdomCome.exe` or `KingdomCome.exe`) and primary engine/game DLLs (`WHGame.dll`, `CryAction.dll`, `CrySystem.dll`).
* **Disassembler**: IDA Pro 7.7+ / 8.x / 9.x (x86_64 PE).
* **Compiler Mode**: Microsoft Visual C++ (`MSVC`).
* **Initial Analysis**:
  - Enable *Create function tails* and *Analyze and create all xrefs*.
  - Load PDB debug symbols if any are shipped or downloadable via symbol servers.

### 1.2 RTTI Reconstruction
CryEngine is deeply object-oriented and heavily relies on C++ RTTI (Run-Time Type Information).
1. Run **ClassInformer** plugin or modern `ida-rtti` script (`Ctrl + Alt + I` or `File -> Script file`).
2. Search the recovered VFTables / Class names for:
   * `*SaveGame*`
   * `*LoadGame*`
   * `*Serialize*`
   * `*WHGame*` / `*Warhorse*`
   * `*RPG*`
   * `*Inventory*`
   * `*CryPak*`

---

## 2. Key Target Strings & Anchors

Search the String Window (`Shift + F12`, search mode: Case Insensitive) for the following exact and substring patterns:

### 2.1 Save / Load Dispatchers
* `"SaveToFile"`
* `"LoadFromFile"`
* `"Saving game to '%s'..."`
* `"Loading game from '%s'..."`
* `"SaveGameProvider"`
* `".whs"`
* `"savegame.xml"` / `"header.xml"` / `"manifest.xml"`
* `"playline"`
* `"Corrupted savegame"`
* `"Savegame version mismatch"`

### 2.2 Console Commands
CryEngine registers engine console commands via `IConsole::AddCommand` / `Register`. Locate:
* `"save"`
* `"load"`
* `"save_game"`
* `"load_game"`
* `"save_auto"`
* `"sys_save_folder"`
* `"sys_saves_dir"`

*Workflow in IDA*:
1. Double-click the command string (e.g., `"save_game"`).
2. Press `X` to find xrefs to code.
3. Locate where it is passed to `IConsole::AddCommand(..., FunctionPointer, ...)` or a wrapper.
4. The function pointer is the engine's top-level save entry point.

### 2.3 File I/O & Container Packaging
CryEngine wraps all virtual file and archive I/O in `ICryPak`:
* Import table: Search for `CreateFileW`, `WriteFile`, `ReadFile`, `SetFilePointerEx`.
* CryPak symbols: `ICryPak::FOpen`, `ICryPak::FWrite`, `ICryPak::FRead`, `ICryPak::FSeek`, `ICryPak::FClose`.
* Archive magic & signatures:
  - Standard ZIP: `PK\x03\x04` (`0x04034B50`)
  - LZ4 Frame: `\x04\x22\x4D\x18` (`0x184D2204`)
  - Zstandard: `\x28\xB5\x2F\xFD` (`0xFD2FB528`)
  - Deflate / zlib: `0x789C`, `0x7801`, `0x78DA`

---

## 3. Serialization Protocol Analysis

### 3.1 The `TSerialize` Architecture
In CryEngine, serialization uses the `TSerialize` visitor pattern:
```cpp
struct ISerialize {
    virtual bool BeginGroup(const char* szName) = 0;
    virtual bool EndGroup() = 0;
    virtual void Value(const char* szName, int& value, int policy) = 0;
    virtual void Value(const char* szName, float& value, int policy) = 0;
    virtual void Value(const char* szName, string& value, int policy) = 0;
    // ...
};
```

1. Look for functions with repeated calls to string constants paired with struct offsets:
   - Example strings: `"Health"`, `"Stamina"`, `"Money"`, `"Groschen"`, `"Items"`, `"Buffs"`, `"Perks"`, `"PlayerStats"`.
2. Determine if the serializer creates:
   - **Plaintext XML**: Human readable tags.
   - **Binary XML (EBX / CryXml)**: Tokenized XML where tag names are indexed or hashed.
   - **Raw Binary Stream**: Direct binary write of primitive types.

---

## 4. Integrity, Hashes & Checksums

Game save files often contain validation mechanisms to prevent corrupted or edited saves from loading.

### 4.1 Checksum Algorithms to Identify
1. **CRC-32**:
   - Look for standard lookup table with generator `0xEDB88320` or `0x82F63B78` (Castagnoli).
   - Look for intrinsic instruction `_mm_crc32_u32` / `_mm_crc32_u64`.
2. **Adler-32 / Fletcher**:
   - Characteristic loop: modulo 65521 (`0xFFF1`).
3. **SHA-256 / MD5**:
   - Cryptographic initialization constants (e.g., SHA256: `0x6A09E667`, `0xBB67AE85`, `0x3C6EF372`).
4. **HMAC**:
   - Check if a static secret key or player SteamID is concatenated before hashing.

### 4.2 Finding the Verification Routine
1. In IDA, find the string `"Corrupted savegame"` or xrefs where `LoadFromFile` returns `false`.
2. Trace backwards from the branch (`jz` / `jnz` conditional jump) that triggers the error.
3. The comparison instruction (`cmp eax, [rbp+saved_crc]`) reveals:
   - Where the expected checksum is read from the file.
   - Which function computed the actual checksum over the memory buffer.
   - The start offset and length of the hashed buffer.

---

## 5. Binary Diffing Strategy for Live Saves

When two saves are available:
1. Make **Save 1**: Start game, note Henry's Groschen = $X$.
2. Make **Save 2**: Spend 1 Groschen, save immediately -> Groschen = $X - 1$.
3. Run container extraction (unzip / unpack).
4. Run hexadecimal diff (`fc /b`, Beyond Compare, or 010 Editor) on the extracted payload chunks:
   - Finds exact byte offset of player money.
   - Verifies endianness (little-endian 32-bit integer or 64-bit integer or float).

---

## 6. Post-Reversing Deliverables Checklist
- [ ] Container type documented (.zip, custom header, compression stream).
- [ ] Checksum algorithm, polynomial, and byte-range documented.
- [ ] Player stat chunk offset / XML tag schema documented.
- [ ] Inventory item structure (Item ID, count, health/durability) documented.
- [ ] C++ serialization functions modeled in `KCD2SaveCore`.
