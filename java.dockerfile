FROM alpine:latest AS protobufCompiler

RUN mkdir /home/proto-builder
ARG BUILDER_PATH=/home/proto-builder
WORKDIR $BUILDER_PATH
COPY Tinkar.proto .

# Installing protoc using apt-install and generating the correct directories for the stored generated classes.
RUN apk update && \
    apk add protobuf wget unzip protobuf-dev

RUN mkdir protoc && \
    mkdir -p code/java/src/main/java

# Running protoc to generate both C# and Java generated classes.
RUN protoc -I $BUILDER_PATH $BUILDER_PATH/Tinkar.proto \
    --java_out=$BUILDER_PATH/code/java/src/main/java \
    --proto_path=/usr/local/include/google/protobuf

FROM maven:3.9.0-amazoncorretto-19 AS mavenBuilder

RUN mkdir /home/proto-builder
ARG BUILDER_PATH=/home/proto-builder
WORKDIR $BUILDER_PATH

# Copying over generated classes over from the previous image.
COPY --from=protobufCompiler $BUILDER_PATH/code/java ./
COPY pom.xml .

# Generating the binary into a jar file.
RUN mvn clean deploy
