from sanic.response import json
from sanic.response import raw
from sanic import Blueprint
from beacons import jinja
from beacons import auth
from beacons.repositories.database import users_quests_progress
from beacons.repositories.user import User
from beacons.repositories.beacon import Beacon
from beacons.repositories.photo import Photo
import re
from bson.objectid import ObjectId
from random import choices
import string


beacon_page = Blueprint("beacon", url_prefix="/Beacon")

    
def get_random_id():
    return ''.join(choices(string.hexdigits, k=24))


@beacon_page.route("/Add", methods=["POST"])
@auth.login_required
async def add_beacon(request):
    request_user = auth.current_user(request)
    user = await User.find_one(request_user.id)

    name = request.args["name"][0]
    comment = request.args["comment"][0]
        
    if not re.match(r"[A-Za-z0-9_]+", name):
        return {"message": "Username should contains only letters, numbers or _"}
    if not re.match(r"[A-Za-z0-9_!?.,]+", request.args["comment"][0]):
        return {"message": "Incorrect symbol in comment"}
    
    coord_x = int(request.args["coord_x"][0])
    coord_y = int(request.args["coord_y"][0])
    
    if await Beacon.find_one({"coord_x": coord_x, "coord_y": coord_y}):
        return {"message": "Beacon exists"}
    
    upserted_id  = (await Beacon.update_one({"_id": ObjectId(get_random_id())}, {
                                            "$set": {"name": name,
                                                     "comment": comment,
                                                     "coord_x": coord_x,
                                                     "coord_y": coord_y,
                                                     "creator": str(user.id)},
                                            "$currentDate": {"createDate": {"$type": "timestamp"}}}, upsert=True)
                   ).upserted_id
                   
    print(user.beacons)
    await User.update_one({"_id": user.id}, 
                            {"$push": {"beacons": str(upserted_id)}})
    return json({"upserted_id": str(upserted_id)})


@beacon_page.route("/<beacon_id>")
@auth.login_required
async def get_beacon(request, beacon_id):
    beacon = await Beacon.find_one(beacon_id)
    return json({"name": beacon.name, "description": beacon.description, "creator": beacon.creator, "photos": beacon.photos})


@beacon_page.route("/GetPhoto/<photo_id>")
# @auth.login_required
async def get_photo(request, photo_id):
    photo = await Photo.find_one(photo_id)
    return raw(photo.photo)


@beacon_page.route("/AddPhoto/<beacon_id>", methods=["POST"])
# @auth.login_required
async def add_photo(request, beacon_id):
    # request_user = auth.current_user(request)
    # user = await User.find_one(request_user.id)
    # if not re.match(r"[A-Za-z0-9_]+", request.args["name"]):
    #     return {"message": "Username should contains only letters, numbers or _"}
    # if not re.match(r"[A-Za-z0-9_!?.,]+", request.args["comment"]):
    #     return {"message": "Incorrect symbol in comment"}

    photo = request.files.get("photo")
    upserted_id = (await Photo.update_one({"_id": ObjectId(get_random_id())}, {
                                                            "$set": {"photo": photo.body},
                                                            "$currentDate": {"createDate": {"$type": "timestamp"}}},
                                                            upsert=True)).upserted_id

    await Beacon.update_one({"_id": ObjectId(beacon_id)}, {"$push": {"photos": str(upserted_id)}})

    return json({"upserted_id": str(upserted_id)})
