from sanic_auth import User

users = {'a': {'user': User(id=1, name='a'), 'password': 'a'}}

class Quest():
    def __init__(self, name, quest_id, points):
        self.name = name
        self.quest_id = quest_id
        self.points = points.copy()

class QuestPoint():
    def __init__(self, point_id, picture_path, latitude, longitude): # в будущем будем из картинки брать
        self.point_id = point_id
        self.picture_path = picture_path
        self.latitude = latitude
        self.longitude = longitude

class UserQuestProgress():
    def __init__(self, user_id, quest_id, quest_point_id):
        self.user_id = user_id
        self.quest_id = quest_id
        self.quest_point_id = quest_point_id

quests_db = {1: Quest("qqq", 1, [QuestPoint(5, "c:", 4, 5), QuestPoint(6, "c:", 7, 8)]),
             2: Quest("www", 2, [QuestPoint(1, "c:", 1, 1)])}

users_quests_progress = {1: {1: 6, 2: 1}}