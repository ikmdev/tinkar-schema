ARG BASE_IMAGE_REGISTRY_URL=""
FROM https://docker.build.tinkarbuild.com:5000/docker-internal/java:17.0.2

###################################################
# Run updates and add required packages
###################################################
RUN apk update && \
    apk add --no-cache protobuf