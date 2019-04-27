import requests
import json
import generator
import traceback
import time
from functools import wraps

SERVICE_PORT = 8000


def handle_exception(function):
    @wraps(function)
    def wrapper(*args, **kwargs):
        try:
            return function(*args, **kwargs)
        except requests.exceptions.ConnectionError:
            raise requests.exceptions.ConnectionError
        except Exception as occurred:
            # print(occurred)
            # print(traceback.format_exc())
            return None

    return wrapper


@handle_exception
def upload_image(host, session, beacon_id, filename, folder='pictures/'):
    with open(f'{folder}{filename}', 'rb') as photo:
        photo_name = generator.generate_picture_name()
        image = {'photo': (photo_name, photo.read(), 'image/jpeg')}
        data = requests.post(f'http://{host}:{SERVICE_PORT}/Beacon/AddPhoto/{beacon_id}',
                             files=image,
                             timeout=10,
                             cookies={'session': session})
        return json.loads(data.content.decode())['id']


@handle_exception
def get_all_user_beacons(host, session):
    data = requests.get(f'http://{host}:{SERVICE_PORT}/Beacon/GetUserBeacons',
                        cookies={'session': session},
                        timeout=5).content.decode()
    return [item['id'] for item in json.loads(data)['beacons']]


@handle_exception
def register_user(host, user, password):
    session = requests.Session()
    session.post(f'http://{host}:{SERVICE_PORT}/Signup', data={'username': user, 'password': password}, timeout=5)
    return session.cookies.get_dict()['session']


@handle_exception
def sign_in(host, user, password):
    session = requests.Session()
    session.post(f'http://{host}:{SERVICE_PORT}/Signin', data={'username': user, 'password': password}, timeout=5)
    return session.cookies.get_dict()['session']


@handle_exception
def get_beacon_comment(host, session, beacon_id):
    data = requests.get(f'http://{host}:{SERVICE_PORT}/Beacon/{beacon_id}',
                        cookies={'session': session}, timeout=5).content.decode()
    return json.loads(data)['comment']


@handle_exception
def logout(host, session):
    req = requests.get(f'http://{host}:{SERVICE_PORT}/Logout',
                       cookies={'session': session}, timeout=5)
    return req.status_code


@handle_exception
def get_shared_beacon(host, session, invite):
    data = requests.post(f'http://{host}:{SERVICE_PORT}/Beacon/Invite',
                         data={'invite': invite},
                         cookies={'session': session}, timeout=5).content.decode()
    beacon_id = json.loads(data)['id']
    is_private = requests.get(f'http://{host}:{SERVICE_PORT}/Beacon/{beacon_id}',
                              cookies={'session': session}, timeout=5).content.decode()
    return json.loads(is_private)['is_private']


# @handle_exception
# def logout(host, session):
#     done = requests.

@handle_exception
def get_beacon_invite(host, session, beacon_id):
    data = requests.get(f'http://{host}:{SERVICE_PORT}/Beacon/{beacon_id}',
                        cookies={'session': session}, timeout=5).content.decode()
    return json.loads(data)['invite']


# @handle_exception
# def get_image_device(host, session, beacon_id):
#     print('asd')
#     data = requests.get(f'http://{host}:{SERVICE_PORT}/Beacon/{beacon_id}',
#                         cookies={'session': session}, timeout=5)
#     print('oooooo')
#     data = requests.get(f'http://{host}:{SERVICE_PORT}/',
#                         cookies={'session': session}, timeout=5)
#     print('sdsdfs')
#     print(data.content)
#     return json.loads(data)['device']


@handle_exception
def get_image_ids(host, session, beacon_id):
    data = requests.get(f'http://{host}:{SERVICE_PORT}/Beacon/{beacon_id}',
                        cookies={'session': session}, timeout=5).content.decode()
    return json.loads(data)['photos']


# unable to add beacon if another user has it
@handle_exception
def add_beacon(host, session, x, y, name, comment, private=True):
    if private:
        datadict = {'name': name,
                    'comment': comment,
                    'coord_x': x,
                    'coord_y': y,
                    'isPrivate': 'on'}
    else:
        datadict = {'name': name,
                    'comment': comment,
                    'coord_x': x,
                    'coord_y': y}

    data = json.loads(requests.post(f'http://{host}:{SERVICE_PORT}/Beacon/Add',
                                    data=datadict,
                                    cookies={'session': session},
                                    timeout=5).content)
    if data.get('upserted_id'):
        return data['upserted_id']


if __name__ == '__main__':
    print()
