from sanic_motor import BaseModel


def create_beacon(name, is_public=True, comment="", photos=[]):
    return dict(name=name, is_public=is_public, comment=comment, photos=photos)


class Beacon(BaseModel):
    __coll__ = "beacons"
    __unique_fields__ = ["id"]