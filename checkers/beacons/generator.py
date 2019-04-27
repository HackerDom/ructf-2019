import random
import mimesis
import json


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


# папка указывается уже при загрузке картинки (beacons_api)
def get_image():
    with open('pictures/dict.json', 'rb') as images:
        image_names = json.load(images)
        filename = random.choice(list(image_names.keys()))
    return filename, image_names[filename]


if __name__ == '__main__':
    print()

