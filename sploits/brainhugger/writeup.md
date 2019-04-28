# BrainHugger

BrainHugger service is online [brainfuck](https://en.wikipedia.org/wiki/Brainfuck)-interpreter.

## Vulnerability #1 (flag is a password)
### Cookies
In this service cookies is a pair of `uid` and `secret`. Secret is a string `uid|password` encrypted with [cbc](https://en.wikipedia.org/wiki/Block_cipher_mode_of_operation#Cipher_Block_Chaining_(CBC)) algorithm with [padding PKCS-7](https://en.wikipedia.org/wiki/PKCS) and unique-generated key for each user. That's why all valid cookies for one user are same. Server keeps exactly this secret instead of password.
### Cookie leaking
As we can see, in function `handleLoginUser:163`, cookies are validates, but then, if it's valid,
**another** valid cookie will be taken from the storage by given userId. So, if we have valid cookies, we can get valid cookies with any userId.

### Exploiting
At first, we register a user and get valid cookies. Then, we steal a secret of target user with the cookie leaking. We know, that secret is cbc-encrypted cipher-text. Also, we can check it secret by checking cookie with `handleTask`. Every cookie checking service will decrypt cyphertext with needed key. There is three possible answers for checking cookie: valid cookie (HTTP 200), invalid cookie with valid padding (HTTP 403) and invalid cookie with invalid padding (HTTP 400). Consequently, we can use a [Padding oracle attack](https://en.wikipedia.org/wiki/Padding_oracle_attack) for getting plain secret, which conatins a user password.

### How to defend
Just remove different HTTP response codes for valid and invalid cookies while checking.

