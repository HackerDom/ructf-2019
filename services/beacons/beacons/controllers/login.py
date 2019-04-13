from sanic.response import redirect
from sanic import Blueprint
from beacons import jinja
from beacons import auth
from beacons.repositories.user import User
import re

login_page = Blueprint('login_page', url_prefix='/')

@login_page.route('/Signin', methods=['GET', 'POST'])
@jinja.template('signin.html')
async def login(request):
    if request.method == 'POST':
        username = request.form.get('username')
        password = request.form.get('password')
        user = await User.find_one({"name": username})
        print(user)
        if user and check_password(user, password):
            auth.login_user(request, user)
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
async def signin(request):
    if request.method == 'POST':
        username = request.form.get('username')
        password = request.form.get('password')
        if await User.find_one({"name": username}):
            return {'message': "User exists"}
        if not re.match(r'[A-Za-z0-9_]+', username):
            return {'message': "Username should contains only letters, numbers or _"}
        if not re.match(r'[A-Za-z0-9_]+', password):
            return {'message': "Password should contains only letters, numbers or _"}
        inserted_id = (await User.insert_one({"name": username, "password": password, "beacons": []})).inserted_id
        user = auth.load_user({'uid': inserted_id, 'name': username})
        print(user)
        auth.login_user(request, user)
        return redirect('/')
    return {'message': ""}


def check_password(user, password):
    return user['password'] == password