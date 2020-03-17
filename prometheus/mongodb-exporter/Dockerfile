FROM alpine
RUN apk update && apk add wget && rm -rf /var/cache/apk/*
RUN wget https://github.com/dcu/mongodb_exporter/releases/download/v1.0.0/mongodb_exporter-linux-amd64
RUN chmod 777 mongodb_exporter-linux-amd64
EXPOSE 9001