#!/usr/bin/python
from bottle import get, post, request, run, abort
import os
import re
import json
import socket
import struct


@get('/get')
def GetUnit():
	print("Get unit request")

	uuid = request.query.uuid
	if not uuid:
		abort(400, "Invalid UUID")

	if not re.match("[0-9a-fA-F]{32}", uuid):
		abort(400, "Invalid UUID")

	try:
		sock = socket.socket()
		sock.connect(("127.0.0.1", 3333))

		bytes = ''.join([chr(int(uuid[i * 2 : i * 2 + 2], 16)) for i in range(16)])
		header = '\x01\x00\x00\x00' + bytes
		sock.send(header)

		bytes = '\x00' * 16
		sock.send(bytes)

		response = sock.recv(52)
		found, mind, pos_x, pos_y, pos_z, power = struct.unpack('<I32sffff', response)

		if found:
			return '{{ "uuid": "{}", "mind": "{}", "posX": {}, "posY": {}, "posZ": {}, "power": {} }}'.format(
					uuid, mind, pos_x, pos_y, pos_z, power
				)
		else:
			abort(404)

	except:
		abort(500)		


@post('/add_unit')
def AddUnit():
	print("Add unit request")
	
	uuid = request.query.uuid
	if not uuid:
		abort(400, "Invalid UUID")

	if not re.match("^[0-9a-fA-F]{32}$", uuid):
		abort(400, "Invalid UUID")

	mind = request.query.mind
	if not mind or len(mind) != 32:
		abort(400, "Invalid mind")

	try:
		sock = socket.socket()
		sock.connect(("127.0.0.1", 3333))

		bytes = ''.join([chr(int(uuid[i * 2 : i * 2 + 2], 16)) for i in range(16)])
		header = '\x00\x00\x00\x00' + bytes
		sock.send(header)
		sock.send(mind)

		response = sock.recv(4)
		code = struct.unpack('<I', response)[0]

		if code == 0:
			return "Success!"
		
		reasons = [ "Too many units", "Unit already exists" ]
		abort(500, reasons[code - 1])

	except:
		abort(500)

run(server='tornado', host='0.0.0.0', port=16781, reloader=True)