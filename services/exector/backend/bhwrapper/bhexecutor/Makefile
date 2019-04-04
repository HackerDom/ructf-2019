all:
	rm -rf lib
	mkdir lib
	gcc -Wall -c bhexecutor.c -o bhexecutor.o
	ar -cq lib/bhexecutor.a bhexecutor.o
	rm bhexecutor.o
