from Checkers.infrastructure.actions import Checker
from Checkers.infrastructure.verdict import Verdict

from RustClient import RustClient
from NotificationApiClient import NotificationApiClient
from binascii import hexlify

from Crypto.Cipher import AES
import string
import random
import uuid
Checker.INFO = "1:2"

RUSTPORT = 7878
NotificationApiPort = 5000

tokens_storage = {}

rustClient = RustClient(RUSTPORT, 300)
notificationApiClient = NotificationApiClient(NotificationApiPort, 300)


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
        decode_result = decode_flag_from_image(mess)
        if decode_result is None:
            continue

        if message in decode_result:
            return Verdict.OK()

    return Verdict.CORRUPT("flag not found", "flag not found")

@Checker.define_put(vuln_num=1)
def put_flag_into_the_service(host: str, flag_id: str, flag: str) -> Verdict:
    password = generate_random_string()
    result = rustClient.create_source(flag_id, password, False, False, '', host)
    a = None
    if not check_result(result, a):
        return a

    token = result.result
    push_result = rustClient.push_to_source(flag_id, password, flag, host)
    a = None
    if not check_result(push_result, a):
        return a

    return Verdict.OK('{}:{}'.format(flag, token))


@Checker.define_get(vuln_num=1)
def get_flag_from_the_service(host: str, flag_id: str, flag: str) -> Verdict:
    parts = flag_id.split(':')
    token = parts[1]
    flag_id = parts[0]
    subscribe_result = notificationApiClient.subscribe_on_source(flag_id, token, host)
    if not subscribe_result.is_success:
        return Verdict.DOWN("network error", "network error")

    for mess in subscribe_result.iter:
        decode_result = decode_flag_from_image(mess)
        if decode_result is None:
            continue

        if flag in decode_result:
            return Verdict.OK()

    return Verdict.CORRUPT("flag not found", "flag not found")


@Checker.define_put(vuln_num=2)
def put_flag_into_the_service(host: str, flag_id: str, flag: str) -> Verdict:
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
def get_flag_from_the_service(host: str, flag_id: str, flag: str) -> Verdict:
    parts = flag_id.split(':')
    token = parts[1]
    flag_id = parts[0]
    key = parts[2]
    iv = parts[3]
    subscribe_result = notificationApiClient.subscribe_on_source(flag_id, token, host)
    if not subscribe_result.is_success:
        return Verdict.DOWN("network error", "network error")

    for mess in subscribe_result.iter:
        decode_result = decode_flag_from_image(mess)
        if not decode_result[0]:
            continue

        if flag in decode_result[1]:
            return Verdict.OK()

    return Verdict.CORRUPT("flag not found", "flag not found")


def generate_random_string(N=32):
    return ''.join(random.choices(string.ascii_uppercase + string.digits, k=N))


def generate_random_bytes(N=16):
    a = bytearray(random.getrandbits(8) for _ in range(N))
    return hexlify(a).upper().decode("utf-8")


def decodeAES(key, ciphertext, iv):
    cipher = AES.new(key, AES.MODE_EAX, iv=iv)
    result = cipher.decrypt(ciphertext)
    print(result)

def decode_flag_from_image(u8bytes):
    try:
        nums = []
        i = 0
        n = 0

        for x in u8bytes[::-1]:
            i += 1
            n = n << 8
            n += x

            if i == 11:
                i = 0
                nums.append(n)
                n = 0
        res = []
        for x in nums:
            r = x
            for i in range(17):
                num = to_u32(r % 36)
                res.append(unpack_char(num))
                r = r // 36

        res2 = []
        for x in range(17, 50):
            if x != 14 and x != 15:
                res2.append(res[x])

        return True, ''.join(res2[::-1])
    except Exception as e:
        return False, e


def unpack_char(n):
    if 0 <= n < 10:
        return chr(to_u8(n) + to_u8(48))

    if 10 <= n <= 36:
        return chr(to_u8(n - 10) + to_u8(97))


def to_u8(i):
    return i % 2 ** 8


def to_u32(i):
    return i % 2 ** 32


if __name__ == '__main__':
    a = generate_random_bytes()
    print(a)
