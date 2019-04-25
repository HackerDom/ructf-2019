#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <stdio.h>
#include <string.h>
#include <unistd.h>
#include <map>
#include <vector>
#include <thread>
#include <time.h>
#include <poll.h>
#include "interface.h"
#include "thread_affinity.h"


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
	time_t lastTouchTime = 0;
};


static TInterfaceCallback GCallback = nullptr;
static std::thread GThread;
static bool GStopThread = false;
static int GListenSocket = 0;
static uint16_t GListenPort = 3333;
static uint32_t kCommandsSize[] =
{
	sizeof(CommandAddUnit),
	sizeof(CommandGetUnit)
};
static const uint32_t GTimeout = 10; // secs


void NetworkThread();


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

	GThread = std::thread(NetworkThread);

	return true;
}


void ShutdownInterface()
{
	GStopThread = true;
	GThread.join();

	close(GListenSocket);
}


void NetworkThread()
{
	if(PinThreadToCore(1) != 0)
	{
		perror("PinThreadToCore");
		return;
	}

	std::map<int, Socket> socketsMap;
	std::vector<pollfd> pollFds;
	std::vector<int> socketsToAdd;
	std::vector<size_t> idxToRemove;

	pollfd pollFd;
	pollFd.fd = GListenSocket;
	pollFd.events = POLLIN;
	pollFd.revents = 0;
	pollFds.push_back(pollFd);

	while(!GStopThread)
	{
		int ret = poll(pollFds.data(), pollFds.size(), 1);
		if(ret < 0)
		{
			perror("poll");
			exit(1);
		}
		if(ret == 0)
			continue;

		for(size_t i = 0; i < pollFds.size(); i++)
		{
			auto& pollFd = pollFds[i];
			if(pollFd.revents == 0)
			{
				if(pollFd.fd != GListenSocket)
				{
					bool timeout = time(nullptr) - socketsMap[pollFd.fd].lastTouchTime > GTimeout;
					if(timeout)
					{
						printf("Sock %d timeout\n", pollFd.fd);
						idxToRemove.push_back(i);
					}
				}
				continue;
			}
			if(pollFd.revents != POLLIN)
			{
				printf("Invalid revents\n");
				exit(1);
			}

			if(pollFd.fd == GListenSocket)
			{
				sockaddr_in clientaddr;
				socklen_t addrLen = 0;
				int s = accept(GListenSocket, (sockaddr*)&clientaddr, &addrLen);
				if(s < 0)
				{
					perror("accept");
					continue;
				}

				socketsToAdd.push_back(s);
			}
			else
			{
				Socket& sock = socketsMap[pollFd.fd];
				sock.lastTouchTime = time(nullptr);
				uint32_t dataToRead = 0;
				if(sock.state == kSocketStateWaitHeader)
					dataToRead = sizeof(CommandHeader) - sock.dataRead;
				else
					dataToRead = kCommandsSize[sock.header.cmd] - sock.dataRead;

				const uint32_t kBufSize = 512;
				char buf[kBufSize];
				int bytes = recv(pollFd.fd, buf, std::min(dataToRead, kBufSize), 0);
				if(bytes <= 0)
				{
					if(bytes < 0)
						perror("recv");

					idxToRemove.push_back(i);
				}
				else
				{
					memcpy(&sock.buffer[sock.dataRead], buf, bytes);
					sock.dataRead += bytes;

					if(sock.state == kSocketStateWaitHeader)
					{
						if(sock.dataRead == sizeof(CommandHeader))
						{
							memcpy(&sock.header, buf, sizeof(CommandHeader));
							if(sock.header.cmd >= kCommandsCount)
							{
								idxToRemove.push_back(i);
							}
							else
							{
								sock.dataRead = 0;
								sock.state = kSocketStateWaitCommand;
							}
						}
					}
					else
					{
						if(sock.dataRead == kCommandsSize[sock.header.cmd])
						{
							sock.state = kSocketStateReady;
							if(GCallback)
							{
								char* response = nullptr;
								uint32_t responseSize = 0;
								GCallback(sock.header, sock.buffer, response, responseSize);
								if(responseSize)
									send(pollFd.fd, response, responseSize, 0);
							}
						}
					}
				}
			}
		}

		for(size_t idx : idxToRemove)
		{
			if(idx == 0)
			{
				printf("Trying to remove listening socket\n");
				exit(1);
			}

			int sock = pollFds[idx].fd;
			socketsMap.erase(sock);
			if(pollFds.size() != 2 && idx != pollFds.size() - 1)
				pollFds[idx] = pollFds.back();
			pollFds.pop_back();
			close(sock);
		}
		idxToRemove.clear();

		for(int sock : socketsToAdd)
		{
			socketsMap[sock] = Socket();
			socketsMap[sock].lastTouchTime = time(nullptr);
			pollfd pollFd;
			pollFd.fd = sock;
			pollFd.events = POLLIN;
			pollFd.revents = 0;
			pollFds.push_back(pollFd);
		}
		socketsToAdd.clear();
	}

	for(auto& pollFd : pollFds)
		close(pollFd.fd);

	printf("Network thread stopped\n");
}
