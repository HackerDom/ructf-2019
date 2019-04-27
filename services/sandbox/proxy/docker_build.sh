#!/bin/bash
docker build --tag=proxy .
docker run --network="host" --restart=on-failure --detach proxy
