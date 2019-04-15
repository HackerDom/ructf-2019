from sanic import Blueprint
from sanic.response import json
from beacons import jinja
from beacons import auth
from beacons.repositories.beacon import Beacon


map_page = Blueprint("map_page", url_prefix="/")
map_height = 400
map_weight = 1000


@map_page.route("/")
@auth.login_required
@jinja.template("quests.html")
def index(request):
    return {"quests": ["name"]}


def parse_float(s):
    try:
        d = float(s)
        if d:
            return d
        return 0
    except:
        return 0


def get_borders(center_coord_x, center_coord_y):
    half_map_height = map_height / 2
    half_map_weight = map_weight / 2
    return (center_coord_y + half_map_height, center_coord_x + half_map_weight,
            center_coord_y - half_map_height, center_coord_x - half_map_weight)


async def get_beacons_in_area(top, right, bottom, left, request):
    beacons = await Beacon.find(request, {"coord_x": {"$lt": right, "$gt": left},
                                          "coord_y": {"$lt": top, "$gt": bottom}})

    return beacons.objects


@map_page.route("/GetBeacons")
@auth.login_required
async def get_beacons(request):
    center_coord_x = parse_float(request.args["center_coord_x"])
    center_coord_y = parse_float(request.args["center_coord_y"])

    beacons = await get_beacons_in_area(*get_borders(center_coord_x, center_coord_y), request)

    return json({"beacons": [beacon.name for beacon in beacons]})
