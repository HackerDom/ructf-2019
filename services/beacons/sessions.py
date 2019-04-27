from collections import namedtuple
from functools import wraps
from sanic import response
from inspect import isawaitable
from random import choices
import string
import hashlib


User = namedtuple('User', 'id name'.split())

class Sessions:
    
    def __init__(self, app):
        self.app = app
        self.login_url = app.config.get('AUTH_LOGIN_URL', None)
        self.cookie_name = 'session'
        self._sessions = {}
        self.cookie_generator = self._cookie_gen()

    def _cookie_gen(self, cookie=''):
        while True:
            cookie = hashlib.md5(cookie.encode()).hexdigest()
            yield cookie
            
    def login_user(self, request, user):
        cookie = next(self.cookie_generator)
        self._sessions[cookie] = {'uid': str(user.id), 'name': user.name}
        resp = response.redirect('/')
        resp.cookies[self.cookie_name] = cookie
        return resp
        
    def logout_user(self, request):
        cookie = request.cookies.get(self.cookie_name)
        if cookie:
            self._sessions.pop(cookie, None)
        
    def login_required(self, route):
        @wraps(route)
        async def privileged(request, *args, **kwargs):
            user = self.current_user(request)
            if user:
                resp = route(request, *args, **kwargs)
            else:
                resp = response.redirect(self.login_url)
                
            if isawaitable(resp):
                resp = await resp
            return resp
        return privileged
            
    def current_user(self, request):
        cookie = request.cookies.get(self.cookie_name)
        user = self._sessions.get(cookie)
        return self.load_user(user)
        
    def load_user(self, token):
        if token is not None:
            return User(id=token['uid'], name=token['name'])
    