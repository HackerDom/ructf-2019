from sanic_motor import BaseModel


def create_beacon(name, coord_x, coord_y, is_public=True, comment="", photos=[]):
    return dict(name=name, coord_x=coord_x, coord_y=coord_y, is_public=is_public, comment=comment, photos=photos)


class Beacon(BaseModel):
    __coll__ = "beacons"
    __unique_fields__ = ["id"]