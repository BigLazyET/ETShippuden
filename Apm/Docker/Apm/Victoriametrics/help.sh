#!/bin/bash

docker build . -f Dockerfile -t victoriametrics:optl


docker run -itd --net=host -p 8428:8428 --name victoriametrics victoriametrics:optl
