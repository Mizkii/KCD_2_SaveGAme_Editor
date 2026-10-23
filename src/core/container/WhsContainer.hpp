#pragma once

#include <string>
#include <vector>
#include <cstdint>

#include "../../include/kcd2_save_api.h"

namespace KCD2 {

constexpr uint32_t WHS_HEADER_MAGIC = 0xFFFFFFFF;
constexpr uint32_t WHS_TRAILER_MAGIC = 0x50425830; // "0XBP" in little endian
constexpr size_t WHS_CHUNK_UNCOMPRESSED_SIZE = 32768;
constexpr size_t WHS_TRAILER_SIZE = 64;

struct WhsTrailer {
    uint32_t magic = WHS_TRAILER_MAGIC;
    uint8_t md5[16] = {0};
    uint8_t reserved[44] = {0};
};

class KCD2_API WhsContainer {
public:
    WhsContainer();
    ~WhsContainer() = default;

    /**
     * Reads, verifies checksum, and decompresses all chunks of a .whs file.
     */
    bool LoadFromFile(const std::string& filePath, std::string& outError);

    /**
     * Recompresses payload into 32KB zlib chunks, re-calculates the MD5 trailer,
     * and writes the .whs file to disk.
     */
    bool SaveToFile(const std::string& filePath, bool createBackup, std::string& outError);

    const std::string& GetManifestXml() const { return m_manifestXml; }
    void SetManifestXml(const std::string& xml) { m_manifestXml = xml; }

    const std::vector<uint8_t>& GetPayload() const { return m_payload; }
    std::vector<uint8_t>& GetPayload() { return m_payload; }

    const std::string& GetSourcePath() const { return m_sourcePath; }

    /**
     * Computes the exact XMLCPB MD5 checksum according to the reverse-engineered specification.
     */
    static void ComputeMd5(const std::vector<uint8_t>& fileBody, const WhsTrailer& trailerTemplate, uint8_t outMd5[16]);

private:
    std::string m_sourcePath;
    std::string m_manifestXml;
    std::vector<uint8_t> m_payload;
    WhsTrailer m_trailer;
};

} // namespace KCD2
