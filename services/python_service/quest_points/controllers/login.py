from sanic.response import redirect
from sanic import Blueprint
from quest_points import jinja
from quest_points import auth
from quest_points.database.database import users
import re

id = 1

login_page = Blueprint('login_page', url_prefix='/')

@login_page.route('/Signin', methods=['GET', 'POST'])
@jinja.template('signin.html')
def login(request):
    if request.method == 'POST':
        username = request.form.get('username')
        password = request.form.get('password')
        user = users.get(username, None)
        if user and check_password(user, password):
            auth.login_user(request, user['user'])
            return redirect('/')
        else:
            return {'message': "Incorrect username or password"}
    return {'message': ''}


@login_page.route('/Logout')
@auth.login_required
def logout(request):
    auth.logout_user(request)
    return redirect('/Login')


@login_page.route('/Signup', methods=['GET', 'POST'])
@jinja.template('signup.html')
def signin(request):
    if request.method == 'POST':
        username = request.form.get('username')
        password = request.form.get('password')
        if username in users:
            return {'message': "User exists"}
        if not re.match(r'[A-Za-z0-9_]+', username):
            return {'message': "Username should contains only letters, numbers or _"}
        if not re.match(r'[A-Za-z0-9_]+', password):
            return {'message': "Password should contains only letters, numbers or _"}
        user = auth.load_user({'uid': id, 'name': username})
        users[username] = {'user': user, 'password': password}
        auth.login_user(request, user)
        return redirect('/')
    return {'message': ""}


def check_password(user, password):
    return user['password'] == password