#!/bin/bash

docker build . -f Dockerfile -t optlcolcontrib

docker run -itd --net=host --name optlcolcontrib optlcolcontrib
