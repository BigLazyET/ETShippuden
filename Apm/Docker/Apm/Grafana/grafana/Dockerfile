FROM grafana/grafana:latest

COPY ./ /etc/grafana/

RUN grafana-cli plugins install novatec-sdg-panel
