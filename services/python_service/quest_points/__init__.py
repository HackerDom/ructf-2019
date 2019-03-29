from sanic import Sanic
from sanic_auth import Auth
from sanic_session import Session
from sanic_jinja2 import SanicJinja2

app = Sanic(__name__)
jinja = SanicJinja2(app)
session = Session(app)
auth = Auth(app)

from quest_points.controllers.quests import quests
from quest_points.controllers.quest import quest_page
from quest_points.controllers.create_quest import create_quest
from quest_points.controllers.login import login_page

app.blueprint(login_page)
app.blueprint(quests)
app.blueprint(quest_page)
app.blueprint(create_quest)

