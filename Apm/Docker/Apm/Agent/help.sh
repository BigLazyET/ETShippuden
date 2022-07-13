#!/bin/bash

docker build . -f Dockerfile -t otelcolagent


docker run -itd --net=host --name otelcolagent otelcolagent
