# Base image used by the build system to generate protobuf code

FROM alpine:3.17.2 AS protobufCompiler

# Make and directories that we might need for creating code
RUN mkdir -p /home/proto-builder && \
    mkdir protoc && \
    mkdir -p code/java/src/main/java

ARG BUILDER_PATH=/home/proto-builder
WORKDIR $BUILDER_PATH

# Installing protoc using apt-install and generating the correct directories for the stored generated classes.
RUN apk update && \
    apk add protobuf=3.21.9-r0 protobuf-dev=3.21.9-r0 wget unzip

