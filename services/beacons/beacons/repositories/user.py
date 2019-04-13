from sanic_motor import BaseModel


def create_user(name, password, beacons=[]):
    return dict(name=name, password=password, beacons=beacons)


class User(BaseModel):
    __coll__ = "users"
    __unique_fields__ = ["id", "name"]
