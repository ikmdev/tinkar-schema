# Base image used by the build system to generate protobuf code

FROM alpine:3.17.2

# Make and directories that we might need for creating code
RUN mkdir -p /home/proto-builder

ARG BUILDER_PATH=/home/proto-builder
WORKDIR $BUILDER_PATH

# Installing protoc using apt-install and generating the correct directories for the stored generated classes.
RUN apk update && \
    apk add protobuf=3.21.9-r0 protobuf-dev=3.21.9-r0 wget unzip gnupg haveged tini

COPY Tinkar.proto .
COPY gen-key-script .
RUN gpg --default-new-key-algo --no-tty --yes rsa4096 --gen-key
RUN gpg --list-secret-keys --keyid-format=long --verbose