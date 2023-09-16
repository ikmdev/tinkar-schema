FROM alpine:3.17.2
RUN apk add gnupg bash curl
COPY gen-key-script /root
COPY gpg_passphrase /root

