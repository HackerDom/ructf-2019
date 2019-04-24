from base64 import b64encode
from random import randint, choice
from string import ascii_letters, digits

from bh_translator import translate

BUBBLE_SORT_SOURCE = ">>,[>>,]<<[[<<]>>>>[<<[>+<<+>-]>>[>+<<<<[->]>[<]>>-]<<<[[-]>>[>+<-]>>[<<<+>>>-]]>>[[<+>-]>>]<]<<[>>+<<-]<<]>>>>[.>>]"
BUBBLE_SORT_STDIN = b"dbeac\0"
BUBBLE_SORT_STDOUT = "abcde"

with open("user-agents") as user_agents_file:
    USER_AGENTS = user_agents_file.read().split('\n')


def generate_string(password_len):
    return "".join(choice(ascii_letters + digits) for _ in range(password_len))


def generate_user_agent():
    return choice(USER_AGENTS)


def generate_headers():
    return {'User-Agent': generate_user_agent()}


def generate_mega_task():
    length = 10
    output = generate_string(length)
    source = translate(output.encode()) + ">"
    tiny_stdin = generate_string(length)
    return source + BUBBLE_SORT_SOURCE + ",." * length,\
        BUBBLE_SORT_STDIN + tiny_stdin.encode(), \
        output + BUBBLE_SORT_STDOUT + tiny_stdin


def generate_simple_task():
    return "".join(choice("+->.") for _ in range(10))