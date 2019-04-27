import psycopg2
import datetime
import random
from psycopg2.extensions import AsIs

CREDS = {'host': 'localhost', 'port': 5432, 'user': 'postgres', 'password': 'postgres', 'dbname': 'checker'}


def get_free_worker():
    return _execute("select host, port from workers where state='free' limit 1;", [])


# state 'free' for worker will be set automatically
def remove_job(team_ip, vuln):
    _execute("delete from jobs where team_ip=%s and vuln=%s;", (team_ip, vuln))


# state 'busy' for worker will be set automatically
def add_job(w_host, w_port, team_ip, flag_id, flag, vuln):
    try:
        worker_id = get_worker_id_by_host(w_host, w_port)[0][0]
    except:
        _print_exception(f"Can't find id of worker at {w_host}:{w_port} in table workers")
        return
    _execute("insert into jobs (worker_id, team_ip, flag_id, flag, vuln) "
             "values (%s, %s, %s, %s, %s);", (worker_id, team_ip, flag_id, flag, vuln))


def get_worker_id_by_host(host, port):
    return _execute("select id from workers where host=%s and port=%s;", (host, port))


def get_worker_id_by_team(team_ip, vuln):
    return _execute("select worker_id from jobs where team_ip=%s and vuln=%s;", (team_ip, vuln))


def get_worker_state(host, port):
    return _execute("select state from workers where host=%s and port=%s;", (host, port))


# Если в таблице есть старая джоба, не удаленная почему-то,
# это значит что ее воркер нерабочий (id в таблице уникальны).
# Скажем всем что он даун и что команде нужен новый воркер.
def is_processing_by_worker(team_ip, vuln):
    max_working_time = 60
    result = _execute("select worker_id, init_time from jobs where team_ip=%s and vuln=%s;", (team_ip, vuln))
    if result:
        worker_id, init_time = result[0][0], result[0][1]
        if init_time < (datetime.datetime.now() - datetime.timedelta(seconds=max_working_time)).time():
            set_down_state(worker_id)
            return False
        return True
    return False


def get_team_state(team_ip, vuln):
    return _execute("select put from teams_state where team_ip=%s and vuln=%s;", ( team_ip, vuln))


# makes state of worker directly to down (use ONLY THIS function for that purpose)
def set_down_state(worker_id):
    data = _execute("select flag_id, flag from jobs where worker_id=%s;", (worker_id,))
    try:
        flag_id, flag = data[0][0], data[0][1]
    except:
        flag_id, flag = None, None
    _execute("delete from jobs where worker_id=%s; "
             "update workers set state='down' where id=%s;", (worker_id, worker_id))
    return flag_id, flag


# time updates automatically
def update_team_state(team_ip, vuln, put_state=None, get_state=None):
    data = _execute("select * from teams_state where team_ip=%s and vuln=%s;", (team_ip, vuln))
    if not data:
        _execute('insert into teams_state (team_ip, vuln, put, get) values (%s, %s, %s, %s);',
                 (team_ip, vuln, put_state, get_state))
    else:
        if put_state:
            _execute("update teams_state set put=%s where team_ip=%s and vuln=%s;", (put_state, team_ip, vuln))
        if get_state:
            _execute("update teams_state set get=%s where team_ip=%s and vuln=%s;", (get_state, team_ip, vuln))


def add_free_worker(host, port):
    _execute("insert into workers (state, host, port) values ('free', %s, %s)"
             "on conflict (host, port) do update set state='free';", (host, port))


def get_oldest_refresh():
    return _execute("select refresh_time, team_ip, vuln from teams_state order by refresh_time limit 1;", [])


def _execute(query, params):
    try:
        conn = psycopg2.connect(**CREDS)
        conn.autocommit = True
        with conn.cursor() as cursor:
            cursor.execute(query, params)
            data = cursor.fetchall() if query[:6] == 'select' else None
        conn.close()
        return data
    except Exception as e:
        _print_exception(e)


def _print_exception(e):
    print('\n*** DB ERROR ***')
    print(e)
    print('****************\n')


# if __name__ == '__main__':
#     # testing
#     x, y = get_coords('checker')

