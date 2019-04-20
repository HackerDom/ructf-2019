[SECTION .text]
global _start
_start:
		sub rsp, 0x10
		push rbx
 		mov rbx, [rel $ + 0x2013cb]
 		sub rbx, 0x13F150
 		call rbx
 		pop rbx
 		add rsp, 0x10
 		ret