from random import getrandbits
from functools import reduce
from operator import concat
from uuid import UUID

# https://github.com/kmyk/mersenne-twister-predictor
from mt19937predictor import MT19937Predictor


def rand_four_bytes():
    return getrandbits(32).to_bytes(4, 'little')


def get_uuid_bytes():
    return bytes(reduce(concat, (rand_four_bytes() for _ in range(4)), bytearray()))


def get_uuid():
    return str(UUID(bytes=get_uuid_bytes()))


def get_invites_pack(count=624 / 4):
    return [get_uuid() for _ in range(int(count))]

    
invites = get_invites_pack()
pred = MT19937Predictor()
for invite in invites:
    id = UUID(invite).bytes
    for i in range(0, len(id), 4): 
        pred.setrandbits(int.from_bytes(id[i:i+4], 'little'), 32)

print(get_uuid())
print(str(UUID(bytes=bytes(reduce(concat, (pred.getrandbits(32).to_bytes(4, 'little') for _ in range(4)), bytearray())))))