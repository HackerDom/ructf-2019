import random


# created using FoodDispencer Checker from ructf-2017

def get():
    error = ""
    for i in range(2):
        try:
            return __get()
        except Exception as e:
            error = e
    raise OSError(str(error))


def __get():
    with open('useragents') as fin:
        user_agents = [line.strip() for line in fin]
    return random.choice(user_agents)
