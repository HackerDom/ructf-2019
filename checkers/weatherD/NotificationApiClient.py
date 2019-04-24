class NotificationApiClient:
    def __init__(self, port, timeout):
        self.port = port
        self.timeout = timeout

    def create_subscribe_on_source_request(self, source_name, token, ip):
        url = "http://{0}:{1}/subscribe?source={2}&token={3}".format(ip, self.port, source_name, token)
        return url