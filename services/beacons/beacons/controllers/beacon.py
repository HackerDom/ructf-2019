from sanic.response import json
from sanic import Blueprint
from beacons import auth
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

    name = request.form.get("name")
    comment = request.form.get("comment")

    if len(name) > 25:
        return json({"message": "Name must be less then 25 symbols"})
    if len(comment) > 255:
        return json({"message": "Comment must be less then 255 symbols"})

    if not re.match(r"^[\w_ ]+$", name):
        return json({"error": "Username should contains only letters, numbers or _"})
    if comment and not re.match(r"^[\w_!?., ='\r\n]+$", comment):
        return json({"error": "Incorrect symbol in comment"})

    coord_x = int(request.form.get("coord_x"))
    coord_y = int(request.form.get("coord_y"))
    is_private = True if request.form.get("isPrivate") == 'on' else False
    
    if len(user.invites) < 1 and is_private:
        return json({"error": "Only 200 beacon permit"})
    
    if await Beacon.find_one({"coord_x": coord_x, "coord_y": coord_y}):
        return json({"error": "Beacon exists"})
        
    upserted_id = (await Beacon.update_one({"_id": ObjectId(get_random_id())}, {
                                            "$set": {"name": name,
                                                     "comment": comment,
                                                     "coord_x": coord_x,
                                                     "coord_y": coord_y,
                                                     "creator": str(user.id),
                                                     "photos": [],
                                                     "is_private": is_private,
                                                     "invite": (await get_invite_by_user(user)) if is_private else ''},
                                                     "$currentDate": {"createDate": {"$type": "date"}}}, upsert=True)
                   ).upserted_id

    await User.update_one({"_id": user.id}, {"$push": {"beacons": str(upserted_id)}})
    return json({"upserted_id": str(upserted_id)})


async def get_invite_by_user(user):
    await User.update_one({"_id": user.id}, {"$pop": {"invites": -1}})
    return user.invites.pop(0)


@beacon_page.route("/<beacon_id>")
@auth.login_required
async def get_beacon(request, beacon_id):
    if not re.match(r"^[\da-fA-F]{24}$", beacon_id):
        return json({"error": "Incorrect beacon id"})
    
    user = await User.find_one(auth.current_user(request).id)
    beacon = await Beacon.find_one(beacon_id)

    creator = await User.find_one(beacon.creator)
    
    if beacon.is_private and beacon_id not in user.beacons:
        return json({"name": "Hidden", "comment": "Hidden", "is_private": True,
                     "x": beacon.coord_x, "y": beacon.coord_y,
                     "creator": creator.name, "photos": []})
    
    return json({"name": beacon.name, "comment": beacon.comment, "is_private": False,
                 "x": beacon.coord_x, "y": beacon.coord_y,
                 "creator": creator.name, "photos": beacon.photos, "invite": beacon.invite})


@beacon_page.route("/Invite", methods=["POST"])
@auth.login_required
async def get_beacon_by_invite(request):
    invite = request.form.get("invite")

    if not re.match(r"^[\da-fA-F-]{36}$", invite):
        return json({"error": "Incorrect invite"})

    beacon = await Beacon.find_one({'invite': invite})
    if not beacon:
        return json({"error": "Incorrect invite"})
        
    user = await User.find_one(auth.current_user(request).id)
    if str(beacon.id) in user.beacons:
        return json({"error": "Invite already released"})
    
    await User.update_one({"_id": user.id}, {"$push": {"beacons": str(beacon.id)}})

    return json({"id": str(beacon.id), "x": beacon.coord_x, "y": beacon.coord_y})


@beacon_page.route("/AddPhoto/<beacon_id>", methods=["POST"])
@auth.login_required
async def add_photo(request, beacon_id):
    if not re.match(r"^[\da-fA-F]{24}$", beacon_id):
        return json({"error": "Incorrect beacon id"})

    user = await User.find_one(auth.current_user(request).id)
    beacon = await Beacon.find_one(beacon_id)

    if beacon.is_private and beacon_id not in user.beacons:
        return json({"error": "You have no rights on adding photo"})

    photo = request.files.get("photo")

    if not re.match(r"^image\/jpg|image\/jpeg|image\/tiff$", photo.type):
        return json({"error": "File should be *.jpeg or *.tiff"})
    if not re.match(r"^[\w_!?.,]+$", photo.name):
        return json({"error": "Incorrect symbol in filename"})
    if len(photo.body) > 10000000:
        return json({"error": "File should be less then 5 mg"})

    upserted_id = (await Photo.update_one({"_id": ObjectId(get_random_id())}, {
                                                            "$set": {"photo": photo.body, "beaconId": beacon_id, "is_private": beacon.is_private},
                                                            "$currentDate": {"createDate": {"$type": "date"}}},
                                                            upsert=True)).upserted_id

    await Beacon.update_one({"_id": ObjectId(beacon_id)},
                            {"$push": {"photos": {"id": str(upserted_id), "name": photo.name}}})

    return json({"id": str(upserted_id), "name": photo.name})


@beacon_page.route("/GetUserBeacons")
@auth.login_required
async def get_user_beacons(request):
    user = await User.find_one(auth.current_user(request).id)

    user_beacons_ids = [ObjectId(beacon) for beacon in user.beacons]
    beacons = await Beacon.find(filter={"_id": {"$in": user_beacons_ids}},
                                sort=[("createDate", -1)],
                                limit=50)

    return json({"beacons": [{"id": str(beacon.id), "name": beacon.name,
                              "x": beacon.coord_x, "y": beacon.coord_y}
                             for beacon in beacons.objects]})
