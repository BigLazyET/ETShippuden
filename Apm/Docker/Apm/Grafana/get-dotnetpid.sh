#!/bin/bash

dotnetpid=No
until [ -n $dotnetpid ] && [ $dotnetpid != "No" ]
do
 dotnetpid=`dotnet-counters ps | awk {'print$1'}`
 echo $(date "+%Y-%m-%d %H:%M:%S") [INFO] there is no dotnet process running, dotnetpid=$dotnetpid
 sleep 5
done

echo $(date "+%Y-%m-%d %H:%M:%S") [INFO] dotnet process is running...
export DOTNETPID=$dotnetpid
echo $(date "+%Y-%m-%d %H:%M:%S") [INFO] the pid of dotnet=$DOTNETPID
