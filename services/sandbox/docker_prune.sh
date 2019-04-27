#!/bin/bash

if [[ ! -z `docker container ls -aq` ]]; then
docker container stop $(docker container ls -aq)
docker container rm $(docker container ls -aq)
fi

if [[ ! -z `docker image ls | grep proxy` ]]; then
docker image rm proxy
fi

if [[ ! -z `docker image ls | grep proxy_v2` ]]; then
docker image rm proxy_v2
fi