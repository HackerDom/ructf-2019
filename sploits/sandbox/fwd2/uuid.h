#pragma once
#include <array>

#define kUUIDSize 16
using UUID = std::array<uint8_t, kUUIDSize>;


inline void uuid_generate(UUID& uuid)
{
    for(size_t i = 0; i < uuid.size(); i++)
        uuid[i] = rand() % 256;
}


inline bool uuid_parse(const char* str, uint8_t* uuid)
{
    char buf[3];
    buf[2] = 0;
    for(size_t i = 0, byteIdx = 0; i < strlen(str); i += 2, byteIdx++)
    {
        buf[0] = str[i + 0];
        buf[1] = str[i + 1];
        if(!isxdigit(buf[0]) || !isxdigit(buf[1]))
            return false;

        uuid[byteIdx] = strtoul(buf, nullptr, 16);
    }

    return true;
}


inline bool uuid_parse(const char* str, UUID& uuid)
{
    return uuid_parse(str, uuid.data());
}


inline void uuid_unparse(const uint8_t* uuid, char* str)
{
    char* ptr = str;
    for(size_t i = 0; i < 16; i++)
    {
        sprintf(ptr, "%02X", uuid[i]);
        ptr += 2;
    }
    *ptr = '\0';
}


inline void uuid_unparse(const UUID& uuid, char* str)
{
    char* ptr = str;
    for(size_t i = 0; i < uuid.size(); i++)
    {
        sprintf(ptr, "%02X", uuid[i]);
        ptr += 2;
    }
    *ptr = '\0';
}