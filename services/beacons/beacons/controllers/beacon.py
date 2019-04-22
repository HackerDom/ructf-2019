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


beacon_page = Blueprint("beacon", url_prefix="/Beacon")


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
    
    inserted_id = (await Beacon.insert_one({"name": name,
                                            "comment": comment,
                                            "coord_x": coord_x,
                                            "coord_y": coord_y,
                                            "creator": user.id})
                   ).inserted_id
    print(user.beacons)
    await User.update_one({"_id": user.id}, 
                            {"$push": {"beacons": str(inserted_id)}})
    return json({"inserted_id": str(inserted_id)})


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
    photo = request.files.get("photo")
    if not re.match(r"image\/jpg|image\/jpeg|image\/tiff", photo.type):
        return json({"error": "File should be *.jpeg or *.tiff"})
    if not re.match(r"[A-Za-z0-9_!?.,]+", photo.name):
        return json({"error": "Incorrect symbol in filename"})
    if len(photo.body) > 10000000:
        return json({"error": "File should be less then 5 mg"})
    inserted_id = (await Photo.insert_one({"photo": photo.body})).inserted_id

    await Beacon.update_one({"_id": ObjectId(beacon_id)},
                            {"$push": {"photos": {"id": str(inserted_id), "name": photo.name}}})

    return json({"id": str(inserted_id), "name": photo.name})
