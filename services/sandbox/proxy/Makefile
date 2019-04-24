#.PHONY: debug release clean
CXXFLAGS = -g -std=c++17 -DLINUX=1
SOURCES = httpserver.cpp main.cpp
LDFLAGS = -L $(shell pwd) -lmicrohttpd -ldl

all: release

debug: CXXFLAGS += -O0 -DDEBUG=1
debug: OUTPUT = proxy-dbg
debug: lib
debug: proxy

release: CXXFLAGS += -O2
release: OUTPUT = proxy
release: proxy

hash:
	python3 gen_hash.py

proxy: hash hash.h
	g++ $(CXXFLAGS) $(SOURCES) $(LDFLAGS) -o $(OUTPUT)

.PHONY: clean
clean:
	rm -f *.o proxy proxy-dbg

start:
	/home/sandbox/proxy

serve: release start
