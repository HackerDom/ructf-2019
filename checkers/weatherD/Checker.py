from Checkers.infrastructure.actions import Checker
from Checkers.infrastructure.verdict import Verdict
from aiohttp_sse_client import client as sse_client
import asyncio

from RustClient import RustClient
from NotificationApiClient import NotificationApiClient
from binascii import hexlify, unhexlify

from Crypto.Cipher import AES
from PIL import Image
import base64
import io
import string
import random
import uuid
import time
Checker.INFO = "1:2"

RUSTPORT = 7878
NotificationApiPort = 5000

rustClient = RustClient(RUSTPORT, 3)
notificationApiClient = NotificationApiClient(NotificationApiPort, 3)

IMAGE_WIDTH = 1000
IMAGE_HEIGHT = 1000

PIXELS_WITH_FLAG = [(1088, 223), (992,283), (1020, 172), (1066, 353), (974, 636), (982, 570), (1042, 497), (1055, 420)]

def check_result(result, out):
    if result is None:
        out = Verdict.DOWN("network error", "network error")
        return False
    if not result.is_success:
        out = Verdict.MUMBLE("unexpected result", "request finished with code {}".format(result.code))
        return False

    return True

@Checker.define_check
def check_service(host: str) -> Verdict:
    password = generate_random_string()
    src_name = str(uuid.uuid4())
    message = generate_random_string()
    result = rustClient.create_source(src_name, password, False, False, '', '', host)
    a = None
    if not check_result(result, a):
        return a

    token = result.result
    push_result = rustClient.push_to_source(src_name, password, message, host)
    if not check_result(push_result, a):
        return a

    subscribe_result = notificationApiClient.subscribe_on_source(src_name, token, host)
    if not subscribe_result.is_success:
        return Verdict.DOWN("network error", "network error")

    for mess in subscribe_result.iter:
        decode_result = get_flag_from_base64(mess)
        if decode_result is None:
            continue

        if message in decode_result:
            return Verdict.OK()

    return Verdict.CORRUPT("flag not found", "flag not found")

def put_flag_into_the_service1(host: str, flag_id: str, flag: str) -> Verdict:
    password = generate_random_string()
    result = rustClient.create_source(flag_id, password, False, False, '', '', host)
    a = None
    if not check_result(result, a):
        return a

    token = result.result
    print(token)
    push_result = rustClient.push_to_source(flag_id, password, flag, host)
    a = None
    if not check_result(push_result, a):
        return a

    return '{}:{}'.format(flag_id, token)
    return Verdict.OK('{}:{}'.format(flag_id, token))


async def get_flag_from_the_service1(host: str, flag_id: str, flag: str) -> Verdict:
    parts = flag_id.split(':')
    token = parts[1]
    flag_id = parts[0]
    print("flag_id:   "  + flag_id)
    subscribe_req = notificationApiClient.create_subscribe_on_source_request(flag_id, token, host)
    async with sse_client.EventSource(subscribe_req) as event_source:
        try:
            async for event in event_source:
                decode_result = get_flag_from_base64(event.data)
                print(decode_result[1].upper() == flag)
                if flag in decode_result:
                    return Verdict.OK()
        except Exception as e:
            return Verdict.DOWN("network error", "network error")

    return Verdict.CORRUPT("flag not found", "flag not found")



def put_flag_into_the_service2(host: str, flag_id: str, flag: str) -> Verdict:
    password = generate_random_string()
    key = generate_random_bytes(16)
    iv = generate_random_bytes(35)
    result = rustClient.create_source(flag_id, password, True, True, key, iv, host)
    a = None
    if not check_result(result, a):
        return a

    token = result.result
    push_result = rustClient.push_to_source(flag_id, password, flag, host)
    if not check_result(push_result, a):
        return a

    return Verdict.OK('{}:{}:{}:{}'.format(flag, token, key, iv))


@Checker.define_get(vuln_num=2)
async def get_flag_from_the_service(host: str, flag_id: str, flag: str) -> Verdict:
    parts = flag_id.split(':')
    token = parts[1]
    flag_id = parts[0]
    key = parts[2]
    iv = parts[3]

    subscribe_req = notificationApiClient.create_subscribe_on_source_request(flag_id, token, host)
    async with sse_client.EventSource(subscribe_req) as event_source:
        try:
            async for event in event_source:
                #(event)
                pass
        except ConnectionError:
            return Verdict.DOWN("network error", "network error")


    # for mess in subscribe_result.iter:
    #     decode_result = get_flag_from_aes(mess)
    #     if not decode_result[0]:
    #         continue
    #
    #     if flag in decode_result[1]:
    #         return Verdict.OK()

    return Verdict.CORRUPT("flag not found", "flag not found")


def generate_random_string(N=32):
    return ''.join(random.choices(string.ascii_uppercase + string.digits, k=N))


def generate_random_bytes(N=16):
    a = bytearray(random.getrandbits(8) for _ in range(N))
    return hexlify(a).upper().decode("utf-8")


def get_flag_from_aes(key, ciphertext, iv):
    try:
        image = getImageFromBase64(ciphertext)
        bytes = get_bytes_with_flag(image)
        message = decode_flag_bytes(bytes)
        result = decodeAES(key, message, iv)
        return result
    except Exception as e:
        return None


def get_flag_from_base64(base64text):
    try:
        image = getImageFromBase64(base64text)
        bytes = get_bytes_with_flag(image)
        flag = decode_flag_bytes(bytes)
        print(len(flag[1]))
        return flag
    except Exception as e:
        return None


def decodeAES(key, ciphertext, iv):
    cipher = AES.new(key, AES.MODE_EAX, iv=iv)
    result = cipher.decrypt(ciphertext)
    return result

def getImageFromBase64(base64Image) -> Image:
    try:
        image = open("image.png", "wb")
        image.write( base64.b64decode(base64Image))
        image.close()

        bytes = base64.b64decode(base64Image)

        image = Image.open(io.BytesIO(bytes))
        return image
    except Exception as e:
        return None



def get_bytes_with_flag(image : Image):
    res = []
    for coord in PIXELS_WITH_FLAG:
        a = image.getpixel(coord)
        r, g, b = a[0], a[1], a[2]
        res = res + [r, g, b]

    print(res)
    print(','.join(list(map(lambda x: str(hex(x)), res))))
    return res


def decode_flag_bytes(u8bytes):
    try:
        nums = []
        i = 0
        n = 0

        for x in u8bytes[::-1]:
            i += 1
            n = n << 8
            n += x

            if i == 12: # 11
                i = 0
                nums.append(n)
                n = 0
        res = []
        for x in nums:
            print("num :", x)
            r = x
            for i in range(17):
                num = to_u32(r % 37)
                res.append(unpack_char(num))
                r = r // 37

        res2 = []
        try:
            for x in range(len(res)):
                if x != 33 and x != 16: # 14
                    res2.append(res[x])
        except:
            pass

        return True, ''.join(res2[::-1])
    except Exception as e:
        return False, e


def unpack_char(n):
    if 0 <= n < 10:
        return chr(to_u8(n) + ord('0'))

    if 10 <= n < 36:
        return chr(to_u8(n - 10) + ord('a'))

    if n == 36:
        return '='

def to_u8(i):
    return i % 2 ** 8


def to_u32(i):
    return i % 2 ** 32


if __name__ == '__main__':
    flag = generate_random_string(31) +  '='
    print(flag)
    name= str(uuid.uuid4())
    a = put_flag_into_the_service1("10.33.54.127", name, flag)

    loop = asyncio.get_event_loop()
    loop.run_until_complete(get_flag_from_the_service1("10.33.54.127", a, flag))
