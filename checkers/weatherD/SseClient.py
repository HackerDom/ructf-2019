#!/usr/bin/env python
"""client library for iterating over http Server Sent Event (SSE) streams"""
#
# Distributed under the terms of the MIT license.
#
from __future__ import unicode_literals

import codecs
import re
import time

import six

import requests

__version__ = '0.0.24'

end_of_field = re.compile(r'\r\n\r\n|\r\r|\n\n')


class SSEClient(object):
    def __init__(self, url, retry=3000,chunk_size=1024, **kwargs):
        self.url = url
        self.retry = retry
        self.chunk_size = chunk_size
        self.requests_kwargs = kwargs

        if 'headers' not in self.requests_kwargs:
            self.requests_kwargs['headers'] = {}
        self.requests_kwargs['headers']['Cache-Control'] = 'no-cache'

        self.requests_kwargs['headers']['Accept'] = 'text/event-stream'
        self.buf = ''

        self._connect()

    def _connect(self):
        requester = requests
        self.resp = requester.get(self.url, stream=True, **self.requests_kwargs)
        self.resp_iterator = self.iter_content()

        self.resp.raise_for_status()

    def iter_content(self):
        def generate():
            while True:
                if hasattr(self.resp.raw, '_fp') and \
                        hasattr(self.resp.raw._fp, 'fp') and \
                        hasattr(self.resp.raw._fp.fp, 'read1'):
                    chunk = self.resp.raw._fp.fp.read1(self.chunk_size)
                else:
                    chunk = self.resp.raw.read(self.chunk_size)
                if not chunk:
                    break
                yield chunk

        return generate()

    def _event_complete(self):
        return re.search(end_of_field, self.buf) is not None

    def __iter__(self):
        return self

    def __next__(self):
        decoder = codecs.getincrementaldecoder(
            self.resp.encoding)(errors='replace')
        while not self._event_complete():
            try:
                next_chunk = next(self.resp_iterator)
                if not next_chunk:
                    raise EOFError()

                self.buf += decoder.decode(next_chunk)

            except (StopIteration, requests.RequestException, EOFError, six.moves.http_client.IncompleteRead) as e:
                print(e)
                time.sleep(self.retry / 1000.0)
                self._connect()
                head, sep, tail = self.buf.rpartition('\n')
                self.buf = head + sep
                continue
        (msg, self.buf) = re.split(end_of_field, self.buf, maxsplit=1)

        return msg