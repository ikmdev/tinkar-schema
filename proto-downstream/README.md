# Downstream-compiled `.proto` sources

Schemas here are **packaged into the tinkar-schema jar but not compiled by this
project**. `proto/` holds the schemas this project does compile and ships as Java;
this directory holds the ones its consumers compile themselves. Downstream projects unpack them and run their own `protoc`.

That is deliberate. `tinkar_search.proto` declares
`option java_package = "dev.ikm.tinkar.service.proto"` and needs the gRPC service
stubs that `protoc-gen-grpc-java` generates. This project does not configure that
plugin, and compiling the file here anyway would emit message classes under
`dev.ikm.tinkar.service.proto` into this jar that collide with the ones
tinkar-service and komet-grpc-plugin already generate. That is why this directory
sits outside `proto/`, which *is* on the `protobuf-maven-plugin`
`sourceDirectories` and ships as generated Java.

## Consumers

`tinkar-service` and `komet-grpc-plugin` each unpack `proto/tinkar_search.proto`
from this jar and generate their own stubs. It lives here so the schema has one
owner: the two of them previously kept hand-synced copies, and a field added to one
and missed in the other failed at runtime as a silently empty value rather than at
build time.

Keep this file free of anything either consumer cannot generate.
