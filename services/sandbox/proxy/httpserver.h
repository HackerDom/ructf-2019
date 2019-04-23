#pragma once

#include <microhttpd.h>
#include <string>
#include <map>

#define THREADPOOL_SIZE 16

class HttpRequestState;
class HttpRequestHandler;

using Headers = std::map<std::string, const char*>;
using QueryString = std::map<std::string, const char*>;

struct HttpRequest
{
	HttpRequest();
	HttpRequest(const char* url,
	            const char* method,
	            const Headers& headers,
	            QueryString& queryString,
	            MHD_Connection* connection);

	const char* url;
	const char* method;
	Headers headers;
	QueryString queryString;

	MHD_Connection* connection;
};

struct HttpResponse
{
	HttpResponse();
	HttpResponse(uint32_t code);
	HttpResponse(uint32_t code, char* content, size_t contentLength, const Headers& headers);

	uint32_t code;
	char* content;
	size_t contentLength;
	Headers headers;
};

class HttpServer
{
public:
	HttpServer(HttpRequestHandler* requestHandler);
	virtual ~HttpServer();

	void Start(uint32_t port);
	void Stop();

private:
	static int HandleRequest(void* param,
	                         MHD_Connection* connection,
	                         const char* url,
	                         const char* method,
	                         const char* version,
	                         const char* uploadData,
	                         size_t* uploadDataSize,
	                         void** context);
	static void PostProcessRequest(void* param,
	                               MHD_Connection* connection,
	                               void** context,
	                               MHD_RequestTerminationCode toe);
	static int IterateHeadersBase(void* cls, enum MHD_ValueKind kind, const char* key, const char* value);
	static int IterateQueryString(void* cls, enum MHD_ValueKind kind, const char* key, const char* value);
	static int SendResponse(MHD_Connection* connection, HttpResponse response);
	static void OnFatalError(void* param, const char* file, uint32_t line, const char* reason);

	bool isRunning;
	MHD_Daemon* daemon;
	HttpRequestHandler* requestHandler;
};

class HttpPostProcessor
{
public:
	virtual ~HttpPostProcessor();

	void CreateMhdProcessor();
	bool TryGetResponse(HttpResponse* response);

	virtual int IteratePostData(MHD_ValueKind kind,
	                            const char* key,
	                            const char* filename,
	                            const char* contentType,
	                            const char* transferEncoding,
	                            const char* data,
	                            uint64_t offset,
	                            size_t size) = 0;

	MHD_PostProcessor* mhdProcessor;

protected:
	HttpPostProcessor(const HttpRequest& request);

	void Complete(HttpResponse response);
	virtual void FinalizeRequest() = 0;

private:
	bool isCompleted;
	HttpRequest request;
	HttpResponse response;

	static int IteratePostDataBase(void* context,
	                               MHD_ValueKind kind,
	                               const char* key,
	                               const char* filename,
	                               const char* contentType,
	                               const char* transferEncoding,
	                               const char* data,
	                               uint64_t offset,
	                               size_t size);
};

#define OUT(x) NULL, sizeof(x), x

class HttpRequestHandler
{
public:
	virtual HttpResponse HandleGet(HttpRequest request) = 0;
	virtual HttpResponse HandlePost(HttpRequest request, HttpPostProcessor** postProcessor) = 0;

	static bool ParseUrl(const char* url, int parts, ...);
};


template <class Map>
const char* FindInMap(const Map& map, const std::string& key)
{
	auto iter = map.find(key);
	if (iter != map.end())
		return iter->second;
	return nullptr;
}


template <class Map>
bool FindInMap(const Map& map, const std::string& key, std::string& val)
{
	auto iter = map.find(key);
	if (iter != map.end())
	{
		val = iter->second;
		return true;
	}
	return false;
}


template <class Map>
bool FindInMap(const Map& map, const std::string& key, int& val)
{
	std::string valStr;
	if (!FindInMap(map, key, valStr))
		return false;
	val = atoi(valStr.c_str());
	return true;
}


template <class Map>
bool FindInMap(const Map& map, const std::string& key, uint32_t& val)
{
	std::string valStr;
	if (!FindInMap(map, key, valStr))
		return false;
	char* end;
	val = strtoul(valStr.c_str(), &end, 10);
	return true;
}


template <class Map>
bool FindInMap(const Map& map, const std::string& key, float& val)
{
	std::string valStr;
	if (!FindInMap(map, key, valStr))
		return false;
	val = atof(valStr.c_str());
	return true;
}
