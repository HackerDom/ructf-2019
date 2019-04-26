from subprocess import check_output, Popen, PIPE
from time import sleep
from random import random
from os import fork
from generators import generate_string


HOST = "localhost"


def check_ret_code(popen):
    if popen.returncode != 101:
        print(popen.stdout.read().decode())
        print(popen.stderr.read().decode())
        print(popen.returncode)
        return True
    return False


def check():
    print("CHECK", end=' ')
    popen = Popen("./checker.py check localhost", stderr=PIPE, stdout=PIPE, shell=True)
    popen.wait()
    if check_ret_code(popen):
        print("FAIL")
    return False


def put(flag, vuln):
    print("PUT FLAG={} VULN={}".format(flag, vuln), end=' ')
    popen = Popen(["./checker.py", "put",  HOST, "flag_id", flag, str(vuln)], stderr=PIPE, stdout=PIPE)
    popen.wait()
    if check_ret_code(popen):
        print("FAIL")
    else:
        print("OK")
        return popen.stdout.read().decode().strip()


def get(flag_id, flag, vuln):
    print("GET FLAG_ID={} FLAG={} VULN={}".format(flag_id, flag, vuln), end=' ')
    popen = Popen(["./checker.py", "get",  HOST, flag_id, flag, str(vuln)], stderr=PIPE, stdout=PIPE)
    popen.wait()
    if check_ret_code(popen):
        print("FAIL")
    else:
        print("OK")
        return popen.stdout.read()


def run():
    cycle = 0
    while True:
        try:
            print("Cycle:", cycle)
            if check() is None:
                continue
            flag = generate_string(32)
            for vuln in range(1, 3):
                flag_id = put(flag, vuln)
                if flag_id is None:
                    continue
                get(flag_id, flag, vuln)
            cycle += 1
            print()
            # sleep(1)
        except KeyboardInterrupt:
            print("\nstop")
            break


def sr():
    sleep(random())


def main():
     for _ in range(3):
         sr()
         fork()
    run()


if __name__ == '__main__':
    main()


