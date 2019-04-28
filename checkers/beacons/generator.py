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
    image_names = ['101.jpg',
                   '106.jpg',
                   '111.jpg',
                   '116.jpg',
                   '121.jpg',
                   '126.jpg',
                   '131.jpg',
                   '136.jpg',
                   '141.jpg',
                   '146.jpg',
                   '151.jpg',
                   '156.jpg',
                   '161.jpg',
                   '166.jpg',
                   '171.jpg',
                   '176.jpg',
                   '181.jpg',
                   '186.jpg',
                   '191.jpg',
                   '196.jpg',
                   '201.jpg',
                   '102.jpg',
                   '107.jpg',
                   '112.jpg',
                   '117.jpg',
                   '122.jpg',
                   '127.jpg',
                   '132.jpg',
                   '137.jpg',
                   '142.jpg',
                   '147.jpg',
                   '152.jpg',
                   '157.jpg',
                   '162.jpg',
                   '167.jpg',
                   '172.jpg',
                   '177.jpg',
                   '182.jpg',
                   '187.jpg',
                   '192.jpg',
                   '197.jpg',
                   '103.jpg',
                   '108.jpg',
                   '113.jpg',
                   '118.jpg',
                   '123.jpg',
                   '128.jpg',
                   '133.jpg',
                   '138.jpg',
                   '143.jpg',
                   '148.jpg',
                   '153.jpg',
                   '158.jpg',
                   '163.jpg',
                   '168.jpg',
                   '173.jpg',
                   '178.jpg',
                   '183.jpg',
                   '188.jpg',
                   '193.jpg',
                   '198.jpg',
                   '104.jpg',
                   '109.jpg',
                   '114.jpg',
                   '119.jpg',
                   '124.jpg',
                   '129.jpg',
                   '134.jpg',
                   '139.jpg',
                   '144.jpg',
                   '149.jpg',
                   '154.jpg',
                   '159.jpg',
                   '164.jpg',
                   '169.jpg',
                   '174.jpg',
                   '179.jpg',
                   '184.jpg',
                   '189.jpg',
                   '194.jpg',
                   '199.jpg',
                   '105.jpg',
                   '110.jpg',
                   '115.jpg',
                   '120.jpg',
                   '125.jpg',
                   '130.jpg',
                   '135.jpg',
                   '140.jpg',
                   '145.jpg',
                   '150.jpg',
                   '155.jpg',
                   '160.jpg',
                   '165.jpg',
                   '170.jpg',
                   '175.jpg',
                   '180.jpg',
                   '185.jpg',
                   '190.jpg',
                   '195.jpg',
                   '200.jpg']

    return random.choice(image_names)


if __name__ == '__main__':
    print(generate_picture_name())
