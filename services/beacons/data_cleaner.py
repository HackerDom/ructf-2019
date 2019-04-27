import sys
from datetime import datetime, timedelta
from pymongo import MongoClient
import time
from bson import ObjectId


def clean_photo(photo_id, photos_collection, beacon_collection):
    beacon_collection.update({}, {"$pull": {"photos": {"id": str(photo_id)}}}, multi=True)
    photos_collection.remove({"_id": photo_id})


def clean_photos(date_to_expire, photos_collection, beacon_collection):
    print("clear photos")
    expired_photos_cursor = photos_collection.find({"createDate": {"$lt": date_to_expire}})
    c = 0
    for photo in expired_photos_cursor:
        photo_id = photo["_id"]
        clean_photo(photo_id, photos_collection, beacon_collection)
        c += 1
    print(str(c) + " photos were removed")


def clean_beacons(date_to_expire, beacon_collection, users_collection):
    print("clear beacons")
    expired_beacons_cursor = beacon_collection.find({"createDate": {"$lt": date_to_expire}})
    c = 0
    for beacon in expired_beacons_cursor:

        beacon_id = beacon["_id"]

        if len(beacon["photos"]) == 0:
            users_collection.update({}, {"$pull": {"beacons": {"id": str(beacon_id)}}}, multi=True)
            beacon_collection.remove({"_id": beacon_id})
            c += 1

    print(str(c) + " beacons were removed")


def clean(expire_after, photos_collection, beacon_collection, users_collection):
    while True:
        date_to_expire = datetime.utcnow() - timedelta(0, expire_after_interval_seconds)
        clean_photos(date_to_expire, photos_collection, beacon_collection)
        clean_beacons(date_to_expire, beacon_collection, users_collection)
        time.sleep(expire_after)


def print_help():
    print("python3 data_cleaner expire_after_interval_seconds(default=7200)")


if __name__ == "__main__":
    args_len = len(sys.argv)
    expire_after_interval_seconds = 7200
    if args_len > 1:
        arg = sys.argv[1]
        if arg == "--help" or arg == "-h":
            print_help()
            sys.exit()
        else:
            expire_after_interval_seconds = int(arg)

    client = MongoClient('mongodb://localhost:27017/beacons')
    db = client.beacons

    date_to_expire = datetime.utcnow() - timedelta(0, expire_after_interval_seconds)

    clean(expire_after_interval_seconds, db.photos, db.beacons, db.users)

