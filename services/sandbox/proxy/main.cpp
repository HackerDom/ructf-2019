#include <string.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <fcntl.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <stdio.h>
#include <string.h>
#include <unistd.h>
#include "httpserver.h"
#include "hash.h"
#include "interface.h"


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

	if (m_mind.length() != 32)
		m_isHeadersValid = false;

	if (FindInMap(request.queryString, powerStr, m_power))
	{
		if (m_power < 0.0f || m_power > 1.0f)
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

	char* uuid = (char*)malloc(64);
	memset(uuid, 0, 64);
	if(!AddUnit(m_mind.data(), m_power, uuid))
	{
		Complete(HttpResponse(MHD_HTTP_INTERNAL_SERVER_ERROR));
		return;
	}

	Complete(HttpResponse(MHD_HTTP_OK, uuid, strlen(uuid), Headers()));
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
	if (ParseUrl(request.url, 1, "get"))
	{
		printf("Get unit request\n");

		static std::string uuidStr("uuid");
		std::string uuid;
		if(!FindInMap(request.queryString, uuidStr, uuid))
		{
			printf("Missing UUID in get request\n");
			return HttpResponse(MHD_HTTP_BAD_REQUEST);
		}

		bool found = false;
		UnitDesc desc;
		if(!GetUnit(uuid.c_str(), desc, found))
			return HttpResponse(MHD_HTTP_INTERNAL_SERVER_ERROR);

		if (found)
		{
			HttpResponse response;
			response.code = MHD_HTTP_OK;
			response.headers.insert({"Content-Type", "application/json"});
			char mindStr[256];
			memset(mindStr, 0, 256);
			memcpy(mindStr, desc.mind, 32);
			response.content = (char*)malloc(512);
			memset(response.content, 0, 512);
			sprintf(response.content,
					"{ \"uuid\": %s, \"mind\": \"%s\", \"posX\": %f, \"posY\": %f, \"posZ\": %f, \"power\": %f }", uuid.c_str(),
			        mindStr, desc.posX, desc.posY, desc.posZ, desc.power);
			response.contentLength = strlen(response.content);

			return response;
		}
		else
		{
			return HttpResponse(MHD_HTTP_NOT_FOUND);
		}
	}

	return HttpResponse(MHD_HTTP_NOT_FOUND);
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


bool CheckInterfaceVersion()
{
	int f = open("libinterface.so", O_RDWR);
	if (f < 0)
	{
		perror("open");
		return false;
	}

	int size = lseek(f, 0, SEEK_END);
	lseek(f, 0, SEEK_SET);
	
	uint32_t hash = 0;
	for(int i = 0; i < size; i += 4)
	{
		uint32_t bytes;
		if(pread(f, &bytes, 4, i) != 4)
			return false;
		
		hash ^= bytes;
	}

	return hash == kLibHash;
}


int main()
{
	if(!CheckInterfaceVersion())
	{
		printf("Something wrong\n");
		return 1;
	}

	RequestHandler handler;
	HttpServer server(&handler);

	server.Start(16780);

	while (1)
		sleep(1);

	server.Stop();
	return 0;
}
