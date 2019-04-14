#include <string.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <stdio.h>
#include <string.h>
#include <unistd.h>
#include "httpserver.h"
#include "../commands.h"


class AddUnitProcessor : public HttpPostProcessor
{
public:
	AddUnitProcessor(const HttpRequest& request);
	virtual ~AddUnitProcessor();

	int IteratePostData(MHD_ValueKind kind,
	                    const char* key,
	                    const char* filename,
	                    const char* contentType,
	                    const char* transferEncoding,
	                    const char* data,
	                    uint64_t offset,
	                    size_t size);

	std::string m_mind;
	float m_power;
	bool m_isHeadersValid;

protected:
	virtual void FinalizeRequest();
};


AddUnitProcessor::AddUnitProcessor(const HttpRequest& request) : HttpPostProcessor(request)
{
	printf("Add unit request\n");

	static std::string mindStr("mind");
	static std::string powerStr("power");

	m_power = 1.0f;
	m_isHeadersValid = true;

	if (!FindInMap(request.queryString, mindStr, m_mind))
		m_isHeadersValid = false;

	if(m_mind.length() != 32)
		m_isHeadersValid = false;

	if(FindInMap(request.queryString, powerStr, m_power))
	{
		if(m_power < 0.0f || m_power > 1.0f)
			m_isHeadersValid = false;
	}
}


AddUnitProcessor::~AddUnitProcessor()
{
}


void AddUnitProcessor::FinalizeRequest()
{
	if (!m_isHeadersValid)
	{
		Complete(HttpResponse(MHD_HTTP_BAD_REQUEST));
		printf("Invalid query\n");
		return;
	}

	int sock = socket(AF_INET, SOCK_STREAM, 0);
	if(sock < 0)
	{
		perror("socket");
		Complete(HttpResponse(MHD_HTTP_INTERNAL_SERVER_ERROR));
		return;
	}

	sockaddr_in addr;
	memset(&addr, 0, sizeof(addr));
	addr.sin_family = AF_INET;
	addr.sin_port = htons(3333);
	inet_aton("127.0.0.1", &addr.sin_addr);
	if(connect(sock, (struct sockaddr *)&addr, sizeof(addr)) < 0)
	{
		perror("connect");
		Complete(HttpResponse(MHD_HTTP_INTERNAL_SERVER_ERROR));
		return;
	}

	CommandHeader h;
	h.cmd = kCommandAddUnit;
	send(sock, &h, sizeof(h), 0);

	CommandAddUnit cmd;
	memcpy(cmd.mind, m_mind.data(), 32);
	cmd.power = m_power;
	send(sock, &cmd, sizeof(cmd), 0);

	CommandAddUnitResponse response;
	int bytes = recv(sock, &response, sizeof(response), 0);
	if(bytes != sizeof(response))
	{
		perror("recv");
		close(sock);
		Complete(HttpResponse(MHD_HTTP_INTERNAL_SERVER_ERROR));
		return;
	}
	close(sock);

	char* buffer = (char*)malloc(16);
	memset(buffer, 0, 16);
	sprintf(buffer, "%u", response.id);
	Complete(HttpResponse(MHD_HTTP_OK, buffer, strlen(buffer), Headers()));
	printf("Unit added\n");
}


//
int AddUnitProcessor::IteratePostData(MHD_ValueKind kind,
                                      const char* key,
                                      const char* filename,
                                      const char* contentType,
                                      const char* transferEncoding,
                                      const char* data,
                                      uint64_t offset,
                                      size_t size)
{
	return MHD_YES;
}


class RequestHandler : public HttpRequestHandler
{
public:
	RequestHandler() = default;
	virtual ~RequestHandler() = default;

	HttpResponse HandleGet(HttpRequest request);
	HttpResponse HandlePost(HttpRequest request, HttpPostProcessor** postProcessor);

private:
};


HttpResponse RequestHandler::HandleGet(HttpRequest request)
{
}


HttpResponse RequestHandler::HandlePost(HttpRequest request, HttpPostProcessor** postProcessor)
{
	if (ParseUrl(request.url, 1, "add_unit"))
	{
		*postProcessor = new AddUnitProcessor(request);
		return HttpResponse();
	}

	return HttpResponse(MHD_HTTP_NOT_FOUND);
}


int main()
{
	RequestHandler handler;
	HttpServer server(&handler);

	server.Start(16780);

	while (1)
		sleep(1);

	server.Stop();
	return 0;
}
