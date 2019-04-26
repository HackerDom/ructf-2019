from sanic import Blueprint
from sanic.response import json
from beacons import jinja
from beacons import auth
from beacons.repositories.beacon import Beacon


map_page = Blueprint("map_page", url_prefix="/")
map_height = 30
map_weight = 50


def parse_float(s):
    try:
        d = float(s)
        if d:
            return d
        return 0
    except:
        return 0


def parse_int(s):
    try:
        d = int(s)
        if d:
            return d
        return 0
    except Exception as e:
        print(e)
        return 0


@map_page.route("/")
@auth.login_required
@jinja.template("map.html")
def index(request):
    center_x = 0
    center_y = 0
    if "center_x" in request.args:
        center_x = parse_int(request.args["center_x"][0])
    if "center_y" in request.args:
        center_y = parse_int(request.args["center_y"][0])

    request_user = auth.current_user(request)

    return {"center_x": center_x, "center_y": center_y, "user": {"name": request_user.name}}


def get_borders(center_coord_x, center_coord_y):
    half_map_height = map_height / 2
    half_map_weight = map_weight / 2
    return (center_coord_y + half_map_height, center_coord_x + half_map_weight,
            center_coord_y - half_map_height, center_coord_x - half_map_weight)


async def get_beacons_in_area(top, right, bottom, left):
    beacons = await Beacon.find(filter={"coord_x": {"$lt": right, "$gt": left},
                                        "coord_y": {"$lt": top, "$gt": bottom}})

    return beacons.objects


@map_page.route("/GetBeacons")
@auth.login_required
async def get_beacons(request):
    center_coord_x = parse_int(request.args["center_coord_x"][0])
    center_coord_y = parse_int(request.args["center_coord_y"][0])

    beacons = await get_beacons_in_area(*get_borders(center_coord_x, center_coord_y))

    return json({"beacons": [{"id": str(beacon.id), "coord_x": beacon.coord_x, "coord_y": beacon.coord_y} for beacon in beacons]})
