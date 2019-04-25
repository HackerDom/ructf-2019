#!/home/cs/miniconda3/bin/python3
from __future__ import print_function
from sys import argv, stderr
import os
import requests
import UserAgents
import json
import random
import string
import io

SERVICE_NAME = "sandbox"
OK, CORRUPT, MUMBLE, DOWN, CHECKER_ERROR = 101, 102, 103, 104, 110


def close(code, public="", private=""):
	if public:
		print(public)
	if private:
		print(private, file=stderr)
	print('Exit with code %d' % code, file=stderr)
	exit(code)


def check(*args):
	addr = args[0]
	
	close( OK )


def put(*args):
	addr = args[ 0 ]
	flag_id = args[ 1 ]
	flag = args[ 2 ]

	flag_id = ''.join(random.choice(string.hexdigits) for i in range(32)).upper()

	url = 'http://%s:16780/add_unit?uuid=%s&mind=%s' % ( addr, flag_id, flag )
	headers = { 'User-Agent' : UserAgents.get() }
	try:
		r = requests.post(url, headers=headers )
		if r.status_code == 502:
			close(DOWN, "Service is down", "Nginx 502")
		if r.status_code != 200:
			close( MUMBLE, "Submit error", "Invalid status code: %s %d" % ( url, r.status_code ) )	

	except Exception as e:
		 close(DOWN, "HTTP Error", "HTTP error: %s" % e)

	close(OK, flag_id)


def get(*args):
	addr = args[0]
	flag_id = args[1]
	flag = args[2]

	url = 'http://%s:16780/get?uuid=%s' % ( addr, flag_id )
	headers = { 'User-Agent' : UserAgents.get() }
	try:
		r = requests.get(url, headers=headers )
		if r.status_code == 502:
			close(DOWN, "Service is down", "Nginx 502")
		if r.status_code != 200:
			close( MUMBLE, "Submit error", "Invalid status code: %s %d" % ( url, r.status_code ) )	

		j = json.loads(r.text)
		if 'mind' in j.keys():
			if j['mind'] != flag:
				close(CORRUPT, "Service corrupted", "Invalid flag: %s" % j['mind'] )
		else:
			close(CORRUPT, "Service corrupted", "Invalid response: %s" % r.text )
	except Exception as e:
		 close(DOWN, "HTTP Error", "HTTP error: %s" % e)

	close( OK )


def info(*args):
    close(OK, "vulns: 1")


COMMANDS = {'check': check, 'put': put, 'get': get, 'info': info}


def not_found(*args):
    print("Unsupported command %s" % argv[1], file=stderr)
    return CHECKER_ERROR


if __name__ == '__main__':
	try:
		COMMANDS.get(argv[1], not_found)(*argv[2:])
	except Exception as e:
		close(CHECKER_ERROR, "Evil checker", "INTERNAL ERROR: %s" % e)
