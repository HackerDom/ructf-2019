from sanic.response import redirect
from sanic import Blueprint
from beacons import jinja
from beacons import auth
from beacons.repositories.user import User
import re
from random import getrandbits
from functools import reduce
from operator import concat
from uuid import UUID

login_page = Blueprint("login_page", url_prefix="/")


@login_page.route("/Signin", methods=["GET", "POST"])
@jinja.template("signin.html")
async def login(request):
    if request.method == "POST":
        username = request.form.get("username")
        password = request.form.get("password")
        if len(username) > 40:
            return {"message": "Username must be less then 40 symbols"}
        if len(password) > 40:
            return {"message": "Password must be less then 40 symbols"}
        user = await User.find_one({"name": username})
        if user and check_password(user, password):
            return auth.login_user(request, user)
        else:
            return {"message": "Incorrect username or password"}
    return {"message": ""}


@login_page.route("/Logout")
@auth.login_required
def logout(request):
    auth.logout_user(request)
    resp = redirect("/Signin")
    del resp.cookies['session']
    return resp

def get_uuid_bytes():
    return bytes(reduce(concat, (getrandbits(32).to_bytes(4, 'little') for _ in range(4)), bytearray()))

def get_invites():
    return [str(UUID(bytes=get_uuid_bytes())) for _ in range(200)]
        

@login_page.route("/Signup", methods=["GET", "POST"])
@jinja.template("signup.html")
async def signin(request):
    if request.method == "POST":
        username = request.form.get("username")
        password = request.form.get("password")
        if len(username) > 40:
            return {"message": "Username must be less then 40 symbols"}
        if len(password) > 40:
            return {"message": "Password must be less then 40 symbols"}
        if await User.find_one({"name": username}):
            return {"message": "User exists"}
        if not re.match(r"^[A-Za-z0-9_]+$", username):
            return {"message": "Username should contains only letters, numbers or _"}
        if not re.match(r"^[A-Za-z0-9_]+$", password):
            return {"message": "Password should contains only letters, numbers or _"}
        inserted_id = (await User.insert_one(
                                    {"name": username, "password": password, "beacons": [], "invites": get_invites()})).inserted_id
        user = auth.load_user({"uid": inserted_id, "name": username})
        return auth.login_user(request, user)
    return {"message": ""}


def check_password(user, password):
    return user["password"] == password
