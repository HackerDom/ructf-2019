.PHONY: debug release clean

shaders:
	#glslangValidator -V base.vert
	#glslangValidator -V base.frag

debug: shaders
	g++ -O0 -g -std=c++17 -DDEBUG=1 main.cpp png.cpp shader.cpp texture.cpp gl.cpp -lglfw -lGL -lGLEW -lpng16 -o sandbox-dbg

release: shaders
	g++ -O2 -g -std=c++17 main.cpp png.cpp shader.cpp texture.cpp -o sandbox

start:
	/home/sandbox/sandbox

serve: release start
