from sanic import Sanic
from sanic_jinja2 import SanicJinja2
from sanic_motor import BaseModel
from sessions import Sessions

app = Sanic(__name__)

settings = dict(MOTOR_URI='mongodb://localhost:27017/beacons', LOGO=None,)
app.config.update(settings)
BaseModel.init_app(app)

jinja = SanicJinja2(app)
app.static('/static', './beacons/static')

session = {}
@app.middleware('request')
async def add_session(request):
    request['session'] = session

app.config.AUTH_LOGIN_URL = '/Signin'
auth = Sessions(app)

from beacons.controllers.login import login_page
from beacons.controllers.map import map_page
from beacons.controllers.beacon import beacon_page
from beacons.controllers.photo_controller import photo_page

app.blueprint(login_page)
app.blueprint(map_page)
app.blueprint(beacon_page)
app.blueprint(photo_page)

