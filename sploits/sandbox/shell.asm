[SECTION .text]
global _start
_start:
		sub rsp, 0x8
 		mov rbx, [rel $ + 0x2013d8]
 		mov rax, 0x13F150
 		sub rbx, rax
 		call rbx
 		add rsp, 0x8
 		ret