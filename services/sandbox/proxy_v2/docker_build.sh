#!/bin/bash
docker build --tag=proxy_v2 .
docker run --network="host" --restart=on-failure --detach proxy_v2
