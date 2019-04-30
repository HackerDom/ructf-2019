FROM golang:1.12-alpine

COPY . /go/src/brainhugger

RUN apk --update add build-base make && cd /go/src/brainhugger && make

WORKDIR /go/src/brainhugger
CMD ./run.sh

