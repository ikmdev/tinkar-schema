@Library("titan-library") _

//run the build at 03:10 on every day-of-week from Monday through Friday but only on the main branch
String cron_string = BRANCH_NAME == "main" ? "10 3 * * 1-5" : ""

pipeline {
    agent any

    environment {

        SONAR_AUTH_TOKEN    = credentials('sonarqube_pac_token')
        SONARQUBE_URL       = "${GLOBAL_SONARQUBE_URL}"
        SONAR_HOST_URL      = "${GLOBAL_SONARQUBE_URL}"

        BRANCH_NAME         = "${GIT_BRANCH.split("/").size() > 1 ? GIT_BRANCH.split("/")[1] : GIT_BRANCH}"
    }

    options {

        // Set this to true if you want to clean workspace during the prep stage
        skipDefaultCheckout(false)

        // Console debug options
        timestamps()
        ansiColor('xterm')
    }

    stages {

        stage("Build ProtoC Image") {
            steps {
                script {
                    docker.build("tinkar-schema-protoc:latest", "-f protoc.dockerfile")
                }
            }
        }

        stage("Build CSharp Image") {
            steps {
                script {
                    docker.build("tinkar-schema-csharp:latest", "-f csharp.dockerfile")
                }
            }
        }

        // Running protoc to generate Java generated classes.
        stage("Build Java Code") {
            agent {
                docker {
                    image 'tinkar-schema-protoc:latest'
                    reuseNode true
                }
            }
            steps {
                sh '''
                protoc -I $BUILDER_PATH /home/proto-builder/Tinkar.proto \
                    --java_out=/home/proto-builder/code/java/src/main/java \
                    --proto_path=/usr/local/include/google/protobuf
                '''
                stash(name: "java-schema-proto", includes: '*')
            }
        }

        // Running protoc to generate C# generated classes.
        stage("Build CSharp Code") {
            agent {
                docker {
                    image 'tinkar-schema-protoc:latest'
                    reuseNode true
                }
            }
            steps {
                sh '''
                protoc -I $BUILDER_PATH $BUILDER_PATH/Tinkar.proto --csharp_out=/home/proto-builder/code/csharp
                '''
                stash(name: "csharp-schema-proto", includes: '*')
            }
        }

        // Generate and deploy a jar file
        stage("Deploy Java Code") {
            when {
                branch "main"
            }
            agent {
                docker {
                    image 'maven:3.9.0-amazoncorretto-19'
                    reuseNode true
                }
            }

            steps {
                script {
                    def pom = readMavenPom file: 'pom.xml'
                    def version = pom.version
                    if (!version.contains("-SNAPSHOT")) {
                        unstash(name: "java-schema-proto")
                        sh '''
                        mvn clean deploy -s '${MAVEN_SETTINGS}' --batch-mode
                        '''
                    }
                }
            }
        }

        // Building the .csproj file using dotnet commands.
        stage("Deploy C# Code") {
            when {
                branch "main"
            }
            agent {
                docker {
                    image 'tinkar-schema-csharp:latest'
                    reuseNode true
                }
            }

            steps {
                unstash(name: "csharp-schema-proto")
                sh '''
                    /root/.dotnet/dotnet restore
                    /root/.dotnet/dotnet build --no-restore
                    /root/.dotnet/dotnet pack --no-restore --no-build -o /sln/artifacts
                    '''
            }
        }
    }
}
