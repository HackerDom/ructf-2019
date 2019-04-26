import random
from io import BytesIO
from typing import Tuple
from zipfile import ZipFile

from utils.randomizer import Randomizer


def create_zip() -> Tuple[str, str, bytes]:
    b = BytesIO()
    filename = f'{Randomizer.word()}.zip'
    file_in_zip = Randomizer.word()
    with ZipFile(b, 'w') as z:
        z.filename = filename
        z.writestr(file_in_zip, Randomizer.word())

    return file_in_zip, filename, b.getvalue()


def create_flag_zip(flag) -> Tuple[str, bytes]:
    b = BytesIO()
    is_flag_in = False
    filename = f'{Randomizer.word(20)}.zip'
    with ZipFile(b, 'w') as z:
        n = random.randint(2, 4)
        for i in range(n):
            for ii in range(random.randint(1, 3)):
                if not is_flag_in:
                    z.writestr(flag, Randomizer.word())
                    is_flag_in = True
                else:
                    z.writestr(Randomizer.word(), Randomizer.word())

    return filename, b.getvalue()