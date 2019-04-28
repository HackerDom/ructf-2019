#!/home/cs/miniconda3/bin/python3
import binascii
import time
from base64 import b64encode, b64decode
from http.cookiejar import CookieJar
from sys import argv, stderr
import json
import traceback

import requests
import urllib3

from generators import generate_headers, generate_string, generate_mega_task, generate_simple_task,\
    generate_long_output_task

REGISTER_URL = "http://{hostport}/register"
LOGIN_URL = "http://{hostport}/login"
RUN_TASK_URL = "http://{hostport}/run_task"
TASK_INFO_URL = "http://{hostport}/task_info/{task_id}?token={token}"
TIMEOUT = 10
CHECK_TRIES_COUNT = 10
RETRY_SLEEP = 1
OK, CORRUPT, MUMBLE, DOWN, CHECKER_ERROR = 101, 102, 103, 104, 110
PORT = 8080


def get_hostport(hostname):
    return "{}:{}".format(hostname, PORT)


def print_to_stderr(*objs):
    print(*objs, file=stderr)


def get_stack():
    return "stack=(" + ", ".join("{}:{}".format(frame.name, frame.lineno) for frame in traceback.extract_stack()) + ")"


def exit_with(code, message, print_traceback=False):
    print_to_stderr(message)
    print_to_stderr(get_stack())
    if print_traceback:
        traceback.print_exc()
    exit(code)


def info():
    print("vulns: 1:1")
    exit(OK)


def get_data_from_cookies(cookies):
    uid = None
    secret = None
    for cookie in cookies:
        if cookie.name == "uid":
            try:
                uid = int(cookie.value)
            except ValueError:
                exit_with(MUMBLE, "invalid uid from cookie: '{}'".format(cookie.value))
        elif cookie.name == "secret":
            secret = cookie.value
    if uid is None or secret is None:
        exit_with(MUMBLE, "uid or secret is None")
    return uid, secret


def register(hostport, password):
    url = REGISTER_URL.format(hostport=hostport)
    r = requests.post(
        url,
        json={"password": password},
        headers=generate_headers(),
        timeout=TIMEOUT,
    )
    r.raise_for_status()
    return r.cookies


def login(hostport, password, uid):
    url = LOGIN_URL.format(hostport=hostport)
    r = requests.post(
        url,
        json={"userId": uid, "password": password},
        headers=generate_headers(),
        timeout=TIMEOUT,
    )
    r.raise_for_status()
    return r.cookies


def run_task(hostport: str, cookies: CookieJar, source: str, stdin: bytes, token: str):
    url = RUN_TASK_URL.format(hostport=hostport)
    r = requests.post(
        url,
        cookies=cookies,
        json={
            "source": source,
            "stdinb64": b64encode(stdin).decode(),
            "token": token,
        },
        headers=generate_headers(),
        timeout=TIMEOUT,
    )
    r.raise_for_status()
    data = json.loads(r.content)
    if "taskId" not in data:
        exit_with(MUMBLE, "run task response has not taskId: {}".format(data))
    task_id = data["taskId"]
    if not isinstance(task_id, int):
        exit_with(MUMBLE, "task id is not int: ({}, {})".format(task_id, type(task_id)))
    return task_id


def get_task_info(hostport, cookies, task_id, token):
    url = TASK_INFO_URL.format(hostport=hostport, task_id=task_id, token=token)
    r = requests.get(
        url,
        cookies=cookies,
        headers=generate_headers(),
        timeout=TIMEOUT,
    )
    r.raise_for_status()
    data = json.loads(r.content)
    if "Stdoutb64" not in data or "Status" not in data or "Error" not in data:
        exit_with(MUMBLE, "task info response has no required field(s): {}".format(data))
    stdoutb64 = data["Stdoutb64"]
    stdout = b64decode(stdoutb64)
    status = data["Status"]
    error = data["Error"]
    if not isinstance(stdout, bytes) or not isinstance(status, int) or not isinstance(error, str):
        exit_with(MUMBLE, "at least field in task info response has wrong type: {}".format(data))
    return stdout, status, error


def check_with_task(hostname, task_func):
    hostport = get_hostport(hostname)
    password = generate_string(5)
    token = generate_string(5)
    cookies = register(hostport, password)
    uid, _ = get_data_from_cookies(cookies)
    another_cookies = login(hostport, password, uid)
    if another_cookies != cookies:
        exit_with(MUMBLE, "different cookies with same uid and password: {} != {}".format(cookies, another_cookies))
    # source, stdin, output = generate_long_output_task()
    # source, stdin, output = generate_mega_task()
    source, stdin, output = task_func()
    task_id = run_task(hostport, cookies, source, stdin, token)
    stdout = b""
    status = 1
    error = ""
    for try_index in range(CHECK_TRIES_COUNT):
        try:
            stdout, status, error = get_task_info(hostport, cookies, task_id, token)
            if status != 1:
                break
        except requests.exceptions.HTTPError as e:
            if e.response.status_code != 404:
                raise
        time.sleep(RETRY_SLEEP)
    if status == 1:
        exit_with(MUMBLE, "can not get task info, too long answer")
    elif status == 2:
        exit_with(MUMBLE, "error while task running: {}".format(error))
    if output != stdout:
        exit_with(MUMBLE, "wrong output:\n\nexpected='{}'\n\nreal='{}'\n".format(output, stdout))
    exit(OK)


def check(hostname):
    check_with_task(hostname, generate_mega_task)
    check_with_task(hostname, generate_long_output_task)


def put_first(hostname, flag_id, flag):
    hostport = get_hostport(hostname)
    password = flag
    cookie = register(hostport, password)
    uid, _ = get_data_from_cookies(cookie)
    print("{},{}".format(uid, password))
    exit(OK)


def get_first(hostname, flag_id, flag):
    hostport = get_hostport(hostname)
    raw_uid, password = flag_id.split(',')
    uid = int(raw_uid)
    try:
        login(hostport, password, uid)
    except requests.exceptions.HTTPError as e:
        if e.response.status_code == 403:
            exit_with(CORRUPT, "can not log in", print_traceback=True)
    exit(OK)


def put_second(hostname, flag_id, flag):
    hostport = get_hostport(hostname)
    password = generate_string(10)
    cookies = register(hostport, password)
    source = generate_simple_task()
    token = flag
    task_id = run_task(hostport, cookies, source, b"", token)
    uid, _ = get_data_from_cookies(cookies)
    print("{},{},{}".format(uid, password, task_id))
    exit(OK)


def get_second(hostname, flag_id, flag):
    hostport = get_hostport(hostname)
    raw_uid, password, task_id = flag_id.split(",")
    uid = int(raw_uid)
    try:
        cookies = login(hostport, password, uid)
        get_task_info(hostport, cookies, task_id, flag)
    except requests.exceptions.HTTPError as e:
        if e.response.status_code in {403, 404}:
            exit_with(CORRUPT, "can not get flag", print_traceback=True)
    exit(OK)


def get(hostname, flag_id, flag, vuln):
    {'1': get_first, '2': get_second}[vuln](hostname, flag_id, flag)


def put(hostname, flag_id, flag, vuln):
    {'1': put_first, '2': put_second}[vuln](hostname, flag_id, flag)


def not_found(*args):
    exit_with(CHECKER_ERROR, "Unsupported command: {}".format(args[1]))


COMMANDS = {'check': check, 'put': put, 'get': get, 'info': info}


def main():
    try:
        COMMANDS.get(argv[1], not_found)(*argv[2:])
    except (requests.exceptions.ConnectionError, ConnectionRefusedError, urllib3.exceptions.NewConnectionError,
            urllib3.exceptions.ReadTimeoutError, requests.exceptions.ReadTimeout):
        exit_with(DOWN, "service is down: '{}'".format(argv[1:]), print_traceback=True)
    except (requests.exceptions.HTTPError, json.decoder.JSONDecodeError, ValueError, binascii.Error):
        exit_with(MUMBLE, "known exception while doing command: '{}'".format(argv[1:]), print_traceback=True)
    except Exception:
        traceback.print_exc()
        exit(CHECKER_ERROR)


if __name__ == '__main__':
    main()
