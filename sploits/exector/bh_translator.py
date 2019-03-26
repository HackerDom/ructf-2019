import pickle
import zlib


with open("table", "rb") as table_file:
    table = pickle.loads(zlib.decompress(table_file.read()))


def translate(source: bytes):
    result = []
    lastc = 0
    for c in source:
        a = table[lastc][c]
        b = table[0][c]
        if len(a) <= len(b):
            result.append(a)
        else:
            result.append(">" + b)
        result.append('.')
        lastc = c
    return ''.join(result)


def main():
    print(translate(b'Hello world!'))


if __name__ == '__main__':
    main()
