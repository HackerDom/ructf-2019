import time
import threading
import requests
import beacons_api
import sys  # for args from console
import random  # for testing
import generator
import db_manager
import phantom_logic
from flask import Flask, request
from flask_restful import reqparse


OK, CORRUPT, MUMBLE, DOWN, CHECKER_ERROR = 101, 102, 103, 104, 110
HOST, PORT = None, None

parser = reqparse.RequestParser()
parser.add_argument('team_ip')
parser.add_argument('flag_id')
parser.add_argument('flag')
parser.add_argument('vuln')


class Worker:
    def __init__(self, team_ip, flag_id, flag, vuln):
        self.team_ip = team_ip
        self.flag_id = flag_id
        self.flag = flag
        self.vuln = vuln
        self.put_state = None
        self.get_state = None

    def begin_loop(self, loop_seconds):
        loop_thread = threading.Thread(target=self._begin_loop, args=[loop_seconds])
        loop_thread.start()

    def _begin_loop(self, loop_seconds):
        users = generator.generate_userpass(self.flag_id)
        start_time = time.time()
        for userpass in users[1:]:
            user, password = userpass[0], userpass[1]
            # print(user)
            # print(password)
            if time.time() < start_time + loop_seconds:
                # Если воркера объявили дауном, значит и работу отобрали.
                if self.get_current_state() == 'down':
                    db_manager.add_free_worker(HOST, PORT)
                    return
                self.put(user, password)
                self.get(user, password)
                self.send_state()

        print('finished')
        db_manager.remove_job(self.team_ip, self.vuln)

    def put(self, user, password):
        print('\nputting')
        for i in range(6):
            session = beacons_api.register_user(self.team_ip, user, password)
            if session:
                break
            if i == 5:
                self.put_state = 103
                print("Can't register")
                print(self.put_state)
                return f"{self.put_state} // Can't sign in or register user"
        beacon_name = generator.generate_beacon_name()
        for i in range(7):
            x, y = generator.generate_coords()
            beacon_id = beacons_api.add_beacon(self.team_ip, session, x, y, beacon_name, self.flag)
            if beacon_id:
                self.put_state = 101
                print(self.put_state)
                return str(self.put_state)
        self.put_state = 103
        print(self.put_state)
        return f"{self.put_state} // Can't add new beacon"

    # get state?
    def get(self, user, password):
        print('\ngetting')
        data = phantom_logic.make_request(self.team_ip, user, password)
        print(data)
        self.get_state = data['code']

    def send_state(self):
        db_manager.update_team_state(self.team_ip, self.vuln, self.put_state, self.get_state)

    def get_current_state(self):
        state = db_manager.get_worker_state(HOST, PORT)
        return state[0][0] if state else None


def handle_request():
    args = parser.parse_args()
    if None in args.values():
        return '110 // Checker error // Incorrect arguments', 404

    w = Worker(**args)
    user, password = generator.generate_userpass(args['flag_id'])[0]
    result = w.put(user, password)
    w.begin_loop(60)
    db_manager.add_job(HOST, PORT, **args)
    return result, 200


def ping():
    return 'Listening', 200


if __name__ == '__main__':
    server = Flask(__name__)
    server.route('/', methods=["PUT"])(handle_request)
    server.route('/', methods=["GET"])(ping)
    HOST = sys.argv[1]
    PORT = int(sys.argv[2])
    # HOST = '0.0.0.0'
    # PORT = 5000
    db_manager.add_free_worker(HOST, PORT)
    server.run(debug=False, host=HOST, port=PORT)
