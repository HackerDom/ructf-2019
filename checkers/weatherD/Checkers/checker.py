from infrastructure.actions import Checker
from infrastructure.verdict import Verdict


Checker.INFO = "1:2"


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


if __name__ == '__main__':
    Checker.run()
