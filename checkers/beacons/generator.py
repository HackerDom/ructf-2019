import random
import mimesis


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
    image_names = {'44.jpg': 'Apple'}
    filename = random.choice(list(image_names.keys()))
    return filename, image_names[filename]


if __name__ == '__main__':
    ee = mimesis.Person()
    print(ee.password(18, True))

