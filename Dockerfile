FROM ubuntu:latest AS protobufCompiler

RUN mkdir /home/proto-builder
ARG BUILDER_PATH=/home/proto-builder
WORKDIR $BUILDER_PATH
COPY Tinkar.proto .

# Installing protoc using apt-install and generating the correct directories for the stored generated classes.
RUN mkdir protoc && \
    mkdir -p code/java/src/main/java && \
    mkdir -p code/csharp && \
    apt update -y && \
    apt install -y protobuf-compiler wget unzip
WORKDIR /proto-builder/protoc/bin

# Running protoc to generate both C# and Java generated classes.
RUN protoc -I $BUILDER_PATH $BUILDER_PATH/Tinkar.proto --java_out=$BUILDER_PATH/code/java/src/main/java && \
    protoc -I $BUILDER_PATH $BUILDER_PATH/Tinkar.proto --csharp_out=$BUILDER_PATH/code/csharp


FROM maven:3.9.0-amazoncorretto-19 AS mavenBuilder

RUN mkdir /home/proto-builder
ARG BUILDER_PATH=/home/proto-builder
WORKDIR $BUILDER_PATH

# Copying over generated classes over from the previous image.
COPY --from=protobufCompiler $BUILDER_PATH/code/java ./
COPY pom.xml .

# Generating the binary into a jar file.
RUN mvn clean install && \
    mkdir output_artifact

# TODO: This is where maven deploy would be set-up/configured
RUN cp $BUILDER_PATH/target/protobuf-*.jar $BUILDER_PATH/output_artifact && \
    ls -l && \
    ls -R target

FROM mono:latest AS nugetBuilder

# Setting ARG commands
ARG BUILDER_PATH=/home/proto-builder
# Copying over the C# generated classes from the first image.
COPY --from=protobufCompiler $BUILDER_PATH/code/csharp $BUILDER_PATH/csharp-generated
WORKDIR $BUILDER_PATH/csharp-generated

RUN apt-get update

# Installing NuGet and dotnet which are required for the C# to be built.
RUN apt install nuget
RUN apt-get update && \
    mkdir /root/.dotnet && \
    apt-get install -y wget && \
    cd /root/.dotnet && \
    wget https://download.visualstudio.microsoft.com/download/pr/17b6759f-1af0-41bc-ab12-209ba0377779/e8d02195dbf1434b940e0f05ae086453/dotnet-sdk-6.0.100-linux-x64.tar.gz && \
    tar -xf dotnet-sdk-6.0.100-linux-x64.tar.gz && \
    rm -rf dotnet-sdk-6.0.100-linux-x64.tar.gz \
ARG Version
WORKDIR /sln
COPY Tinkar.ProtoBuf-cs.csproj  /sln
COPY . .

# Building the .csproj file using dotnet commands.
RUN /root/.dotnet/dotnet restore
RUN /root/.dotnet/dotnet build --no-restore
RUN /root/.dotnet/dotnet pack --no-restore --no-build -o /sln/artifacts
COPY --from=mavenBuilder $BUILDER_PATH/output_artifact/*.jar /sln/artifacts