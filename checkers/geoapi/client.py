from sys import stderr
import requests
import UserAgents
import json

OK, CORRUPT, MUMBLE, DOWN, CHECKER_ERROR = 101, 102, 103, 104, 110


def close(code, public="", private=""):
    if public:
        print(public)
    if private:
        print(private, file=stderr)
    print('Exit with code %d' % code, file=stderr)
    exit(code)


def register_agent(addr):
    url = 'http://%s:9007/agent' % (addr)
    headers = {'User-Agent': UserAgents.get()}
    agent = {"AgentName": "TestName"}
    try:
        r = requests.post(url, headers=headers, json=agent)

        if r.status_code != 200:
            close(MUMBLE, "Submit error", "Invalid status code: %s %d" % (url, r.status_code))

        str = r.content.decode("UTF-8")
        return json.loads(str)

    except Exception as e:
        close(DOWN, "HTTP Error", "HTTP error: %s" % e)


def get_agent(addr, agentToken):
    url = 'http://%s:9007/agent?AgentKey=%s' % (addr, agentToken)
    headers = {'User-Agent': UserAgents.get()}
    try:
        r = requests.get(url, headers=headers)

        if r.status_code != 200:
            close(MUMBLE, "Submit error", "Invalid status code: %s %d" % (url, r.status_code))

        str = r.content.decode("UTF-8")
        return json.loads(str)

    except Exception as e:
        close(DOWN, "HTTP Error", "HTTP error: %s" % e)


def upload_object(addr, tobject):
    url = 'http://%s:9007/object' % (addr)
    headers = {'User-Agent': UserAgents.get()}

    try:
        r = requests.put(url, headers=headers, json=tobject)

        if r.status_code != 200:
            close(MUMBLE, "Submit error", "Invalid status code: %s %d" % (url, r.status_code))

        return r.content.decode("UTF-8")

    except Exception as e:
        close(DOWN, "HTTP Error", "HTTP error: %s" % e)


def get_object(addr, objectKey, agentKey):
    url = 'http://%s:9007/object?ObjectKey=%s&AgentKey=%s' % (addr, objectKey, agentKey)
    headers = {'User-Agent': UserAgents.get()}
    try:
        r = requests.get(url, headers=headers)

        if r.status_code != 200:
            close(MUMBLE, "Submit error", "Invalid status code: %s %d" % (url, r.status_code))

        str = r.content.decode("UTF-8")
        return json.loads(str)

    except Exception as e:
        close(DOWN, "HTTP Error", "HTTP error: %s" % e)


def get_objects(addr, agentKey):
    url = 'http://%s:9007/objects?AgentKey=%s' % (addr, agentKey)
    headers = {'User-Agent': UserAgents.get()}
    try:
        r = requests.get(url, headers=headers)

        if r.status_code != 200:
            close(MUMBLE, "Submit error", "Invalid status code: %s %d" % (url, r.status_code))

        str = r.content.decode("UTF-8")
        return json.loads(str)

    except Exception as e:
        close(DOWN, "HTTP Error", "HTTP error: %s" % e)
