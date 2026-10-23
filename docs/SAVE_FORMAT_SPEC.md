# Kingdom Come: Deliverance 2 — Save File (.whs) Format Specification

> **Status**: Verified by binary analysis of official KCD2 save files (`save008.whs`, etc.).

---

## 1. High-Level Container Layout

A KCD2 save file (`.whs`) is a structured binary container consisting of four primary sections:

```
+-------------------------------------------------------------------+
| Offset                | Size       | Description                  |
|-----------------------|------------|------------------------------|
| 0x00000000            | 4 bytes    | Magic Marker (0xFFFFFFFF)    |
| 0x00000004            | 4 bytes    | XML Header Length (uint32_t) |
| 0x00000008            | N bytes    | XML Manifest UTF-8 String    |
| 0x00000008 + N        | M bytes    | Stream of Zlib Chunks (32KB) |
| FileSize - 64         | 64 bytes   | Checksum & Trailer Block     |
+-------------------------------------------------------------------+
```

---

## 2. Section Details

### 2.1 File Header & Manifest XML
* **Magic Marker**: `FF FF FF FF` (`-1` as signed 32-bit int).
* **Length**: `uint32_t` little-endian integer indicating byte count of the XML string.
* **Manifest XML**: Clean UTF-8 plaintext XML `<C_SaveGameDescription>`.
  - Attributes:
    - `FormatVersion`: Save schema version (e.g. `"0"`).
    - `SaveType`: `"ManualSave"`, `"AutoSave"`, etc.
    - `SaveId`: Sequential numeric ID.
    - `SaveTime`: Unix timestamp (e.g. `1739117311`).
    - `LevelName`: World region name (e.g. `"trosecko"`).
    - `PlayerId`: ID of the player entity (`"0"`).
    - `UIDescription`: Formatted string for the in-game load menu (`"2|8|@qname_zachrana_FbKt||ui_countryside|1739117311|09/02/2025 16:08|1.572715|"`).
    - `BuildInfo`: Engine build string (e.g. `"1.1.1-11377-release_1_1"`).
    - `AssemblyDate`: Build compilation date (e.g. `"2025-01-27"`).
    - `GameReleaseVersion`: Version code (e.g. `"10101"`).
    - `FileName`: Target filename (e.g. `"save008.whs"`).
  - Child Tags:
    - `<DLCs>`: List of enabled DLCs (`<C_DLCDescription Id="QuestForValor" />`, etc.).

---

### 2.2 ZLIB Chunk Stream
Immediately following the last byte of the XML string, the file is divided into consecutive chunks.

Each chunk has an 8-byte header followed by compressed payload:
```cpp
struct WhsChunkHeader {
    uint32_t compressed_size;   // Byte size of following zlib stream
    uint32_t uncompressed_size; // Size after decompression (standard: 32768 = 32KB)
};
```
* **Compression**: Standard RFC 1950 ZLIB Deflate.
  - Byte 0: `0x78` (Deflate, 32K window).
  - Byte 1: `0x5E` (Check flags matching `(0x785E) % 31 == 0`).
  - Trailing 4 bytes of stream: Standard Adler-32 checksum.
* Total decompressed payload across all chunks combines into the full game state binary stream (~3.5 MB to ~4 MB).

---

### 2.3 Trailer & Integrity Block (Last 64 Bytes)
The last 64 bytes of every `.whs` file contain the integrity block:

```cpp
struct WhsTrailer {
    char magic[4];          // "0XBP" (0x50425830 in little endian)
    uint8_t md5[16];        // 128-bit MD5 checksum
    uint8_t reserved[44];   // Null padding (0x00)
};
```
* **Magic**: `0x30, 0x58, 0x42, 0x50` (`"0XBP"`).
* **MD5 Checksum (Verified 100%)**:
  The game validates integrity using MD5 calculated over the **entire file content up to the trailer, concatenated with the trailer where bytes 4..20 are zeroed**:
  ```python
  zeroed_trailer = trailer[:4] + b'\x00' * 16 + trailer[20:]
  calculated_md5 = hashlib.md5(file_data[:-64] + zeroed_trailer).digest()
  assert calculated_md5 == trailer[4:20]
  ```
  *Tested and bit-perfectly verified against `save008.whs`, `save017.whs`, `save026.whs`, `autosave289.whs`, `permanent001.whs`, and `exit.whs`.*
* **Reserved**: 44 bytes filled with `0x00`.

---

## 3. Decompressed Payload Architecture

The combined uncompressed stream (~3.7 MB to 4.5 MB) is a structured hybrid serialization:

### 3.1 Region 1: Stream Header (Bytes 0 to 293)
* Binary prefix containing map name (`trosecko`), save UI description (`2|8|@qname_...`), active DLC descriptors (`ui_dlc_quest_for_valor`, `ui_dlc_season_pass_shields`, `ui_dlc_gold_edition_huntsman`), and `ConceptState`.

### 3.2 Region 2: World State & Quest Progress (Hierarchical XML)
* Starts at offset 293 with `<Roots>`.
* **Human-readable plaintext XML** containing quest state machines, dialogue barks, and scene variables.
* Character node tags:
  - `<_jindrich>` (Henry - Jindřich in Czech)
  - `<_JanPtacek>` (Lord Hans Capon)
  - `<_zizka>` (Jan Žižka)
  - `<_vybaveni_pro_jindricha>` (Henry's equipment & quest items)

### 3.3 Region 3: CryEngine `TSerialize` Statistics Table (`0x79 0x17` Prefix)
* Key-value table of player lifetime statistics and economy:
  - `MaxMoneyOwned`: Highest groschen held.
  - `MoneyCollected`: Total groschen acquired.
  - `LocksPicked`, `LockpicksBroken`: Lockpicking stats.
  - `PotionsConsumed`, `PotionsBrewed`, `PoisonousHerbsCollected`: Alchemy stats.
  - `HumansKilled`, `HumansKilledWithCrossbowLight`, `HumansKilledWithRifle`: Combat kill breakdown.

### 3.4 Region 4: Character Actor RPG Schema
* Serialization nodes defining Henry's active stats:
  - **Base Attributes**: `strength._`, `agility._`, `vitality._`, `speech._`
  - **Secondary Stats**: `barter._`, `courage._`
  - **Vitals**: `health._`, `toHeal._`, `stamina._`, `vigour._` (Energy), `food._` (Nourishment)
  - **Sensory**: `vision._`, `hearing._`
  - **Social**: `reputation._`, `faction._`
