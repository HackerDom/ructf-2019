import json
from time import sleep, time
from random import choice, randint, random
import requests
from string import ascii_letters, digits


# HOST = "Rally"
HOST = "0.0.0.0"


TASK_TEMPLATE = """{{
    "source": "{}",
    "stdin": "{}",
    "token": "{}"
}}
"""

def gen_str(n):
    return ''.join(choice(ascii_letters + digits) for _ in range(n))


def gen_code():
    return ''.join(choice("+-<>.,") for _ in range(1000))


def add_new_task(source, stdin, token):
    data = TASK_TEMPLATE.format(source, stdin, token)
    print(data)
    content = requests.post("http://{}:8080/run_task".format(HOST), data=data).content.decode()
    # print(content)
    return json.loads(content.replace('\n', '\\'))


def get_info(task_id, token):
    content = requests.get("http://{}:8080/task_info/{}?token={}".format(HOST, task_id, token)).content.decode()
    # print('get content', content)
    return json.loads(content.replace('\n', '\\'))


def main():
    start = time()
    cnt = 1
    oks = 0
    for _ in range(cnt):
        if True:
            source = "+[>]"
        else:
            source = "+[>+.]"
        stdin = gen_str(1000)
        token = "TOKEN"
        try:
            res = add_new_task(source, stdin, token).get('taskId')
            oks += 1
            print(res)
        except KeyboardInterrupt:
            print(time() - start)
            print("Requests:", oks)
            return
    print(time() - start)
    print("Requests:", oks)




if __name__ == '__main__':
    main()
