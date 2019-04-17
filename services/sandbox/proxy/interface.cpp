#include <stdio.h>
#include <string.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <unistd.h>
#include "interface.h"


struct AddUnitStackFrame
{
    sockaddr_in addr;
    CommandHeader h;
    CommandAddUnit cmd;
    CommandAddUnitResponse response;
    int sock;
    size_t size;
};


EAddUnitResult AddUnit(const char* mind, const char* uuid)
{
    AddUnitStackFrame s;
    memset(&s.addr, 0, sizeof(s.addr));
    s.addr.sin_family = AF_INET;
    s.addr.sin_port = htons(3333);
    inet_aton("127.0.0.1", &s.addr.sin_addr);

    s.sock = socket(AF_INET, SOCK_STREAM, 0);
	if (s.sock < 0)
	{
		perror("socket");
		return kAddUnitInternalError;
	}

	if (connect(s.sock, (struct sockaddr*)&s.addr, sizeof(s.addr)) < 0)
	{
		perror("connect");
        close(s.sock);
		return kAddUnitInternalError;
	}

    s.size = sizeof(CommandHeader);
	s.h.cmd = kCommandAddUnit;
    uuid_parse(uuid, s.h.uuid);
	write(s.sock, &s.h, s.size);

    s.size = sizeof(CommandAddUnit);
	memcpy(s.cmd.mind, mind, 32);
	write(s.sock, &s.cmd, s.size);

	int bytes = recv(s.sock, &s.response, sizeof(s.response), 0);
	if (bytes != sizeof(s.response))
	{
		perror("recv");
		return kAddUnitInternalError;
	}
	close(s.sock);

    return s.response.result;
}


struct GetUnitStackFrame
{
    sockaddr_in addr;
    CommandHeader h;
    CommandGetUnit cmd;
    CommandGetUnitResponse response;
    int sock;
    size_t size;
};


bool GetUnit(const char* uuid, UnitDesc& desc, bool& found)
{
    GetUnitStackFrame s;
    memset(&s.addr, 0, sizeof(s.addr));
    s.addr.sin_family = AF_INET;
    s.addr.sin_port = htons(3333);
    inet_aton("127.0.0.1", &s.addr.sin_addr);

    s.sock = socket(AF_INET, SOCK_STREAM, 0);
    if (s.sock < 0)
    {
        perror("socket");
        return false;
    }

    if (connect(s.sock, (struct sockaddr*)&s.addr, sizeof(s.addr)) < 0)
    {
        perror("connect");
        close(s.sock);
		return false;
    }

    s.size = sizeof(CommandHeader);
    s.h.cmd = kCommandGetUnit;
    uuid_parse(uuid, s.h.uuid);
    write(s.sock, &s.h, s.size);

    s.size = sizeof(CommandGetUnit);
    write(s.sock, &s.cmd, s.size);

    int bytes = recv(s.sock, &s.response, sizeof(s.response), 0);
    if (bytes != sizeof(s.response))
    {
        perror("recv");
        close(s.sock);
		return false;
    }
    close(s.sock);

    found = s.response.ok;
    if(!found)
        return false;

    memcpy(desc.mind, s.response.mind, sizeof(s.response.mind));
    desc.posX = s.response.posX;
	desc.posY = s.response.posY;
	desc.posZ = s.response.posZ;
	desc.power = s.response.power;

    return true;
}
