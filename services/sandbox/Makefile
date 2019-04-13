#.PHONY: debug release clean
CXXFLAGS = -c -g -std=c++17
SOURCES = buildings.cpp gl.cpp main.cpp png.cpp shader.cpp texture.cpp tiny_obj_loader.cpp units.cpp
LDFLAGS = -lglfw -lGL -lGLEW -lpng16
OBJS = $(SOURCES:.cpp=.o)

all: release

debug: CXXFLAGS += -O0
debug: OUTPUT = sandbox-dbg
debug: sandbox

release: CXXFLAGS += -O2
release: OUTPUT = sandbox
release: sandbox

sandbox: $(OBJS)
	g++ $(OBJS) $(LDFLAGS) -o $(OUTPUT)

.cpp.o:
	g++ $(CXXFLAGS) $< -o $@

.PHONY: clean
clean:
	rm -f $(OBJS) sandbox sandbox-dbg

start:
	/home/sandbox/sandbox

serve: release start
