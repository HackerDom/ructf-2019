from sanic.response import json
from sanic import Blueprint
from beacons import jinja

create_quest = Blueprint('create_quest_page', url_prefix='/CreateQuest')

@create_quest.route("/")
@jinja.template('create_quest.html')
def create(request):
    return {'name': 'Create quest'}

@create_quest.route("/Add")
def add_photo(request):
    return json({'succsess': True})

@create_quest.route("/Save")
def save_draft(request):
    return json({'succsess': True})

@create_quest.route("/Pub")
def publicate_quest(request):
    return json({'succsess': True})

# Тут 3 способа, как можно доставать GPS из картинки.
# Первый самый полный, но неудобный.
# Последний самый удобный, но умеет только с GPS

# 1)
# from PIL import Image
# from PIL.ExifTags import TAGS
# img = Image.open(r'C:\Users\Lenovo\Pictures\cat.jpg')
# exif_data = img._getexif()
#
# print(exif_data)


# 2)
# import exifread
# # Open image file for reading (binary mode)
# f = open(r'C:\Users\Lenovo\Pictures\cat.jpg', 'rb')
#
# # Return Exif tags
# tags = exifread.process_file(f)
# latitude = tags.get('GPS GPSLatitude')
#
# print(tags)
# print(latitude)


# 3)
# from GPSPhoto import gpsphoto
# # Get the data from image file and return a dictionary
# data = gpsphoto.getGPSData(r'C:\Users\Lenovo\Pictures\cat.jpg')
# print(data['Latitude'], data['Longitude'])