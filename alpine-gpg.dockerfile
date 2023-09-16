FROM alpine:3.17.2
WORKDIR /root
RUN apk add gnupg bash curl
COPY gen-key-script .
COPY gpg_passphrase .

