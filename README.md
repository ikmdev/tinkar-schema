# Tinkar Protobuf

This repository contains the Protobuf schema file for the Tinkar project and generates code packages for different languages that can be distributed to read and write Tinkar files.

## Prerequisites

* Java
* Maven
* C#
* NuGet

## Using Docker to Generate Java and C# Artifacts

To generate Java and C# based objects from the Tinkar.proto file, a Docker image will build a JPMS modular jar artifact file and a .nupkg file.

## Creating the Build containers (if applicable)

The proto container can be used for file generation. This is used for executing protoc commands:

```shell
docker build -t protoc -f protoc.dockerfile .
```

Similarly, the csharp/mono build container has been created for consistency in executing the csharp commands.

```shell
docker build -t csharp -f csharp.dockerfile .
```

## Note: If using an M1 chip

Docker doesn't know how to identify the specific version of the OS because of the virtualization layer on the processor. To encourage it to execute the right one, add the following to any build command:

```shell
--platform linux/x86_64
```

## Generate a Source files for Java

To create the Java source files, use something like the following:

```shell
protoc -I <location of current direct> Tinkar.proto --java_out=<target directory>
```

An example of running this and generating the files for your local environment would look something like this:

```shell
docker run -it -v "$(pwd)/src:/home/proto-builder/src" protoc sh -c "mkdir -p /home/proto-builder/src/main/java && protoc -I /home/proto-builder/ Tinkar.proto --java_out=/home/proto-builder/src/main/java"
```

## Creating a Java Package

Once you have created the protoc, use the following to create a Java jar file that will be installed in your local maven repository.

```shell
mvn clean install
```

## Using the Java Package

The C-Sharp dll is pushed to Maven Central and can be found at:

https://www.nuget.org/packages/Tinkar.ProtoBuf-cs/

To use this dependency in maven, include the following dependency (replacing `${tinkar.version}` with the version that you would prefer to use):

```xml
<dependency>
    <groupId>dev.ikm.tinkar</groupId>
    <artifactId>tinkar-schema</artifactId>
    <version>${tinkar.version}</version>
</dependency>
```

## Generate a Source files for C#

If you want to create the Java source files, you should something like the following:

```shell
protoc -I <location of current direct> Tinkar.proto --csharp_out=<target directory>
```

An example of running this and generating the files for your local environment would look something like this:

```shell
docker run -it -v "$(pwd)/code:/home/proto-builder/code" protoc sh -c "mkdir -p /home/proto-builder/code/csharp && protoc -I /home/proto-builder/ Tinkar.proto --csharp_out=/home/proto-builder/code/csharp"
```

## Creating a C# package

To create the C# package, restore the installation, then build everything and package it using the following commands:

```shell
dotnet restore
dotnet build --no-restore
dotnet pack --no-restore --no-build -o /sln/artifacts
```

## Using the Java Package

The C-Sharp dll is pushed to NuGet and can be found at:

https://www.nuget.org/packages/Tinkar.Schema/

To install into Visual Studio, type the following into the NuGet console.

```shell
Install-Package Tinkar.Schema
```

## Issues and Contributions
Technical and non-technical issues can be reported to the [Issue Tracker](https://github.com/ikmdev/tinkar-schema/issues).

Contributions can be submitted via pull requests. Please check the contribution guide for more details.
