mkdir -p $(pwd)/src/main/java-generated
protoc -I $(pwd) $(pwd)/Tinkar.proto --java_out=$(pwd)/src/main/java-generated
pwd
ls -R /home/proto-builder/
ls -R src/