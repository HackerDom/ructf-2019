#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <stdio.h>
#include <string.h>
#include <unistd.h>
#include <map>
#include "interface.h"

enum ESocketState
{
	kSocketStateWaitHeader = 0,
	kSocketStateWaitCommand,
	kSocketStateReady,

	kSocketStatesCount
};

struct Socket
{
	ESocketState state = kSocketStateWaitHeader;
	CommandHeader header = {kCommandsCount};
	char buffer[1024];
	uint32_t dataRead = 0;
};

static TInterfaceCallback GCallback = nullptr;
static int GListenSocket = 0;
static uint16_t GListenPort = 3333;
static fd_set GMasterSet;
static int GFDMax;
static std::map<int, Socket> GSockets;
static uint32_t kCommandsSize[] =
{
	sizeof(CommandAddUnit)
};

bool InitInterface(TInterfaceCallback callback)
{
	GCallback = callback;

	GListenSocket = socket(AF_INET, SOCK_STREAM, 0);
	if(GListenSocket < 0)
	{
		perror("socket:" );
		return false;
	}

	int opt_val = 1;
	setsockopt(GListenSocket, SOL_SOCKET, SO_REUSEADDR, &opt_val, sizeof(opt_val));

	sockaddr_in addr;
	memset(&addr, 0, sizeof(sockaddr_in));
	addr.sin_family = AF_INET;
	addr.sin_port = htons(GListenPort);
	inet_aton("127.0.0.1", &addr.sin_addr);
	if(bind(GListenSocket, (sockaddr*)&addr, sizeof(addr)) < 0)
	{
		perror("bind:");
		close(GListenSocket);
		return false;
	}

	listen(GListenSocket, 1);

	FD_ZERO(&GMasterSet);
	FD_SET(GListenSocket, &GMasterSet);
	GFDMax = GListenSocket;

	return true;
}


void ShutdownInterface()
{
	for(auto& sock : GSockets)
		close(sock.first);
	close(GListenSocket);
}


bool UpdateInterface()
{
	fd_set read_fds = GMasterSet;
	timeval timeout;
	timeout.tv_sec = 0;
	timeout.tv_usec = 1000;
	if(select(GFDMax + 1, &read_fds, nullptr, nullptr, &timeout) == -1)
	{
		perror("select");
		return false;
	}

	for(int i = 0; i <= GFDMax; i++)
	{
		if(FD_ISSET(i, &read_fds))
		{
			if(i == GListenSocket)
			{
				sockaddr_in clientaddr;
				socklen_t addrLen = 0;
				int s = accept(GListenSocket, (sockaddr*)&clientaddr, &addrLen);
				if(s < 0)
				{
					perror("accept");
					continue;
				}

				FD_SET(s, &GMasterSet);
				if(s > GFDMax)
					GFDMax = s;

				GSockets[s] = Socket();
			}
			else
			{
				Socket& sock = GSockets[i];
				uint32_t dataToRead = 0;
				if(sock.state == kSocketStateWaitHeader)
					dataToRead = sizeof(CommandHeader);
				else
					dataToRead = kCommandsSize[sock.header.cmd] - sock.dataRead;

				const uint32_t kBufSize = 512;
				char buf[kBufSize];
				int bytes = recv(i, buf, std::min(dataToRead, kBufSize), 0);
				if(bytes <= 0)
				{
					if(bytes < 0)
						perror("recv");

					close(i);
					FD_CLR(i, &GMasterSet);
					GSockets.erase(i);
				}
				else
				{
					if(sock.state == kSocketStateWaitHeader)
					{
						memcpy(&sock.header, buf, sizeof(CommandHeader));
						if(sock.header.cmd >= kCommandsCount)
						{
							close(i);
							FD_CLR(i, &GMasterSet);
							GSockets.erase(i);
						}
						else
						{
							sock.state = kSocketStateWaitCommand;
						}
					}
					else
					{
						memcpy(&sock.buffer[sock.dataRead], buf, bytes);
						sock.dataRead += bytes;
						if(sock.dataRead == kCommandsSize[sock.header.cmd])
						{
							sock.state = kSocketStateReady;
							if(GCallback)
							{
								char* response = nullptr;
								uint32_t responseSize = 0;
								GCallback(sock.header.cmd, sock.buffer, response, responseSize);
								if(!responseSize)
									send(i, response, responseSize, 0);
							}
						}
					}
				}
			}
		}
	}

	return true;
}
