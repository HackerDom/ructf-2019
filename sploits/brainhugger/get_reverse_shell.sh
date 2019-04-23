as -o reverse_shell.o reverse_shell.s
ld -N -o reverse_shell reverse_shell.o
objcopy -O binary reverse_shell reverse_shell.bin
hexdump -v -e '"\\""x" 1/1 "%02x" ""' reverse_shell.bin

