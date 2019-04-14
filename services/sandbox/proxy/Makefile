#.PHONY: debug release clean
CXXFLAGS = -c -g -std=c++17 -DLINUX=1
SOURCES = httpserver.cpp main.cpp
LDFLAGS = -lmicrohttpd
OBJS = $(SOURCES:.cpp=.o)

all: release

debug: CXXFLAGS += -O0 -DDEBUG=1
debug: OUTPUT = proxy-dbg
debug: proxy

release: CXXFLAGS += -O2
release: OUTPUT = proxy
release: proxy

proxy: $(OBJS)
	g++ $(OBJS) $(LDFLAGS) -o $(OUTPUT)

.cpp.o:
	g++ $(CXXFLAGS) $< -o $@

.PHONY: clean
clean:
	rm -f $(OBJS) proxy proxy-dbg

start:
	/home/sandbox/proxy

serve: release start
