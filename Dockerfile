ARG BASE_IMAGE_REGISTRY_URL=""
FROM ${GLOBAL_NEXUS_SERVER_URL}/${GLOBAL_NEXUS_REPO_NAME}/java:17.0.2

###################################################
# Run updates and add required packages
###################################################
RUN apk update && \
    apk add --no-cache protobuf