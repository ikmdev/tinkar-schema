#FROM --platform=linux/arm64 ubuntu:latest AS protobufCompiler
FROM ubuntu:latest AS protobufCompiler

WORKDIR /home
COPY Tinkar.proto .
RUN mkdir protoc && \
    mkdir -p code/java/src/main/java && \
    mkdir -p code/csharp && \
    apt update -y && \
    apt install -y protobuf-compiler wget unzip
WORKDIR /home/protoc/bin
RUN ls -la
RUN protoc -I /home /home/Tinkar.proto --java_out=/home/code/java/src/main/java && \
    protoc -I /home /home/Tinkar.proto --csharp_out=/home/code/csharp


FROM maven:3.9.0-amazoncorretto-19 AS mavenBuilder

WORKDIR /home
COPY --from=protobufCompiler /home/code/java ./
COPY pom.xml .
# Generating the binary
RUN mvn clean install && \
    mkdir output_artifact
# mvn deploy (nexus) TODO: [Jenkins to push to nexus or the container?]
RUN cp /home/target/protobuf-1.5.0-SNAPSHOT.jar /home/output_artifact && \
    ls -l && \
    ls -R target
#Can also run deploy to nexus in line above

FROM mono:latest AS nugetBuilder

COPY --from=protobufCompiler /home/code/csharp /home/csharp-generated
COPY CSharp/Projects/Tinkar.Protobuf-cs/Tinkar.ProtoBuf-cs.csproj  /home/csharp-generated
WORKDIR /home/csharp-generated
# Nuget is installing because when running 'nuget ?'
RUN apt-get update
#TODO: should I add NuGet restore here?
RUN apt install nuget
RUN apt-get update && \
    mkdir ~/.dotnet && \
    apt-get install -y wget && \
    cd ~/.dotnet && \
    wget https://download.visualstudio.microsoft.com/download/pr/17b6759f-1af0-41bc-ab12-209ba0377779/e8d02195dbf1434b940e0f05ae086453/dotnet-sdk-6.0.100-linux-x64.tar.gz && \
    tar -xf dotnet-sdk-6.0.100-linux-x64.tar.gz && \
    export PATH="$PATH:$HOME/.dotnet" && \
    rm -rf dotnet-sdk-6.0.100-linux-x64.tar.gz \
    #    echo 'export PATH="$PATH:/usr/share/dotnet"' &>> .bashrc && \
    ## Use MSC to run commands
WORKDIR /sln
COPY . .
#TODO: Add after dotnet is installed and working:
#ARG Version
#WORKDIR /sln
#COPY . .
#RUN ls -la
#RUN dotnet restore
#RUN dotnet build /p:Version=$Version -c Release --no-restore
#RUN dotnet pack /p:Version=$Version -c Release --no-restore --no-build -o /sln/artifacts
#RUN dotnet nuget push /sln/artifacts/*.nupkg --source https://api.nuget.org/v3/index.json --api-key MY-SECRET-KEY

#RUN dotnet add package Google.Protobuf --version 3.22.0
##TODO: Trying to get DONET installed
#RUN export DEBIAN_FRONTEND=noninteractive \
#    apt-get update \
#    # Install prerequisites \
#    apt-get install -y --no-install-recommends \
#       wget \
#       ca-certificates \
#    \
#    # Install Microsoft package feed
#    && wget -q https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb \
#    && dpkg -i packages-microsoft-prod.deb \
#    && rm packages-microsoft-prod.deb \
#    \
#    # Install .NET
#    && apt-get update \
#    && apt-get install -y --no-install-recommends \
#        dotnet-runtime-6.0 \
#    \
#    # Cleanup
#    && rm -rf /var/lib/apt/lists/*
#TODO: End of attempt from 2-25-23


##TODO: Andrew's version with windows image 2-23-23
#FROM microsoft/dotnet:2.1.300-rc1-sdk AS nugetbuilder
#
#COPY --from=protobufCompiler /home/code/csharp ./
#ARG Version
#WORKDIR /sln
#COPY . .
#RUN ls -la
#RUN dotnet restore
#RUN dotnet build /p:Version=$Version -c Release --no-restore
#RUN dotnet pack /p:Version=$Version -c Release --no-restore --no-build -o /sln/artifacts
#RUN dotnet nuget push /sln/artifacts/*.nupkg --source https://api.nuget.org/v3/index.json --api-key MY-SECRET-KEY

