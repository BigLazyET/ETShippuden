#!/bin/bash

until [ -n "$dotnetpid" ]
do
 sdotnetpid=`supervisorctl pid dotnet`
 dotnetpid=`pgrep -P $sdotnetpid`
 echo $(date "+%Y-%m-%d %H:%M:%S") [INFO] there is no dotnet process running, dotnetpid=$dotnetpid
 sleep 5
done

export DOTNETPID=$dotnetpid
echo $(date "+%Y-%m-%d %H:%M:%S") [INFO] the pid of dotnet=$DOTNETPID

# /opt/otel/otelcol --config /opt/otel/config.yaml --metrics-addr ""
/opt/otel/otelcolcontrib --config /opt/otel/configcontrib.yaml
