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
#include <dlfcn.h>
#include "httpserver.h"
#include "hash.h"
#include "interface.h"


TAddUnit* AddUnit = nullptr;
TGetUnit* GetUnit = nullptr;


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
					"{ \"uuid\": \"%s\", \"mind\": \"%s\", \"posX\": %f, \"posY\": %f, \"posZ\": %f, \"power\": %f }", uuid.c_str(),
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
		printf("Add unit request\n");

		static std::string mindStr("mind");
		static std::string uuidStr("uuid");

		const char* mind = FindInMap(request.queryString, mindStr);
		if(!mind)
		{
			printf("Missing 'mind' parameter\n");
			return HttpResponse(MHD_HTTP_BAD_REQUEST);
		}

		const char* uuid = FindInMap(request.queryString, uuidStr);
		if(!uuid)
		{
			printf("Missing 'uuid' parameter\n");
			return HttpResponse(MHD_HTTP_BAD_REQUEST);
		}

		char* buf = (char*)malloc(64);
		memset(buf, 0, 64);

		auto result = AddUnit(mind, uuid);
		if(result == kAddUnitOk)
		{
			return HttpResponse(MHD_HTTP_OK, buf, strlen(buf), Headers());
			printf("Unit added\n");
		}
		else
		{
			const char* errorStr = "Unknown error";
			uint32_t code = MHD_HTTP_INTERNAL_SERVER_ERROR;
			switch(result)
			{
				case kAddUnitTooMuchUnits:
					errorStr = "Too much units in simulation";
					break;
				case kAddUnitAlreadyExists:
					errorStr = "Unit already exists";
					break;
				case kAddUnitInternalError:
					errorStr = "Internal error";
					break;
				case kAddUnitBadUUID:
					errorStr = "Bad UUID";
					code = MHD_HTTP_BAD_REQUEST;
					break;
				default:
					break;
			}

			strcpy(buf, errorStr);
			printf("%s\n", errorStr);
			return HttpResponse(code, buf, strlen(buf), Headers());
		}
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

	void* dlh = dlopen("libinterface.so", RTLD_NOW | RTLD_LOCAL);
	if (!dlh)
	{
		fprintf(stderr, "dlopen failed: %s\n", dlerror());
		return 1;
	}

	AddUnit = (TAddUnit*)dlsym(dlh, "AddUnit");
	if(!AddUnit)
	{
		printf("Symbol load error: %s\n", dlerror());
		return 1;
	}
	GetUnit = (TGetUnit*)dlsym(dlh, "GetUnit");
	if(!GetUnit)
	{
		printf("Symbol load error: %s\n", dlerror());
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
