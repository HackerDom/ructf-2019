from urllib.request import urlopen, Request
from urllib.error import HTTPError, URLError
from urllib.parse import urlencode
import json

class RustClient:
    def __init__(self, port, timeout):
        self.port = port
        self.timeout = timeout

    def create_source(self, source_name: str, passwrd: str, is_public: bool,
                      use_encryption: bool, encryption_key: str, iv: str, ip: str):
        req = self.create_create_source_request(source_name, passwrd, is_public, use_encryption, encryption_key, iv, ip)
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

    def do_request(self, req, retries=3):
        for i in range(retries):
            try:
                return urlopen(req, timeout=self.timeout)
            except Exception as e:
                continue

        return None

    def create_create_source_request(self, source_name: str, passwrd: str, is_public: bool,
                                     use_encryption: bool, encryption_key: str, iv: str, ip: str):
        post_fields = {'name': source_name,
                       'password': passwrd,
                       'is_public': is_public,
                       'encryption': use_encryption,
                       'encryption_key': encryption_key,
                       'iv': iv
                       }
        post_json = json.dumps(post_fields)
        post_str = "name={}&password={}&is_public={}&encryption={}&encryption_key={}&iv={}".format(source_name, passwrd, is_public, use_encryption, encryption_key, iv)
        return Request("http://{0}:{1}/create_source".format(ip, self.port), post_json.encode())

    def create_push_to_source_request(self, source_name, password, message, ip):
        post_fields = {'name': source_name, 'password': password, 'message': message}
        post_str = "name={}&password={}&message={}".format(source_name, password,message)

        post_json = json.dumps(post_fields)
        return Request("http://{0}:{1}/push_message".format(ip, self.port), post_json.encode())

    def decode_body(self, response):
        try:
            return (True, response.read().decode().strip())
        except Exception as e:
            return (False, e)


class RustResult:
    def __init__(self, code, result=None, e=None):
        self.code = code
        self.result = result
        self.is_success = code == 200
        self.exception = e
