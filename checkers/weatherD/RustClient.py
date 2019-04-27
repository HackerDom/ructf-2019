from urllib.request import urlopen, Request
import json
from requestsHepler import get_user_agent_header

class RustClient:
    def __init__(self, port, timeout):
        self.port = port
        self.timeout = timeout

    def create_source(self, source_name: str, passwrd: str, is_public: bool, ip: str):
        req = self.create_create_source_request(source_name, passwrd, is_public, ip)
        result = self.do_request(req)
        if result is None:
            return None
        code = result.getcode()
        if code == 200:
            decode_result = self.decode_body(result)
            if decode_result[0]:
                return RustResult(code, decode_result[1])

        return RustResult(code)

    def push_to_source(self, source_name, password, message, ip):
        req = self.create_push_to_source_request(source_name, password, message, ip)
        result = self.do_request(req)

        if result is None:
            return None
        code = result.getcode()
        return RustResult(code)

    def get_sources_list(self, ip):
        req = "http://{0}:{1}/get_sources_list".format(ip, self.port)
        result = self.do_request(req)
        if result is None:
            return None
        code = result.getcode()
        if code == 200:
            decode_result = self.decode_body(result)
            if decode_result[0]:
                return RustResult(code, decode_result[1])

        return RustResult(code)

    def do_request(self, req, retries=3):
        for i in range(retries):
            try:
                return urlopen(req, timeout=5)
            except Exception as e:
                continue

        return None


    def create_create_source_request(self, source_name: str, passwrd: str, is_public: bool, ip: str):
        post_fields = {'name': source_name,
                       'password': passwrd,
                       'is_public': is_public,
                       'landscape' : 'desert',
                       'race' : 'race',
                       "population" : "33"
                       }

        post_json = json.dumps(post_fields)
        return Request("http://{0}:{1}/create_source".format(ip, self.port), post_json.encode(), headers=get_user_agent_header())

    def create_push_to_source_request(self, source_name, password, message, ip):
        post_fields = {'name': source_name, 'password': password, 'message': message}

        post_json = json.dumps(post_fields)
        return Request("http://{0}:{1}/push_message".format(ip, self.port), post_json.encode(), headers=get_user_agent_header())

    def decode_body(self, response):
        try:
            return True, response.read().decode().strip()
        except Exception as e:
            return False, e


class RustResult:
    def __init__(self, code, result=None, e=None):
        self.code = code
        self.result = result
        self.is_success = code == 200
        self.exception = e
