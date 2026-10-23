#include "WhsContainer.hpp"
#include "../crypto/md5.h"
#include "../compression/miniz.h"

#include <fstream>
#include <sstream>
#include <iomanip>
#include <chrono>
#include <cstring>
#include <filesystem>

namespace fs = std::filesystem;

namespace KCD2 {

WhsContainer::WhsContainer() {
    memset(&m_trailer, 0, sizeof(m_trailer));
    m_trailer.magic = WHS_TRAILER_MAGIC;
}

void WhsContainer::ComputeMd5(const std::vector<uint8_t>& fileBody, const WhsTrailer& trailerTemplate, uint8_t outMd5[16]) {
    WhsTrailer zeroed = trailerTemplate;
    memset(zeroed.md5, 0, sizeof(zeroed.md5));

    KCD2_MD5_CTX ctx;
    kcd2_md5_init(&ctx);
    if (!fileBody.empty()) {
        kcd2_md5_update(&ctx, fileBody.data(), fileBody.size());
    }
    kcd2_md5_update(&ctx, reinterpret_cast<const uint8_t*>(&zeroed), sizeof(zeroed));
    kcd2_md5_final(&ctx, outMd5);
}

bool WhsContainer::LoadFromFile(const std::string& filePath, std::string& outError) {
    m_sourcePath = filePath;
    m_payload.clear();
    m_manifestXml.clear();

    std::ifstream file(filePath, std::ios::binary | std::ios::ate);
    if (!file.is_open()) {
        outError = "Failed to open save file: " + filePath;
        return false;
    }

    const std::streamsize fileSize = file.tellg();
    if (fileSize < static_cast<std::streamsize>(8 + WHS_TRAILER_SIZE)) {
        outError = "File too small to be a valid KCD2 savegame (.whs)";
        return false;
    }

    file.seekg(0, std::ios::beg);
    std::vector<uint8_t> fileBytes(static_cast<size_t>(fileSize));
    if (!file.read(reinterpret_cast<char*>(fileBytes.data()), fileSize)) {
        outError = "Failed to read save file into memory";
        return false;
    }

    // 1. Check Magic
    uint32_t magic = 0;
    memcpy(&magic, fileBytes.data(), 4);
    if (magic != WHS_HEADER_MAGIC) {
        outError = "Invalid KCD2 save magic marker (expected 0xFFFFFFFF)";
        return false;
    }

    // 2. Read XML Header Length
    uint32_t xmlLen = 0;
    memcpy(&xmlLen, fileBytes.data() + 4, 4);
    if (8 + xmlLen + WHS_TRAILER_SIZE > fileBytes.size()) {
        outError = "Corrupted header: XML length exceeds file bounds";
        return false;
    }

    // 3. Extract Manifest XML
    m_manifestXml.assign(reinterpret_cast<const char*>(fileBytes.data() + 8), xmlLen);
    while (!m_manifestXml.empty() && (m_manifestXml.back() == '\0' || m_manifestXml.back() == '\r' || m_manifestXml.back() == '\n' || m_manifestXml.back() == ' ')) {
        m_manifestXml.pop_back();
    }

    // 4. Read Trailer
    const size_t trailerOffset = fileBytes.size() - WHS_TRAILER_SIZE;
    memcpy(&m_trailer, fileBytes.data() + trailerOffset, sizeof(WhsTrailer));

    if (m_trailer.magic != WHS_TRAILER_MAGIC) {
        outError = "Invalid trailer magic (expected '0XBP' / 0x50425830)";
        return false;
    }

    // 5. Verify MD5 Integrity
    std::vector<uint8_t> fileBody(fileBytes.begin(), fileBytes.begin() + trailerOffset);
    uint8_t calculatedMd5[16];
    ComputeMd5(fileBody, m_trailer, calculatedMd5);

    if (memcmp(calculatedMd5, m_trailer.md5, 16) != 0) {
        // Warning: Checksum mismatch, but we log and proceed to allow recovering modified saves
        outError = "Warning: Checksum mismatch on load (file may have been modified externally)";
    }

    // 6. Decompress Chunks
    size_t pos = 8 + xmlLen;
    while (pos < trailerOffset) {
        if (pos + 8 > trailerOffset) {
            outError = "Unexpected trailing bytes before save trailer";
            return false;
        }

        uint32_t compSize = 0;
        uint32_t uncompSize = 0;
        memcpy(&compSize, fileBytes.data() + pos, 4);
        memcpy(&uncompSize, fileBytes.data() + pos + 4, 4);

        if (pos + 8 + compSize > trailerOffset) {
            outError = "Chunk compressed size overflows save body";
            return false;
        }

        const uint8_t* compData = fileBytes.data() + pos + 8;
        std::vector<uint8_t> uncompChunk(uncompSize);
        mz_ulong destLen = uncompSize;

        int status = mz_uncompress(uncompChunk.data(), &destLen, compData, compSize);
        if (status != MZ_OK || destLen != uncompSize) {
            outError = "ZLIB decompression error in chunk at offset " + std::to_string(pos);
            return false;
        }

        m_payload.insert(m_payload.end(), uncompChunk.begin(), uncompChunk.end());
        pos += 8 + compSize;
    }

    return true;
}

bool WhsContainer::SaveToFile(const std::string& filePath, bool createBackup, std::string& outError) {
    if (m_payload.empty()) {
        outError = "Cannot save: Payload is empty";
        return false;
    }

    // 1. Create Timestamped Backup if file exists
    if (createBackup && fs::exists(filePath)) {
        try {
            auto now = std::chrono::system_clock::now();
            auto in_time_t = std::chrono::system_clock::to_time_t(now);
            std::stringstream ss;
            ss << std::put_time(std::localtime(&in_time_t), "%Y%m%d_%H%M%S");
            std::string backupPath = filePath + ".bak_" + ss.str();
            fs::copy_file(filePath, backupPath, fs::copy_options::overwrite_existing);
        } catch (const std::exception& ex) {
            outError = std::string("Failed to create backup: ") + ex.what();
            return false;
        }
    }

    // 2. Assemble Header & Compressed Body
    std::string xmlToWrite = m_manifestXml;
    while (!xmlToWrite.empty() && (xmlToWrite.back() == '\0' || xmlToWrite.back() == '\r' || xmlToWrite.back() == '\n' || xmlToWrite.back() == ' ')) {
        xmlToWrite.pop_back();
    }
    xmlToWrite.push_back('\n');
    xmlToWrite.push_back('\0');

    std::vector<uint8_t> fileBody;
    fileBody.reserve(m_payload.size() / 2); // Pre-allocate sensible estimate

    // Magic
    uint32_t magic = WHS_HEADER_MAGIC;
    fileBody.insert(fileBody.end(), reinterpret_cast<uint8_t*>(&magic), reinterpret_cast<uint8_t*>(&magic) + 4);

    // XML Length (must include trailing \n and \0 as expected by CryEngine sub_1806ACD00)
    uint32_t xmlLen = static_cast<uint32_t>(xmlToWrite.size());
    fileBody.insert(fileBody.end(), reinterpret_cast<uint8_t*>(&xmlLen), reinterpret_cast<uint8_t*>(&xmlLen) + 4);

    // XML Manifest
    fileBody.insert(fileBody.end(), reinterpret_cast<const uint8_t*>(xmlToWrite.data()), reinterpret_cast<const uint8_t*>(xmlToWrite.data()) + xmlLen);

    // Compress in 32KB chunks
    const size_t totalPayload = m_payload.size();
    for (size_t offset = 0; offset < totalPayload; offset += WHS_CHUNK_UNCOMPRESSED_SIZE) {
        const size_t currentChunkSize = std::min(WHS_CHUNK_UNCOMPRESSED_SIZE, totalPayload - offset);
        const uint8_t* chunkSrc = m_payload.data() + offset;

        mz_ulong maxCompSize = mz_compressBound(static_cast<mz_ulong>(currentChunkSize));
        std::vector<uint8_t> compBuffer(maxCompSize);
        mz_ulong actualCompSize = maxCompSize;

        int status = mz_compress(compBuffer.data(), &actualCompSize, chunkSrc, static_cast<mz_ulong>(currentChunkSize));
        if (status != MZ_OK) {
            outError = "ZLIB compression error while packing chunk";
            return false;
        }

        // Write Chunk Header: comp_size, uncomp_size
        uint32_t cSize = static_cast<uint32_t>(actualCompSize);
        uint32_t uSize = static_cast<uint32_t>(currentChunkSize);
        fileBody.insert(fileBody.end(), reinterpret_cast<uint8_t*>(&cSize), reinterpret_cast<uint8_t*>(&cSize) + 4);
        fileBody.insert(fileBody.end(), reinterpret_cast<uint8_t*>(&uSize), reinterpret_cast<uint8_t*>(&uSize) + 4);

        // Write Compressed Stream
        fileBody.insert(fileBody.end(), compBuffer.begin(), compBuffer.begin() + actualCompSize);
    }

    // 3. Calculate MD5 Trailer
    WhsTrailer newTrailer;
    memset(&newTrailer, 0, sizeof(newTrailer));
    newTrailer.magic = WHS_TRAILER_MAGIC;

    ComputeMd5(fileBody, newTrailer, newTrailer.md5);

    // 4. Write Output File
    std::ofstream out(filePath, std::ios::binary | std::ios::trunc);
    if (!out.is_open()) {
        outError = "Failed to open destination file for writing: " + filePath;
        return false;
    }

    out.write(reinterpret_cast<const char*>(fileBody.data()), fileBody.size());
    out.write(reinterpret_cast<const char*>(&newTrailer), sizeof(newTrailer));

    if (!out.good()) {
        outError = "Failed while writing save data to disk";
        return false;
    }

    return true;
}

} // namespace KCD2
