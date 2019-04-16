from sanic.response import json
from sanic import Blueprint
from beacons import jinja
from beacons import auth
from beacons.repositories.database import users_quests_progress
from beacons.repositories.user import User
from beacons.repositories.beacon import Beacon
import re


beacon_page = Blueprint("beacon", url_prefix='/Beacon')


@beacon_page.route("/Add", methods=["POST"])
@auth.login_required
async def add_beacon(request):
    request_user = auth.current_user(request)
    user = await User.find_one(request_user.id)
    if not re.match(r"[A-Za-z0-9_]+", request.args["name"]):
        return {"message": "Username should contains only letters, numbers or _"}
    if not re.match(r"[A-Za-z0-9_!?.,]+", request.args["comment"]):
        return {"message": "Incorrect symbol in comment"}

    inserted_id = (await Beacon.insert_one({"name": request.args["name"],
                                            "comment": request.args["comment"],
                                            "creator": user.id})
                   ).inserted_id
    return json({"inserted_id": inserted_id})


@beacon_page.route("/<beacon_id>")
# @auth.login_required
async def get_beacon(request, beacon_id):
    beacon = await Beacon.find_one(beacon_id)
    return json({'name': beacon.name})
