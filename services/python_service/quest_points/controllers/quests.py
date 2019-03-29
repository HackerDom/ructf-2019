from sanic import Blueprint
from quest_points import jinja
from quest_points.database.database import quests_db


quests = Blueprint('quests_page', url_prefix='/')

@quests.route("/")
@jinja.template('quests.html')
def index(request):
    return {'quests': quests_db}

