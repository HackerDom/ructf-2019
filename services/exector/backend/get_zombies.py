s = set()
with open("log.log") as file:
    for e in map(int, file.read().split()):
        if e in s:
            s.remove(e)
        else:
            s.add(e)
print(s)

