from api import Api
from infrastructure.actions import Checker
from infrastructure.verdict import Verdict
from utils.http import build_session
from utils.randomizer import Randomizer
from words import get_text
from zip_utils import create_zip, create_flag_zip

Checker.INFO = "3:1"


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

            resp = api.create_note(get_text(), True)
            if resp.status_code != 201:
                return Verdict.MUMBLE("Can't create note", "Can't create note")

            return Verdict.OK()
    except Exception as e:
        return Verdict.DOWN("Can't connect to service", str(e))


@Checker.define_put(vuln_num=1)
def put_zip_flag(host: str, flag_id: str, flag: str) -> Verdict:
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
def get_zip_flag(host: str, flag_id: str, flag: str) -> Verdict:
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


@Checker.define_put(vuln_num=2)
def put_note_flag(host: str, flag_id: str, flag: str) -> Verdict:
    try:
        with build_session() as session:
            api = Api(host, session)

            user = Randomizer.user()
            resp = api.register_user(user)
            if resp.status_code != 201:
                return Verdict.MUMBLE("Can't register user", "Can't register user")

            resp = api.create_note(flag, False)
            if resp.status_code != 201:
                return Verdict.MUMBLE("Can't create note", "Can't create note")

            return Verdict.OK(f"{user['login']}:{user['pwd']}")
    except Exception as e:
        return Verdict.DOWN("Can't connect to service", str(e))


@Checker.define_get(vuln_num=2)
def get_note_flag(host: str, flag_id: str, flag: str) -> Verdict:
    try:
        with build_session() as session:
            api = Api(host, session)

            l, p = flag_id.split(':')
            user = {'login': l, 'pwd': p}
            resp = api.register_user(user)
            if resp.status_code != 201:
                return Verdict.MUMBLE("Can't login", "Can't login")

            resp = api.get_notes(False)
            if flag not in resp.text:
                return Verdict.CORRUPT("Can't find flag", "Can't find flag from notes")

            return Verdict.OK()
    except Exception as e:
        return Verdict.DOWN("Can't connect to service", str(e))


if __name__ == '__main__':
    Checker.run()
