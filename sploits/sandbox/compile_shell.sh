#!/bin/bash

nasm -f elf64 shell.asm
objdump -d -M intel shell.o
rm shell.o