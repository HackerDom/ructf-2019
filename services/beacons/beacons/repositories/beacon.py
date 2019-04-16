from sanic_motor import BaseModel


class Beacon(BaseModel):
    __coll__ = "beacons"
    __unique_fields__ = ["id"]
