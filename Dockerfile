ARG BASE_IMAGE_REGISTRY_URL=""
FROM ${BASE_IMAGE_REGISTRY_URL}alpine:3.15.0

###################################################
# Run updates and add required packages
###################################################
RUN apk update && \
    apk add --no-cache openjdk17~=17.0 maven~=3.8 protobuf