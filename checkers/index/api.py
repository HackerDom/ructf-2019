from requests import Session, Response

from utils.http import wait

from requests.adapters import HTTPAdapter
from requests.packages.urllib3.util.retry import Retry

PORT = ''  # TODO dont forget


class Api:
    def __init__(self, host, session: Session):
        self.session = session
        retries = Retry(total=3, backoff_factor=0.5)
        self.session.mount('http://', HTTPAdapter(max_retries=retries))
        self.host = f'{host}{PORT}'
        self._is_first = True

    def _url(self, postfix):
        return f'http://{self.host}/api{postfix}'

    def _jitter(self):
        if self._is_first:
            self._is_first = False
        else:
            wait()

    def register_user(self, user) -> Response:
        self._jitter()
        return self.session.post(self._url('/users/register'), data=user)

    def upload_zip(self, file) -> Response:
        self._jitter()
        return self.session.post(self._url('/files'), files={'file': file})

    def search_file(self, file_name) -> Response:
        self._jitter()
        return self.session.get(self._url(f'/files?fileName={file_name}'))

    def create_note(self, note_text, is_public):
        self._jitter()
        return self.session.post(self._url('/notes'), json={'Note': note_text, 'IsPublic': is_public})

    def get_notes(self, is_public):
        self._jitter()
        return self.session.get(self._url(f'/notes?isPublic={is_public}'))
