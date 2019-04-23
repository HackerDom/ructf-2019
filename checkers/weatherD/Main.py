from NotificationApiClient import NotificationApiClient

if __name__ == '__main__':
    ip = '127.0.0.1'
    port = 5000
    source = 'qwert'
    password = '12345'

    client = NotificationApiClient(port, 300)

    token = client.add_source_info(source, password, ip)

    client.subscribe_on_source(source, token, ip)