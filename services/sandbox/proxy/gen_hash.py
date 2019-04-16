#!/usr/bin/env python3
import os

f = open("libinterface.so", "rb")
hash = 0
while 1:
    bytes = f.read(4)
    if not bytes or len(bytes) != 4:
        break
    hash = hash ^ int.from_bytes(bytes, byteorder='little')

os.system("echo \"#define kLibHash %u\" > hash.h" % hash)