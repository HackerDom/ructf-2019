from sanic_motor import BaseModel


def create_photo(beacon, photo):
    return dict(beacon=beacon, photo=photo)


class Photo(BaseModel):
    __coll__ = "photos"
    __unique_fields__ = ["id"]
