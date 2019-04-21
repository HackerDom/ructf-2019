import random
import time

import requests
from requests import Session

from utils.randomizer import Randomizer


def build_session() -> Session:
    s = requests.Session()
    s.headers = {'User-Agent': Randomizer.user_agent()}
    return s

def wait():
    time.sleep(random.randrange(1000, 5000) / 1000)
