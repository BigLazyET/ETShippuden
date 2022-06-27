#!/bin/bash

export BOOTDLL=Learn.Demo.WebApp.dll
export PORT0=16666
export DAOKEAPPUK=opentelemetry.collector.test
export DAOKEID=1666
export DAOKEENVTYPE=qa
export DAOKEENV=qa

export ASPNETCORE_URLS=http://0.0.0.0:${PORT0}/
#echo $(date "+%Y-%m-%d %H:%M:%S") [INFO] ASPNETCORE_URLS=${ASPNETCORE_URLS}
#echo $(date "+%Y-%m-%d %H:%M:%S") [INFO] starting up the project "\""${APPNAME}"\""

# mkdir for log
mkdir -p /data/logs/console-${DAOKEAPPUK}-${DAOKEID}/

# project run by supervisord，and set ProcDump trigger
/usr/bin/supervisord -c /etc/supervisor/supervisord.conf

# supervisord watch dotnet process stdout(not the supervisord itself process stdout) as docker console stdout
supervisorctl tail -f dotnet


