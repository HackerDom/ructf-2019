#include <stdio.h>
#include <string.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <unistd.h>
#include "interface.h"
#include "../commands.h"


bool AddUnit(const char* mind, float power, uint32_t& id)
{
    int sock = socket(AF_INET, SOCK_STREAM, 0);
	if (sock < 0)
	{
		perror("socket");
		return false;
	}

	sockaddr_in addr;
	memset(&addr, 0, sizeof(addr));
	addr.sin_family = AF_INET;
	addr.sin_port = htons(3333);
	inet_aton("127.0.0.1", &addr.sin_addr);
	if (connect(sock, (struct sockaddr*)&addr, sizeof(addr)) < 0)
	{
		perror("connect");
        close(sock);
		return false;
	}

	CommandHeader h;
	h.cmd = kCommandAddUnit;
	write(sock, &h, sizeof(h));

	CommandAddUnit cmd;
	memcpy(cmd.mind, mind, 32);
	cmd.power = power;
	write(sock, &cmd, sizeof(cmd));

	CommandAddUnitResponse response;
	int bytes = recv(sock, &response, sizeof(response), 0);
	if (bytes != sizeof(response))
	{
		perror("recv");
		close(sock);
		return false;
	}
	close(sock);

    id = response.id;
    return true;
}


bool GetUnit(uint32_t id, UnitDesc& desc, bool& found)
{
    int sock = socket(AF_INET, SOCK_STREAM, 0);
    if (sock < 0)
    {
        perror("socket");
        return false;
    }

    sockaddr_in addr;
    memset(&addr, 0, sizeof(addr));
    addr.sin_family = AF_INET;
    addr.sin_port = htons(3333);
    inet_aton("127.0.0.1", &addr.sin_addr);
    if (connect(sock, (struct sockaddr*)&addr, sizeof(addr)) < 0)
    {
        perror("connect");
        close(sock);
		return false;
    }

    CommandHeader h;
    h.cmd = kCommandGetUnit;
    write(sock, &h, sizeof(h));

    CommandGetUnit cmd;
    cmd.id = id;
    write(sock, &cmd, sizeof(cmd));

    CommandGetUnitResponse cmdResponse;
    int bytes = recv(sock, &cmdResponse, sizeof(cmdResponse), 0);
    if (bytes != sizeof(cmdResponse))
    {
        perror("recv");
        close(sock);
		return false;
    }
    close(sock);

    found = cmdResponse.ok;
    if(!found)
        return true;

    memcpy(desc.mind, cmdResponse.mind, sizeof(cmdResponse.mind));
    desc.posX = cmdResponse.posX;
	desc.posY = cmdResponse.posY;
	desc.posZ = cmdResponse.posZ;
	desc.power = cmdResponse.power;

    return true;
}