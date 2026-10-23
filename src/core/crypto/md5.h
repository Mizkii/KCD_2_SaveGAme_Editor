#ifndef KCD2_MD5_H
#define KCD2_MD5_H

#include <stdint.h>
#include <stddef.h>

#ifdef __cplusplus
extern "C" {
#endif

typedef struct {
    uint32_t state[4];
    uint32_t count[2];
    uint8_t buffer[64];
} KCD2_MD5_CTX;

void kcd2_md5_init(KCD2_MD5_CTX* ctx);
void kcd2_md5_update(KCD2_MD5_CTX* ctx, const uint8_t* data, size_t len);
void kcd2_md5_final(KCD2_MD5_CTX* ctx, uint8_t digest[16]);

/**
 * Convenience helper to hash a single contiguous memory block.
 */
void kcd2_md5_compute(const uint8_t* data, size_t len, uint8_t digest[16]);

#ifdef __cplusplus
}
#endif

#endif /* KCD2_MD5_H */
