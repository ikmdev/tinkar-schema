# Tinkar Protobuf

This repository contains the Protobuf file for the Tinkar project,
and the language specific projects to build a language specific artifact (i.e. dll for csharp)
that can be accessed to read and write Tinkar files.

## Using Docker to Generate Java and C# Artifacts:
To generate Java and C# based objects from the Tinkar.proto file a Docker image will build a JPMS modular jar artifact file and a .nupkg file.

## Build a Docker image:**

If you are on a Macbook with a M1 chip run this command instead of the one mentioned in step 1.
```shell
docker build --platform linux/x86_64 -f csharp.dockerfile .
```

If you are on any other computer run this command instead of the one mentioned in step 1.
```shell
docker build -f csharp.dockerfile .
```

**Step 1:** build image by default uses Dockerfile. `-t` will tag it with the naming convention of `<user_name>/<image_name>:<version>`.
```shell
docker build . 
```

This will generate 3 files (`protobuf-1.5.0-SNAPSHOT.jar`, `protobuf-1.5.0-SNAPSHOT-sources.jar`, and `Tinkar.ProtoBuf-cs.1.4.1.nupkg`) files inside the Docker image in the `./sln/artifacts` directory.
In the next step when the image is run the jar file is copied to the host systems current directory.

**Step 2 (Optional):**
To run your image run the following command. If a M1 Macbook is not being used remove the ``--platform linux/x86_64`` portion.
```shell
docker run -it --platform linux/x86_64 myuser_name/tinkar-protobuf-csharp-java bash
```

### C-Sharp
The C-Sharp dll is pushed to nuget and can be found at

https://www.nuget.org/packages/Tinkar.ProtoBuf-cs/

To install into Visual Studio, type the following into the Nuget console.

Install-Package Tinkar.ProtoBuf-cs
