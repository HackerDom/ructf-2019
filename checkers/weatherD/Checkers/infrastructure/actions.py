import sys
import inspect
from typing import Tuple, Type
from .verdict import Verdict
from traceback import format_exc


CHECK, PUT, GET, INFO = "CHECK", "PUT", "GET", "INFO"


class Checker:
    INFO: str = None
    __multiple_actions = frozenset((PUT, GET))
    __actions_handlers = {
        CHECK: None,
        PUT: {},
        GET: {},
    }

    @staticmethod
    def __check_function(func: callable, annotations_mask=None or Tuple[Type]):
        func_name = func.__code__.co_name
        func_args_spec = inspect.getfullargspec(func)

        if func_args_spec.annotations.get("return") != Verdict:
            raise TypeError(f"Checker function ({func_name}) should return {Verdict} object!")

        if annotations_mask is None:
            return

        if len(annotations_mask) != len(func_args_spec.args):
            raise TypeError(f"Checker function ({func_name}) should have {len(annotations_mask)} arg(s)!")

        for i, annotated_type in enumerate(annotations_mask):
            current_arg = func_args_spec.args[i]
            current_arg_type = func_args_spec.annotations.get(current_arg)
            if current_arg_type != annotated_type:
                raise TypeError(f"Checker function ({func_name}) argument type at {i + 1} pos "
                                f"should be {annotated_type}, "
                                f"not {current_arg_type}!")

    @classmethod
    def __register_action(cls, action_name: str, action: callable, action_period: int = None):
        if action_name in cls.__multiple_actions:
            if action_period is None:
                raise ValueError("Period should not be None for multiple actions!")
            cls.__actions_handlers[action_name][action_period] = action
        else:
            if action_name in cls.__actions_handlers:
                if cls.__actions_handlers[action_name] is not None:
                    raise ValueError("Action has been already registered!")
                cls.__actions_handlers[action_name] = action
            else:
                raise ValueError("Incorrect action name!")

    @staticmethod
    def define_check(func: callable) -> callable:
        Checker.__check_function(func, (str,))
        Checker.__register_action(CHECK, func)
        return func

    @staticmethod
    def define_put(vuln_num: int) -> callable:
        if not isinstance(vuln_num, int) or vuln_num < 1:
            raise TypeError(f'You should provide vulnerability natural number as a decorator argument!')

        def wrapper(func: callable):
            Checker.__check_function(func, (str, str, str))
            Checker.__register_action(PUT, func, vuln_num)
            return func
        return wrapper

    @staticmethod
    def define_get(vuln_num: int) -> callable:
        if not isinstance(vuln_num, int) or vuln_num < 1:
            raise TypeError(f'You should provide vulnerability natural number as a decorator argument!')

        def wrapper(func: callable):
            Checker.__check_function(func, (str, str, str))
            Checker.__register_action(GET, func, vuln_num)
            return func
        return wrapper

    # noinspection PyProtectedMember
    @staticmethod
    def run(*args):
        result = Verdict.CHECKER_ERROR("", "Something gone wrong")
        try:
            if not args:
                args = sys.argv[1:]
            result = Checker.__run(*args)

            if type(result) != Verdict:
                result = Verdict.CHECKER_ERROR("", f'Checker function returned not Verdict value, we need to fix it!')
        except Exception as e:
            result = Verdict.CHECKER_ERROR('', f"Checker caught an error: {e},\n {format_exc()}")
        finally:
            if result._public_message:
                print(result._public_message, file=sys.stdout)
            if result._private_message:
                print(result._private_message, file=sys.stderr)
            sys.exit(result._code)

    @staticmethod
    def __run(command=None, hostname=None, flag_id=None, flag=None, vuln_id=None) -> Verdict:
        commands = [CHECK, PUT, GET, INFO]

        if command is None:
            raise ValueError("Expected 1 or more args!")

        command = command.upper()

        if command not in commands:
            raise ValueError(f"Unknown ({command}) command! (Expected one of ({','.join(commands)})")

        if command == INFO:
            return Verdict.OK(Checker.INFO)

        if hostname is None:
            raise ValueError("Can't find 'hostname' arg! (Expected 2 or more args)")

        check_func = Checker.__actions_handlers[CHECK]
        if command == CHECK:
            # noinspection PyCallingNonCallable
            return check_func(hostname)

        if flag_id is None:
            raise ValueError("Can't find 'flag_id' arg! (Expected 3 or more args)")

        if flag is None:
            raise ValueError("Can't find 'flag' arg (Expected 4 or more args)")

        if vuln_id is None:
            raise ValueError("Can't find 'vuln_id' arg (Expected 5 or more args)")
        try:
            vuln_id = int(vuln_id)
            assert vuln_id > 0
            assert vuln_id in Checker.__actions_handlers[PUT]
            assert vuln_id in Checker.__actions_handlers[GET]
        except (TypeError, AssertionError):
            raise ValueError("'vuln_id' should be representative as a natural number, "
                             "GET/PUT methods should be registered in checker!")

        put_func = Checker.__actions_handlers[PUT][vuln_id]
        get_func = Checker.__actions_handlers[GET][vuln_id]
        if command == PUT:
            return put_func(hostname, flag_id, flag)

        if command == GET:
            return get_func(hostname, flag_id, flag)

        raise RuntimeError("Something gone wrong with checker scenario :(")
