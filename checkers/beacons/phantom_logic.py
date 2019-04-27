from selenium.webdriver import PhantomJS
from phantom_js import \
    get_driver, DriverInitializationException, DriverTimeoutException
import traceback
import beacons_api
import random  # for debug
import generator

# temp
OK, CORRUPT, MUMBLE, DOWN, CHECKER_ERROR = 101, 102, 103, 104, 110
SERVICE_PORT = 8000


def make_request(team_ip, user, password):
    try:
        with get_driver() as driver:
            return run_get_logic(driver, team_ip, user, password)
    except DriverInitializationException as e:
        return {
            "code": 110,
            "private": "Couldn't init driver due to {} with {}".format(
                e, traceback.format_exc()
            )
        }
    except DriverTimeoutException as e:
        return {
            "code": 103,
            "public": "Service response timed out!",
            "private": "Service response timed out due to {}".format(e)
        }
    except Exception as e:
        return {
            "code": 110,
            "private": "ATTENTION!!! Unhandled error: {}".format(e)
        }


# team_ip is without port
def run_get_logic(driver: PhantomJS, team_ip, user, password):
    # if team_ip == '127.0.0.1':
    #     team_ip = 'localhost'
    session_cookie = beacons_api.sign_in(team_ip, user, password)
    print('cookie: ' + session_cookie)
    beacons = beacons_api.get_all_user_beacons(team_ip, session_cookie)
    if not beacons:
        print('no beacons')
        return {"code": 103}
    print('beacons: ' + str(beacons))
    driver.get(f"http://{team_ip}:{SERVICE_PORT}/")
    driver.add_cookie({
        'name': 'session',
        'value': session_cookie,
        'domain': "." + team_ip,
        'path': '/'
    })
    for beacon_id in beacons:
        driver.get(f'http://{team_ip}:{SERVICE_PORT}/Beacon/{beacon_id}')
        print(driver.current_url)
        # print(driver.page_source)
        if beacon_id in driver.current_url:
            print('fine')

    return {"code": 101}


if __name__ == '__main__':
    # testing
    seed = '928akl23skkk43f4hjdse83ueje89n0000'
    u, p = generator.generate_userpass(seed)[4]
    s = beacons_api.register_user('127.0.0.1', u, p)
    print(s)
    beacons_api.add_beacon('localhost', s, 8439, 9099, 'ddud', 'frfrfrfr')
    aaa = make_request('localhost', u, p)
