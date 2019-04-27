import requests
from requests import ConnectionError, HTTPError
import db_manager
import threading
import datetime
import time
from flask import Flask, request
from flask_restful import reqparse

OK, CORRUPT, MUMBLE, DOWN, CHECKER_ERROR = 101, 102, 103, 104, 110

server = Flask(__name__)
parser = reqparse.RequestParser()
parser.add_argument('team_ip')
parser.add_argument('flag_id')
parser.add_argument('flag')
parser.add_argument('vuln')

SAFETY_FLAG = 0


def put(team_ip, flag_id, flag, vuln):
    global SAFETY_FLAG
    db_manager.check_down_worker(team_ip, vuln)
    worker = get_free_worker()

    if worker is None:
        print('\n*** FATAL: NO FREE WORKERS ***\n')
        return get_team_state(team_ip, vuln)
        # return '110 // Checker error // No free workers', 404

    try:
        req = requests.put(f'http://{worker[0]}:{worker[1]}',
                           data={'team_ip': team_ip, 'flag_id': flag_id, 'flag': flag, 'vuln': vuln},
                           timeout=10)

        if req.status_code != 200:
            raise ConnectionError
        print('Worker initialized')
        time.sleep(3)  # for worker to fill db
        SAFETY_FLAG = 0

    except ConnectionError or HTTPError:
        print('\n*** ERROR: BAD RESPONSE FROM WORKER ***\n')
        SAFETY_FLAG += 1
        if SAFETY_FLAG >= 8:
            return '110 // Checker error // Too many bad responses from workers'
        bad_worker = db_manager.get_worker_id_by_host(worker[0], worker[1])[0][0]
        db_manager.set_down_state(bad_worker)
        return reinit_worker(team_ip, flag_id, flag, vuln)

    except Exception as e:
        print(f"\n*** FATAL: {e}***\n")
        return f'110 // Error // Error: {e}', 404

    return get_team_state(team_ip, vuln)


def get_team_state(team_ip, vuln):
    state = db_manager.get_team_state(team_ip, vuln)
    if state:
        if int(state[0][0]) == 101:
            return '101', 200
        else:
            return f'{state[0][0]} // no info // no info', 404
    else:
        return '110 // checker error // error', 404


def get_free_worker():
    worker = db_manager.get_free_worker()
    return worker[0] if worker else None


@server.route('/', methods=["PUT"])
def handle_request():
    args = parser.parse_args()
    if None in args.values():
        return 'Incorrect arguments.', CHECKER_ERROR
    result = put(**args)
    print(result)
    return result


def reinit_worker(team_ip, flag_id, flag, vuln):
    print('\n*** WARNING: Found bad worker! Initializing new... ***\n')
    return put(team_ip, flag_id, flag, vuln)


def control_workers_state():
    max_time_inactive = 20
    while True:
        try:
            data = db_manager.get_oldest_refresh()
            if not data:
                time.sleep(3)
                continue

            oldest_refresh, team_ip, vuln = data[0][0], data[0][1], data[0][2]

            if oldest_refresh < (datetime.datetime.now() - datetime.timedelta(seconds=max_time_inactive)).time():
                bad_worker = db_manager.get_worker_id_by_team(team_ip, vuln)

                # возможна ситуация когда рефреш_тайм старое но на эту тиму нет записи в джобс.
                # Это возможно если воркер еще не переинициализировался, но джоба уже удалилась.
                # Если это происходит один-два раза то это нормальное поведение программы.
                # Если простой занимает много времени, то возможно закончились свободные воркеры.
                # Если они не закончились но что-то все равно идет не так,
                # то надо подождать следующего пинка от чекера с командой put. Должно наладиться.
                # Если не наладилось - надо проводить расследование что за фигня
                if bad_worker:
                    flag_id, flag = db_manager.set_down_state(bad_worker[0][0])
                    if flag_id and flag:
                        reinit_worker(team_ip, flag_id, flag, vuln)
                    continue
                else:
                    # Стоит спать только если не нашлось падших воркеров
                    time.sleep(3)
                    continue

            time.sleep(3)
        except:
            continue


if __name__ == '__main__':
    threading.Thread(target=control_workers_state).start()
    server.run(debug=False, host='0.0.0.0', port=5555)
