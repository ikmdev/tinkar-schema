FROM alpine:3.17.2
WORKDIR /root
COPY gen-key-script .
RUN apk add gnupg bash curl