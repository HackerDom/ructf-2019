from sanic import Sanic
from sanic_auth import Auth
from sanic_session import Session
from sanic_jinja2 import SanicJinja2
from sanic_motor import BaseModel

app = Sanic(__name__)

settings = dict(MOTOR_URI='mongodb://localhost:27017/beacons', LOGO=None,)
app.config.update(settings)
BaseModel.init_app(app)

jinja = SanicJinja2(app)
session = Session(app)

app.config.AUTH_LOGIN_URL = '/Signin'
auth = Auth(app)

from beacons.controllers.quests import quests
from beacons.controllers.quest import quest_page
from beacons.controllers.create_quest import create_quest
from beacons.controllers.login import login_page

app.blueprint(login_page)
app.blueprint(quests)
app.blueprint(quest_page)
app.blueprint(create_quest)

