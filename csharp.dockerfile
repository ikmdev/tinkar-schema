FROM mono:latest AS nugetBuilder

# Setting ARG commands
ARG BUILDER_PATH=/home/proto-builder
# Copying over the C# generated classes from the first image.
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
COPY Tinkar.proto /sln
COPY LICENSE /sln



