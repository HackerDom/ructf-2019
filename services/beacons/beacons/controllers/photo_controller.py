from sanic.response import json
from sanic.response import raw
from sanic import Blueprint
from beacons import auth
from beacons.repositories.photo import Photo
from random import shuffle
import re

photo_page = Blueprint("latest", url_prefix="/Photo")
latest_range = 100
latest_count = 10


@photo_page.route("/GetLatest")
@auth.login_required
async def get_latest(request):
    photos = await Photo.find(filter={"is_private": False}, sort=[("createDate", -1)], limit=latest_range)
    latest_range_photos = [{"id": str(photo.id), "beaconId": photo.beaconId} for photo in photos.objects]

    shuffle(latest_range_photos)

    latest_photos = latest_range_photos[:latest_count]
    return json({"latest_photos": latest_photos})


@photo_page.route("/GetPhoto/<photo_id>")
@auth.login_required
async def get_photo(request, photo_id):
    if not re.match(r"^[\da-fA-F]{24}$", photo_id):
        return json({"error": "Incorrect photo id"})
    photo = await Photo.find_one(photo_id)
    return raw(photo.photo)
