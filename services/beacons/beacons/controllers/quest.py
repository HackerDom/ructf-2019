from sanic.response import json
from sanic import Blueprint
from beacons import jinja
from beacons import auth
from beacons.repositories.database import users_quests_progress

quest_page = Blueprint("quest", url_prefix='/Quest')

@quest_page.route("/<quest_id>")
@jinja.template('quest.html')
@auth.login_required
def create_quest(request, quest_id):
    user = auth.current_user(request)
    point = users_quests_progress[int(user.id)][int(quest_id)]
    return {'quest_point': point, 'quest_id': quest_id}

@quest_page.route("/CheckIn/<quest_id>", methods=['POST'])
@auth.login_required
def check_in(request):
    return json({'name': 'Quest'})
