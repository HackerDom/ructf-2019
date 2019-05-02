# Beacons

### Description.

You're given a map on which you can put some tags - beacons. To add new beacon you need to specify its name and write a comment. Also, you can make it private. When the beacon is created, you can attach pictures to it. If it is private, the name, comment and pictures are hidden for other users but you can give them invite code to see all of that.

## Attacks

### First vuln.

The first vulnerability was in the [session.py](https://github.com/HackerDom/ructf-2019/blob/master/services/beacons/sessions.py). If you pay attention to the generation of cookies for the session you can notice that it takes md5 hash of its current cookie, saves it again in *cookie* and returns that new cookie:

**session.py:**

```python
...
def _cookie_gen(self, cookie=''):
    while True:
        cookie = hashlib.md5(cookie.encode()).hexdigest()
        yield cookie
...
```

### Second vuln. 

This vulnerability was in the generation of invite codes for your private beacons. Take a look at the file [beacon.py](https://github.com/HackerDom/ructf-2019/blob/master/services/beacons/beacons/controllers/beacon.py) and its function add_beacon. You can notice that invite codes were taken from the database *User*, list *invites*. 

**beacon.py:**

```python
...
upserted_id = (await Beacon.update_one({"_id": ObjectId(get_random_id())}, {
              "$set": {"name": name,
                       "comment": comment,
                       "coord_x": coord_x,
                       "coord_y": coord_y,
                       "creator": str(user.id),
                       "photos": [],
                       "is_private": is_private,
                       "invite": (await get_invite_by_user(user)) if is_private else ''},
                       "$currentDate": {"createDate": {"$type": "date"}}}, upsert=True)
                   ).upserted_id
...

```

```python
...
async def get_invite_by_user(user):
    await User.update_one({"_id": user.id}, {"$pop": {"invites": -1}})
    return user.invites.pop(0)
...
```

So, you can see that invite codes were generated right at the time you add a new user. Let's look at the [login.py](https://github.com/HackerDom/ructf-2019/blob/master/services/beacons/beacons/controllers/login.py) and its function *signin*:

**login.py:**

```python
...
inserted_id = (await User.insert_one({"name": username, "password": password, 
                                      "beacons": [], "invites": get_invites()})).inserted_id
...
```

And then, the function *get_invites*:

**login.py:**

```python
...
def get_uuid_bytes():
    return bytes(reduce(concat, 
                        (getrandbits(32).to_bytes(4, 'little') for _ in range(4)), bytearray()))

def get_invites():
    return [str(UUID(bytes=get_uuid_bytes())) for _ in range(200)]
...
```

That means, the invite codes were generated from 4 bytes recieved as a result of calling *random.getrandbits*.
This function uses the [Mersenne twister](https://en.wikipedia.org/wiki/Mersenne_Twister) - the pseudorandom number generator. It is based on the  linear feedback shift register 624 elements long 32 bits each. When generating the next number, the register returns the next 32 bit number and shifts one cell. Then some reversible operations are beeing applied to the number obtained from the register. Thus after getting 624 generations of this [PRNG](https://en.wikipedia.org/wiki/Pseudorandom_number_generator) it is possible to get the current state of the generator and the ability to predict the next one. You see that the function *get_uuid_bytes* is called 200 times each retrieving 4 bytes, so it is 800 generations. That's enough for the unambiguous recovery of the generator state. With the help of this information it is easy to predict invite codes for new users and get access to their private beacons. PoC of this vulnerability is [here](https://github.com/HackerDom/ructf-2019/blob/master/sploits/beacons/PoC_invite.py).

### Third vuln.

In this service, the third vulnerability was planned, and it supposed to be related to the XSS.
If you point your cursor to the photo attached to the beacon, you see the name of it and the device, from which the photo was taken. You can make a suggestion that this information can be taken from EXIF tags of picture. But if you read the source code of the backend, you see the server only saves photos to the database and do nothing about EXIF tags. So, let's look at the frontend, file [all.js](https://github.com/HackerDom/ructf-2019/blob/master/services/beacons/beacons/static/js/all.js). The function *setDeviceModel* takes info about the device right from the picture and inserts it into *deviceModelElement.innerHTML* without any check. 

**all.js:**

```javascript
function setDeviceModel(imgElement, deviceModelElement, sizeElement) {
    EXIF.getData(imgElement, function() {
        var make = EXIF.getTag(this, "Make");
        var model = EXIF.getTag(this, "Model");
        var date = EXIF.getTag(this, "DateTime");
        if (make && model) {
            deviceModelElement.innerHTML = `device: ${make} ${model}`;
        }
        if (date) {
            sizeElement.innerHTML = `date: ${date}`;
        }
    });
}
```

So, the XSS is here.
This vuln was released and a special checker was prepared for it but, unfortunately, we were not able to implement the whole infrastucture for it at the contest, so it was not working. Also, for the most attentive participants our developers prepared a little gift - in the folder /beacons/static/images there was the PoC of this vuln in a form of a cute kitty :)
