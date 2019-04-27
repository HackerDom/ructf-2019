#!/usr/bin/env python3
from __future__ import print_function
from sys import argv, stderr
import random
import string
from client import *
from random import randint

SERVICE_NAME = "geoapi"
OK, CORRUPT, MUMBLE, DOWN, CHECKER_ERROR = 101, 102, 103, 104, 110


def close(code, public="", private=""):
    if public:
        print(public)
    if private:
        print(private, file=stderr)
    print('Exit with code %d' % code, file=stderr)
    exit(code)


def check(*args):
    addr = args[0]

    name = "name"

    # TODO: validate data
    agent_sended = register_agent(addr)
    agent = get_agent(addr, agent_sended["AgentToken"])

    try_upload_tobject(addr, agent_sended["AgentToken"])

    try_upload_tobject(addr, agent_sended["AgentToken"])

    recived_tobject = get_objects(addr, agent["AgentToken"])

    close(OK)


def try_upload_tobject(addr, agentToken):
    tobjDescripion = generate_tobject_description(32)
    tobject = generate_tobject_request(agentToken, tobjDescripion)

    tobject_key = upload_object(addr, tobject)
    recived_tobject = get_object(addr, tobject_key, agentToken)

    if (json.dumps(tobject["Cells"]) != json.dumps(recived_tobject["Cells"])):
        close(CORRUPT)

    if (tobject_key != recived_tobject["IndexKey"]):
        close(CORRUPT)


def put(*args):
    addr = args[0]
    flag_id = args[1]
    flag = args[2]

    agent = register_agent(addr)

    request = generate_tobject_request(agent["AgentToken"], flag)
    tobject_key = upload_object(addr, request)

    close(OK, "%s/%s" % (agent["AgentToken"], tobject_key))


def get(*args):
    addr = args[0]
    flag_id = args[1]
    flag = args[2]

    keys = flag_id.split("/")

    recived_tobject = get_object(addr, keys[1], keys[0])

    if (recived_tobject["Info"] != flag):
        close(CORRUPT)

    close(OK)


def info(*args):
    close(OK, "vulns: 1")


def generate_tobject_description(stringLength):
    letters = string.ascii_lowercase
    return ''.join(random.choice(letters) for i in range(stringLength))


def generate_tobject_request(agentKey, flag):
    cells = []
    for x in range(0, 64):
        cells.append([])
        for y in range(0, 64):
            cells[x].append(randint(0, 3))

    return {"AgentId": agentKey, "Info": flag, "Cells": cells}


def not_found(*args):
    print("Unsupported command %s" % argv[1], file=stderr)
    return CHECKER_ERROR


put("localhost", "", "flag")

COMMANDS = {'check': check, 'put': put, 'get': get, 'info': info}

if __name__ == '__main__':
    try:
        COMMANDS.get(argv[1], not_found)(*argv[2:])
    except Exception as e:
        close(CHECKER_ERROR, "Evil checker", "INTERNAL ERROR: %s" % e)
