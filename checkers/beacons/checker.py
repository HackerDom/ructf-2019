#!/home/cs/miniconda3/bin/python3

import requests
import generator
import beacons_api
from infrastructure.actions import Checker
from infrastructure.verdict import Verdict

COORDINATOR = ('10.10.10.97', 5555)
Checker.INFO = 'vulns: 1'
STATES = {101: Verdict.OK, 102: Verdict.CORRUPT, 103: Verdict.MUMBLE, 104: Verdict.DOWN, 110: Verdict.CHECKER_ERROR}


@Checker.define_check
def on_check(team_ip: str) -> Verdict:
    private_result = False
    public_result = False
    sharing_result = False

    # check adding pictures, sharing links and devices
    image_name, device = generator.get_image()
    try:
        for i in range(6):
            user, password = generator.generate_userpass(None)[0]
            cookie = beacons_api.register_user(team_ip, user, password)
            if cookie:
                break
            if i == 5:
                return Verdict.MUMBLE("Can't register new user", "Can't register new user")
    except requests.exceptions.ConnectionError:
        return Verdict.DOWN('down', 'ConnectionError')

    # могжет не подойти название, коммент или координаты оказаться использованными
    for i in range(5):
        name = generator.generate_beacon_name()
        private_x, private_y = generator.generate_coords()
        x, y = generator.generate_coords()
        private_comment = generator.generate_comment()
        public_comment = generator.generate_comment()
        beacon_id_private = beacons_api.add_beacon(team_ip, cookie, private_x, private_y, name, private_comment)
        beacon_id_public = beacons_api.add_beacon(team_ip, cookie, x, y, name, public_comment, False)
        if beacon_id_private and beacon_id_public:
            break
        if i == 4:
            return Verdict.MUMBLE("Can't add new beacon", "Can't add new beacon")
    invite_code = beacons_api.get_beacon_invite(team_ip, cookie, beacon_id_private)
    if not invite_code:
        return Verdict.MUMBLE("Can't get invite code", "Can't get invite code")
    image_id_private = beacons_api.upload_image(team_ip, cookie, beacon_id_private, image_name)
    image_id_public = beacons_api.upload_image(team_ip, cookie, beacon_id_public, image_name)
    if not image_id_private or not image_id_public:
        return Verdict.MUMBLE("Can't upload image", "Can't upload image")

    images = beacons_api.get_image_ids(team_ip, cookie, beacon_id_private)
    if image_id_private in [img['id'] for img in images]:
        print(1)
        private_result = True

    logout = beacons_api.logout(team_ip, cookie)
    if logout != 200:
        return Verdict.MUMBLE('no logout', 'no logout')

    # check availability of public image
    for i in range(4):
        new_user, new_pass = generator.generate_userpass(None)[0]
        another_user_cookie = beacons_api.register_user(team_ip, new_user, new_pass)
        if another_user_cookie:
            break
        if i == 3:
            return Verdict.MUMBLE("Can't register user", "Can't register user")
    images = beacons_api.get_image_ids(team_ip, another_user_cookie, beacon_id_public)
    if image_id_public in [img['id'] for img in images]:
        print(2)
        public_result = True

    # check the possibility to get shared beacons
    is_private = beacons_api.get_shared_beacon(team_ip, another_user_cookie, invite_code)
    if not is_private:
        print(3)
        sharing_result = True

    logout = beacons_api.logout(team_ip, another_user_cookie)
    if logout != 200:
        return Verdict.MUMBLE('no logout', 'no logout')

    if private_result and public_result and sharing_result:
        return Verdict.OK()

    return Verdict.MUMBLE("Can't get images", "Can't get images")


# потом сделать по номерам уязвимостей
@Checker.define_put(vuln_num=1)
def on_put(team_ip: str, flag_id: str, flag: str) -> Verdict:
    global STATES

    print('\nputting')
    for i in range(6):
        try:
            user, password = generator.generate_userpass(flag_id)[0]
            session = beacons_api.register_user(team_ip, user, password)
        except requests.exceptions.ConnectionError:
            return Verdict.DOWN('down', 'ConnectionError')
        if session:
            break
        if i == 5:
            return Verdict.MUMBLE("Can;t register user", '')
    beacon_name = generator.generate_beacon_name()
    for i in range(7):
        x, y = generator.generate_coords()
        beacon_id = beacons_api.add_beacon(team_ip, session, x, y, beacon_name, flag)
        if beacon_id:
            return Verdict.OK()
    return Verdict.MUMBLE("Can't add new beacon", '')


    # try:
    #     req = requests.put(f'http://{COORDINATOR[0]}:{COORDINATOR[1]}',
    #                        data={'team_ip': team_ip, 'flag_id': flag_id, 'flag': flag, 'vuln': 1},
    #                        timeout=10)
    #     result = req.text.split('//')
    #     # print(result)
    #     code = int(result.pop(0))
    #     return STATES[code](*result)
    # except requests.exceptions.ConnectionError:
    #     return Verdict.DOWN('down', 'ConnectionError')
    # except:
    #     return Verdict.CHECKER_ERROR('', 'returned bad format')


@Checker.define_get(vuln_num=1)
def on_get(team_ip: str, flag_id: str, flag: str) -> Verdict:
    user, password = generator.generate_userpass(flag_id)[0]
    try:
        cookie = beacons_api.sign_in(team_ip, user, password)
    except requests.exceptions.ConnectionError:
        return Verdict.DOWN('down', 'ConnectionError')
    if not cookie:
        return Verdict.MUMBLE('', "no session cookie, can't sign in")
    beacons = beacons_api.get_all_user_beacons(team_ip, cookie)
    if not beacons:
        return Verdict.MUMBLE('', 'no beacon')
    comments = []
    for id in beacons:
        comments.append(beacons_api.get_beacon_comment(team_ip, cookie, id))
    if flag in comments:
        print(comments)
        print(flag)
        return Verdict.OK()
    return Verdict.CORRUPT('', 'hz')


if __name__ == '__main__':
    Checker.run()
