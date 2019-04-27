from infrastructure.actions import Checker
from infrastructure.verdict import Verdict


Checker.INFO = "1:2"  # means vulns distribution


@Checker.define_check
def check_service(host: str) -> Verdict:
    ...  # your code

    return Verdict.OK()


@Checker.define_put(vuln_num=1)
def put_flag_into_the_service(host: str, flag_id: str, flag: str) -> Verdict:
    ...  # your code

    return Verdict.OK("my_new_flag_id")


@Checker.define_get(vuln_num=1)
def get_flag_from_the_service(host: str, flag_id: str, flag: str) -> Verdict:
    ...  # your code

    return Verdict.OK()


@Checker.define_put(vuln_num=2)
def put_flag_into_the_service(host: str, flag_id: str, flag: str) -> Verdict:
    ...  # your code

    return Verdict.MUMBLE("something bad with ur proto", "they are cheating!")


@Checker.define_get(vuln_num=2)
def get_flag_from_the_service(host: str, flag_id: str, flag: str) -> Verdict:
    ...  # your code

    return Verdict.CORRUPT("flag lost", "lol, they lost it")


if __name__ == '__main__':
    Checker.run()
