#!/bin/bash

g++ -c interface.cpp -fpic -g -O0
g++ -shared -o libinterface.so interface.o 
rm interface.o