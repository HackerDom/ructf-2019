# BrainHugger

BrainHugger service is online [brainfuck](https://en.wikipedia.org/wiki/Brainfuck)-interpreter.

## Vulnerability #1 (padding oracle, flag is a password)
### Cookies
In this service cookies is a pair of `uid` and `secret`. Secret is a string `uid|password` encrypted with [cbc](https://en.wikipedia.org/wiki/Block_cipher_mode_of_operation#Cipher_Block_Chaining_(CBC)) algorithm with [padding PKCS-7](https://en.wikipedia.org/wiki/PKCS) and unique-generated key for each user. That's why all valid cookies for one user are same. Server keeps exactly this secret instead of password.
### Cookie leaking
As we can see, cookies validates in function `handleLoginUser`, but then, if it's valid,
**another** valid cookie will be taken from the storage by given userId and given in response. So, if we have valid cookies, we can get valid cookies with any userId.

### Exploiting
At first, we register a user and get valid cookies. Then, we steal a secret of the target user with the cookie leaking. We know, that secret is cbc-encrypted cipher-text. Also, we can check it secret by checking cookie with `handleTask`. Every cookie checking service will decrypt cyphertext with needed key. There is three possible answers for checking cookie: valid cookie (HTTP 200, but without right token is HTTP 403), invalid cookie with valid padding (HTTP 403) and invalid cookie with invalid padding (HTTP 400). Consequently, we can use a [Padding oracle attack](https://en.wikipedia.org/wiki/Padding_oracle_attack) for getting plain secret, which conatins a user password.

### Ways to defend
* Remove different HTTP response codes for valid and invalid cookies while checking.
* Change *initializing vector* for cbc.
* Use another cryptography method.

## Vulnerability #2 (buffer overflow, flag is a token)
### Brainhugger executor
Binary executable file `bhexecutor` is a simple brainfuck interpreter written in C with some mistakes:
1) Interpreter doesn't check **out of range** situation while incrementing a **data pointer**.
2) There is an excess buffer for output, which allocates on the heap with [mmap syscall](http://man7.org/linux/man-pages/man2/mmap.2.html) with `PROT_READ | PROT_WRITE | PROT_EXEC` parameter. It means, that we can execute any code from there.
3) Excess buffer doesn't munmapping at the end (yes, it's a memory leak).
4) [ASLR](https://en.wikipedia.org/wiki/Address_space_layout_randomization) is disabled.

### Exploiting
#### Creating a shellcode
We need to create a shellcode. There is a two ways: use [Metasploit Framework](https://en.wikipedia.org/wiki/Metasploit_Project#Metasploit_Framework) or we can write it in assembler language (or even C), but it's important to use only **relative** addresses. Shellcode can perform any useful action, for example, give us a **reverse shell**.
#### Some source code here
There is a function, which executes a brainfuck code.
```
int run_bh_code(<signature>) {  
    ...
    char cells[MEMORY_SIZE];   // on the stack!
    ...
    // excess buffer
    char* output_ptr = (char*)mmap(
        NULL,
        max_output_len,
        PROT_READ | PROT_WRITE | PROT_EXEC,
        MAP_PRIVATE | MAP_ANONYMOUS,
        -1,
        0
    );
    ...
    memcpy(output, output_ptr, max_output_len);
    // output is char* passed from parameters
    // absence of munmap
    return 0;
}
```
#### Return address
To perform our useful action, we need need to change **return address** to address of our shellcode, which located in buffer for copying to the stdout. Fortunately, we can move data pointer in brainfuck in any place, which address bigger, than address of excess buffer (due to our mistake). It means, we can change any data on the stack as we want. As we know, all return addresses keeps on the stack and we can change return address of function `run_bh_code` to address of executable buffer, but it's need to find offset between `cells` and return address of `run_bh_code`. We can do it in any debugger, for example gdb.
#### Performing a shellcode
Now we need write a program in brainfuck, which prints shellcode to output and then changing return address to executable buffer.

### Sploits
Watch `op_attack.go` for working sploit for Padding Oracle attack and `bo_sttack.py`.
