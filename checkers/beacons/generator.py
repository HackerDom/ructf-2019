import random
import mimesis
import json
import re


def generate_userpass(seed):
    person = mimesis.Person(seed=seed)
    return [(f'{person.name()}_{person.surname()}', person.password(24, True)) for i in range(30)]


def generate_coords():
    return random.randint(1, 20000), random.randint(1, 20000)


def generate_beacon_name():
    place = mimesis.Address()
    return place.city()


def generate_comment():
    text = mimesis.Text()
    return text.quote()


def generate_picture_name():
    result = ' ' * 26
    while len(result) > 25:
        place = mimesis.Address().country().replace(' ', '_')
        result = re.sub(r'[^A-z0-9]', '', place)
    return result


# папка указывается уже при загрузке картинки (beacons_api)
def get_image():
    with open('pictures/dict.json', 'rb') as images:
        image_names = json.load(images)
        filename = random.choice(list(image_names.keys()))
    return filename, image_names[filename]


if __name__ == '__main__':
    print(generate_picture_name())
    print(generate_picture_name())
    print(generate_picture_name())
    print(generate_picture_name())
    print(generate_picture_name())
    print(generate_picture_name())
    print(generate_picture_name())
