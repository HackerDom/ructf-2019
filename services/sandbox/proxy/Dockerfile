FROM ubuntu:18.04

RUN apt-get update && \
	apt-get install -y libmicrohttpd-dev

ADD . /root/proxy
ENV LD_LIBRARY_PATH /root/proxy
WORKDIR /root/proxy

CMD ["./proxy"]