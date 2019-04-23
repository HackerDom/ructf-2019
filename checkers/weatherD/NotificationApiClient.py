from urllib.request import urlopen, Request
from urllib.error import HTTPError, URLError
from urllib.parse import urlencode
from SseClient import SSEClient


class NotificationApiClient:
    def __init__(self, port, timeout):
        self.port = port
        self.timeout = timeout

    def subscribe_on_source(self, source_name, token, ip):
        url = "http://{0}:{1}/subscribe?source={2}&token={3}".format(ip, self.port, source_name, token)
        try:
            messages = SSEClient(url)
        except Exception as e:
            return SubscribeResult(False)

        return SubscribeResult(True, messages)

    def create_subscribe_on_source_request(self, source_name, token, ip):
        post_fields = {'source': source_name, 'token': token}
        return Request("http://{0}:{1}/subscribe?source={2}&token={3}".format(ip, self.port, source_name, token), urlencode(post_fields).encode())

    def do_request(self, req):

        try:
            resp = urlopen(req, timeout=self.timeout).read().decode().strip()
        except (HTTPError, URLError):
             resp = urlopen(req, timeout=self.timeout).read().decode().strip()
        return resp


class SubscribeResult:
    def __init__(self, is_success, iter=None):
        self.is_success = is_success
        self.iter = iter