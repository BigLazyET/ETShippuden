FROM victoriametrics/victoria-metrics
COPY ./repositories /etc/apk/repositories
COPY VictoriaMetrics/prometheus.yml /
CMD ["--promscrape.config=/prometheus.yml", "-search.maxUniqueTimeseries=3000000"]