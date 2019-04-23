#include "httpserver.h"

#include <string.h>
#include <stdio.h>
#include <stdlib.h>
#include <sys/types.h>
#include <sys/select.h>
#include <sys/socket.h>
#include <microhttpd.h>
#include <pthread.h>
#include <algorithm>

#define POSTBUFFERSIZE 65536

HttpServer::HttpServer(HttpRequestHandler* requestHandler)
{
	this->requestHandler = requestHandler;
	isRunning = false;
}

HttpServer::~HttpServer()
{
	Stop();
}

void HttpServer::Start(uint32_t port)
{
	if (isRunning)
		return;

	MHD_set_panic_func(OnFatalError, NULL);

	daemon = MHD_start_daemon(MHD_USE_SELECT_INTERNALLY, port, NULL, NULL, HandleRequest, this,
	                          MHD_OPTION_THREAD_POOL_SIZE, THREADPOOL_SIZE, MHD_OPTION_CONNECTION_TIMEOUT, 10u,
	                          MHD_OPTION_NOTIFY_COMPLETED, PostProcessRequest, NULL, MHD_OPTION_END);

	if (!daemon)
	{
		printf("Failed to start MHD_Daemon!\n");
		exit(1);
	}

	printf("Listening on port %d...\n", port);

	isRunning = true;
}

void HttpServer::Stop()
{
	if (!isRunning)
		return;

	MHD_stop_daemon(daemon);

	isRunning = false;
}

int HttpServer::HandleRequest(void* param,
                              MHD_Connection* connection,
                              const char* url,
                              const char* method,
                              const char* version,
                              const char* uploadData,
                              size_t* uploadDataSize,
                              void** context)
{
	HttpServer* self = (HttpServer*)param;

	printf("Received request: %s %s\n", method, url);

	Headers headers;
	QueryString queryString;
	if (!*context)
	{
		if (!strcmp(method, "POST"))
		{
			HttpPostProcessor* postProcessor = NULL;

			MHD_get_connection_values(connection, MHD_HEADER_KIND, IterateHeadersBase, &headers);
			MHD_get_connection_values(connection, MHD_GET_ARGUMENT_KIND, IterateQueryString, &queryString);

			HttpResponse response = self->requestHandler->HandlePost(
			    HttpRequest(url, method, headers, queryString, connection), &postProcessor);

			if (!postProcessor)
			{
				SendResponse(connection, response);
				return MHD_YES;
			}

			postProcessor->CreateMhdProcessor();

			*context = postProcessor;

			return MHD_YES;
		}
	}

	if (!strcmp(method, "GET"))
	{
		MHD_get_connection_values(connection, MHD_GET_ARGUMENT_KIND, IterateQueryString, &queryString);
		SendResponse(connection,
		             self->requestHandler->HandleGet(HttpRequest(url, method, headers, queryString, connection)));
		return MHD_YES;
	}

	if (!strcmp(method, "POST"))
	{
		HttpPostProcessor* postProcessor = (HttpPostProcessor*)*context;

		int result = MHD_YES;

		if (*uploadDataSize != 0)
		{
			result = MHD_post_process(postProcessor->mhdProcessor, uploadData, *uploadDataSize);

			*uploadDataSize = 0;
		}
		else
		{
			HttpResponse response;
			if (postProcessor->TryGetResponse(&response))
				SendResponse(connection, response);
		}


		return result;
	}

	SendResponse(connection, HttpResponse(MHD_HTTP_NOT_IMPLEMENTED));
}

void HttpServer::PostProcessRequest(void* param,
                                    MHD_Connection* connection,
                                    void** context,
                                    MHD_RequestTerminationCode toe)
{
#if DEBUG
	printf(":: post process request started\n");
#endif

	HttpPostProcessor* postProcessor = (HttpPostProcessor*)*context;

	if (!postProcessor)
		return;

	delete postProcessor;
	*context = NULL;
}

int HttpServer::IterateHeadersBase(void* cls, enum MHD_ValueKind kind, const char* key, const char* value)
{
	Headers* headers = (Headers*)cls;
	std::string keyStr = key ? key : "";
	std::transform(keyStr.begin(), keyStr.end(), keyStr.begin(), ::tolower);
	headers->insert({keyStr, value});
	return MHD_YES;
}

int HttpServer::IterateQueryString(void* cls, enum MHD_ValueKind kind, const char* key, const char* value)
{
	QueryString* args = (QueryString*)cls;
	std::string keyStr = key ? key : "";
	std::transform(keyStr.begin(), keyStr.end(), keyStr.begin(), ::tolower);
	args->insert({keyStr, value});
	return MHD_YES;
}

int HttpServer::SendResponse(MHD_Connection* connection, HttpResponse response)
{
#if DEBUG
	printf(":: send response");
#endif

	MHD_Response* mhdResponse =
	    MHD_create_response_from_buffer(response.contentLength, response.content, MHD_RESPMEM_MUST_COPY);

	if (!mhdResponse)
		return MHD_NO;

	for (const auto& iter : response.headers)
		MHD_add_response_header(mhdResponse, iter.first.c_str(), iter.second);  //"Content-Type", MIMETYPE);

	int result = MHD_queue_response(connection, response.code, mhdResponse);

	MHD_destroy_response(mhdResponse);

	free(response.content);

	return result;
}

void HttpServer::OnFatalError(void* param, const char* file, uint32_t line, const char* reason)
{
	printf("Fatal: %s at %s, line %d\n", reason, file, line);
	exit(1);
}

HttpPostProcessor::HttpPostProcessor(const HttpRequest& request)
{
	this->request = request;
	isCompleted = false;
}

void HttpPostProcessor::CreateMhdProcessor()
{
	mhdProcessor = MHD_create_post_processor(request.connection, POSTBUFFERSIZE, IteratePostDataBase, (void*)this);
	if (!mhdProcessor)
		printf("MHD_create_post_processor failed\n");
}

bool HttpPostProcessor::TryGetResponse(HttpResponse* response)
{
	if (!this->response.code)
		FinalizeRequest();

	if (!isCompleted)
		return false;

	*response = this->response;

	isCompleted = false;

	return true;
}

void HttpPostProcessor::Complete(HttpResponse response)
{
	if (isCompleted)
		return;

	this->response = response;

	isCompleted = true;
}

HttpPostProcessor::~HttpPostProcessor()
{
	if (mhdProcessor)
	{
		MHD_destroy_post_processor(mhdProcessor);
		mhdProcessor = NULL;
	}
}

int HttpPostProcessor::IteratePostDataBase(void* context,
                                           MHD_ValueKind kind,
                                           const char* key,
                                           const char* filename,
                                           const char* contentType,
                                           const char* transferEncoding,
                                           const char* data,
                                           uint64_t offset,
                                           size_t size)
{
	HttpPostProcessor* self = (HttpPostProcessor*)context;
	if (!self)
		return MHD_NO;

	return self->IteratePostData(kind, key, filename, contentType, transferEncoding, data, offset, size);
}

HttpRequest::HttpRequest()
{
	this->url = NULL;
	this->method = NULL;
	this->connection = NULL;
};

HttpRequest::HttpRequest(const char* url,
                         const char* method,
                         const Headers& headers,
                         QueryString& queryString,
                         MHD_Connection* connection)
{
	this->url = url;
	this->method = method;
	this->connection = connection;
	this->headers = headers;
	this->queryString = queryString;
};

HttpResponse::HttpResponse() : HttpResponse(0, NULL, 0, Headers())
{
}

HttpResponse::HttpResponse(uint32_t code) : HttpResponse(code, NULL, 0, Headers())
{
}

HttpResponse::HttpResponse(uint32_t code, char* content, size_t contentLength, const Headers& headers)
{
	this->code = code;
	this->content = content;
	this->contentLength = contentLength;
	this->headers = headers;
}

bool HttpRequestHandler::ParseUrl(const char* url, int parts, ...)
{
	if (url[0] != '/')
		return false;

	va_list args;

	va_start(args, parts);

	bool result = true;

	const char* position = url + 1;
	for (int i = 0; i < parts; i++)
	{
		const char* nextSlash = strchr(position, '/');

		if (!nextSlash)
		{
			if (i != parts - 1)
			{
				result = false;
				break;
			}

			nextSlash = strchr(position, '\0');
		}

		int partLength = nextSlash - position;

		const char* part = va_arg(args, const char*);

		if (!part)
		{
			size_t size = va_arg(args, size_t);

			if (partLength >= size)
			{
				result = false;
				break;
			}

			strncpy(va_arg(args, char*), position, partLength);
		}
		else
		{
			if (strlen(part) != partLength)
			{
				result = false;
				break;
			}

			if (strncmp(part, position, partLength))
			{
				result = false;
				break;
			}
		}

		position = nextSlash + 1;
	}

	va_end(args);

	return result;
}
