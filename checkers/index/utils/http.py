import random
import time

import requests
from requests import Session

from utils.randomizer import Randomizer


def build_session() -> Session:
    s = requests.Session()
    s.headers = {'User-Agent': Randomizer.user_agent()}
    return s

def wait(max_time = 3000):
    time.sleep(random.randrange(1000, max_time) / 1000)
