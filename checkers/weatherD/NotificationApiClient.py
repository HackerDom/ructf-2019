from urllib.request import urlopen, Request
from urllib.error import HTTPError, URLError
from urllib.parse import urlencode
from SseClient import SSEClient

class NotificationApiClient:
    def __init__(self, port, timeout):
        self.port = port
        self.timeout = timeout

    def create_subscribe_on_source_request(self, source_name, token, ip):
        url = "http://{0}:{1}/subscribe?source={2}&token={3}".format(ip, self.port, source_name, token)
        return url

    def subscribe_on_source(self, source_name, token, ip):
        url = "http://{0}:{1}/subscribe?source={2}&token={3}".format(ip, self.port, source_name, token)
        try:
            messages = SSEClient(url)
        except Exception as e:
            return None

        return messages