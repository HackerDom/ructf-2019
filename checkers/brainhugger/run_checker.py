from subprocess import check_output, Popen, PIPE
from time import sleep

from generators import generate_string


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


def put(flag, vuln):
    print("PUT FLAG={} VULN={}".format(flag, vuln), end=' ')
    popen = Popen(["./checker.py", "put",  "localhost", "flag_id", flag, str(vuln)], stderr=PIPE, stdout=PIPE)
    popen.wait()
    if check_ret_code(popen):
        print("FAIL")
    else:
        print("OK")
        return popen.stdout.read().decode().strip()


def get(flag_id, flag, vuln):
    print("GET FLAG_ID={} FLAG={} VULN={}".format(flag_id, flag, vuln), end=' ')
    popen = Popen(["./checker.py", "get",  "localhost", flag_id, flag, str(vuln)], stderr=PIPE, stdout=PIPE)
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
            sleep(1)
            print("Cycle:", cycle)
            check()
            flag = generate_string(32)
            for vuln in range(1, 3):
                flag_id = put(flag, vuln)
                if flag_id is not None:
                    get(flag_id, flag, vuln)
            cycle += 1
            print()
        except KeyboardInterrupt:
            print("\nstop")
            break


if __name__ == '__main__':
    run()
