#!/home/cs/miniconda3/bin/python3
from infrastructure.actions import Checker
from infrastructure.verdict import Verdict
from aiohttp_sse_client import client as sse_client
import asyncio

from RustClient import RustClient
from NotificationApiClient import NotificationApiClient
from binascii import hexlify

from PIL import Image
import base64
import io
import string
import random
Checker.INFO = "1"

RUSTPORT = 7878
NotificationApiPort = 5000

rustClient = RustClient(RUSTPORT, 3)
notificationApiClient = NotificationApiClient("10.33.54.127", NotificationApiPort, 3)

IMAGE_WIDTH = 1000
IMAGE_HEIGHT = 1000

PIXELS_WITH_FLAG = [(1088, 223), (992,283), (1020, 172), (1066, 353), (974, 636), (982, 570), (1042, 497), (1055, 420)]




def check_result(result):
    if result is None:
        return Verdict.DOWN("network error", "network error")
    if not result.is_success:
        return Verdict.MUMBLE("unexpected result", "request finished with code {}".format(result.code))

    return None


@Checker.define_check
def check_service(host: str) -> Verdict:
    return asyncio.run(check(host))


async def check(host: str) -> Verdict:
    password = generate_random_string()
    src_name = generate_random_string(31)
    message = generate_random_string()
    result = rustClient.create_source(src_name, password, False, host)
    check_res = check_result(result)
    if check_res is not None:
        return check_res

    token = result.result
    push_result = rustClient.push_to_source(src_name, password, message, host)
    check_res = check_result(push_result)
    if check_res is not None:
        return check_res

    fl = True
    subscribe_req = notificationApiClient.create_subscribe_on_source_request(src_name, token)
    async with sse_client.EventSource(subscribe_req) as event_source:
        try:
            async for event in event_source:
                decode_result = get_flag_from_base64(event.data)
                if decode_result is None:
                    continue

                if message in decode_result.upper():
                    fl = False
                    break
        except Exception as e:
            return Verdict.DOWN("network error", "network error")

    if fl:
        return Verdict.CORRUPT("flag not found", "flag not found")

    get_sources_list_result = rustClient.get_sources_list(host)
    check_res = check_result(get_sources_list_result)
    if check_res is not None:
        return check_res

    print(1)
    return Verdict.OK()


@Checker.define_put(vuln_num=1)
def put_flag_into_the_service(host: str, flag_id: str, flag: str) -> Verdict:
    password = generate_random_string()
    result = rustClient.create_source(flag_id, password, False, host)
    check_res = check_result(result)
    if check_res is not None:
        return check_res

    token = result.result

    push_result = rustClient.push_to_source(flag_id, password, flag, host)
    check_res = check_result(push_result)
    if check_res is not None:
        return check_res

    return Verdict.OK('{}:{}'.format(flag_id, token))


@Checker.define_get(vuln_num=1)
def get_flag_from_the_service(host: str, flag_id: str, flag: str) -> Verdict:
    return asyncio.run(get_flag(host, flag_id, flag))


async def get_flag(host: str, flag_id: str, flag: str) -> Verdict:
    parts = flag_id.split(':')
    token = parts[1]
    flag_id = parts[0]
    subscribe_req = notificationApiClient.create_subscribe_on_source_request(flag_id, token)
    async with sse_client.EventSource(subscribe_req) as event_source:
        try:
            async for event in event_source:
                decode_result = get_flag_from_base64(event.data)
                if decode_result is None:
                    continue

                if flag in decode_result.upper():
                    return Verdict.OK()
        except Exception as e:
            return Verdict.DOWN("network error", "network error")

    return Verdict.CORRUPT("flag not found", "flag not found")


def generate_random_string(N=32):
    return ''.join(random.choices(string.ascii_uppercase + string.digits, k=N))


def generate_random_bytes(N=16):
    a = bytearray(random.getrandbits(8) for _ in range(N))
    return hexlify(a).upper().decode("utf-8")


def get_flag_from_base64(base64text):
    try:
        image = getImageFromBase64(base64text)
        bytes = get_bytes_with_flag(image)
        flag = decode_flag_bytes(bytes)
        return flag
    except Exception as e:
        return None


def getImageFromBase64(base64Image) -> Image:
    try:
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

        return ''.join(res2[::-1])
    except Exception as e:
        return None


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
   ip = "10.33.54.127"
   check_service(ip)

