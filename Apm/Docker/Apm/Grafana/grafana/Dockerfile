FROM grafana/grafana

COPY ./grafana.ini /etc/grafana/

RUN grafana-cli plugins install novatec-sdg-panel
