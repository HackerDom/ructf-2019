import random
from typing import Tuple

from api import Api
from infrastructure.actions import Checker
from infrastructure.verdict import Verdict
from utils.http import build_session
from utils.randomizer import Randomizer
from io import BytesIO
from zipfile import ZipFile

Checker.INFO = "1"


def create_zip() -> Tuple[str, str, bytes]:
    b = BytesIO()
    filename = f'{Randomizer.word()}.zip'
    file_in_zip = Randomizer.word()
    with ZipFile(b, 'w') as z:
        z.filename = filename
        z.writestr(file_in_zip, Randomizer.word())

    return file_in_zip, filename, b.getvalue()


def create_flag_zip(flag) -> Tuple[str, bytes]:
    b = BytesIO()
    is_flag_in = False
    filename = f'{Randomizer.word(20)}.zip'
    with ZipFile(b, 'w') as z:
        n = random.randint(2, 4)
        for i in range(n):
            for ii in range(random.randint(1, 3)):
                if (not is_flag_in and random.randint(1, 2) % 2 == 0):
                    z.writestr(flag, Randomizer.word())
                    is_flag_in = True
                else:
                    z.writestr(Randomizer.word(), Randomizer.word())

    return filename, b.getvalue()

@Checker.define_check
def check_service(host: str) -> Verdict:
    try:
        with build_session() as session:
            api = Api(host, session)

            resp = api.register_user(Randomizer.user())
            if resp.status_code != 201:
                return Verdict.MUMBLE("Can't register user", "Can't register user")

            file_in_zip, *file = create_zip()
            resp = api.upload_zip(file)
            if resp.status_code != 202:
                return Verdict.MUMBLE("Can't upload file", "Can't upload file")

            resp = api.search_file(file_in_zip)
            if file_in_zip not in resp.text:
                return Verdict.MUMBLE("Can't find file from zip", "Can't find file from zip")

            return Verdict.OK()
    except Exception as e:
        return Verdict.DOWN("Can't connect to service", str(e))



@Checker.define_put(vuln_num=1)
def put_flag_into_the_service(host: str, flag_id: str, flag: str) -> Verdict:
    try:
        with build_session() as session:
            api = Api(host, session)

            user = Randomizer.user()
            resp = api.register_user(user)
            if resp.status_code != 201:
                return Verdict.MUMBLE("Can't register user", "Can't register user")

            resp = api.upload_zip(create_flag_zip(flag))
            if resp.status_code != 202:
                return Verdict.MUMBLE("Can't upload file", "Can't upload file")

            return Verdict.OK(f"{user['login']}:{user['pwd']}")
    except Exception as e:
        return Verdict.DOWN("Can't connect to service", str(e))


@Checker.define_get(vuln_num=1)
def get_flag_from_the_service(host: str, flag_id: str, flag: str) -> Verdict:
    try:
        with build_session() as session:
            api = Api(host, session)

            l, p = flag_id.split(':')
            user = {'login': l, 'pwd': p}
            resp = api.register_user(user)
            if resp.status_code != 201:
                return Verdict.MUMBLE("Can't login", "Can't login")

            resp = api.search_file(flag)
            if flag not in resp.text:
                return Verdict.CORRUPT("Can't find flag", "Can't find flag from zip")

            return Verdict.OK()
    except Exception as e:
        return Verdict.DOWN("Can't connect to service", str(e))


if __name__ == '__main__':
    # Checker.run()
    print(check_service("10.33.64.130:5000")._code)
