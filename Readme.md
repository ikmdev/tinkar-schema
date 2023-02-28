## Tinkar Protobuf

This repository contains the Protobuf file for the Tinkar project,
and the language specific projects to build a language specific artifact (i.e. dll for csharp)
that can be accessed to read and write Tinkar files.
### Java
To generate Java based objects from the Tinkar.proto file a Docker image will build a JPMS modular jar artifact file.

**Build a Docker image**

**Step 1 :** build image by default uses Dockerfile. `-t` will tag it with the naming convention of `<user_name>/<image_name>:<version>`.
```shell
docker build -t myuser_name/tinkar-proto:latest . 
```

This will generate a resultant jar (`protobuf-1.5.0-SNAPSHOT.jar`) file inside the Docker image in the `./Java/target` directory.
In the next step when the image is run the jar file is copied to the host systems current directory.

**Step 2 :** Run with current directory as a mount
```shell
docker run -v `pwd`:/output_artifact myuser_name/tinkar-proto && ls -la *.jar
```

### C-Sharp
The C-Sharp dll is pushed to nuget and can be found at

https://www.nuget.org/packages/Tinkar.ProtoBuf-cs/

To install into Visual Studio, type the following into the Nuget console.

Install-Package Tinkar.ProtoBuf-cs

