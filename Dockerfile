ARG BASE_IMAGE_REGISTRY_URL=""
FROM maven:3.8.7-eclipse-temurin-19-focal

###################################################
# Run updates and add required packages
###################################################
RUN apk update && \
    apk add --no-cache protobuf-dev 