from sanic_motor import BaseModel


class Photo(BaseModel):
    __coll__ = "photos"
    __unique_fields__ = ["id"]
