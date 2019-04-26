from urllib.request import urlopen

class NotificationApiClient:
    def __init__(self, port, timeout):
        self.port = port
        self.timeout = timeout

    def create_subscribe_on_source_request(self, source_name, token, ip):
        url = "http://{0}:{1}/subscribe?source={2}&token={3}".format(ip, self.port, source_name, token)
        return url

    def subscribe_on_source(self, source_name, token, ip):
        req = self.create_subscribe_on_source_request(source_name, token, ip)
        return self.do_request(req)


    def do_request(self, req, retries=3):
        for i in range(retries):
            try:
                return urlopen(req, timeout=1000)
            except Exception as e:
                continue

        return None