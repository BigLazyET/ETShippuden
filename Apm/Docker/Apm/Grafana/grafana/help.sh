#!/bin/bash

docker build . -f Dockerfile -t grafana:optl

docker run -itd -p 3001:3000 --name=grafana grafana:optl
