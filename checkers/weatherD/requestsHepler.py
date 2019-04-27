import random

with open("user-agents") as f:
    USER_AGENTS = f.read().split('\n')

@staticmethod
def get_user_agent_header():
    return {'User-Agent': random.choice(USER_AGENTS)}
