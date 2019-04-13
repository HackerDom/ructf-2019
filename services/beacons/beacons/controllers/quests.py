from sanic import Blueprint
from beacons import jinja
from beacons.repositories.database import quests_db


quests = Blueprint('quests_page', url_prefix='/')

@quests.route("/")
@jinja.template('quests.html')
def index(request):
    return {'quests': quests_db}

