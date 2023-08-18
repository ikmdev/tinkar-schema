#!groovy

@Library("titan-library") _

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

        // necessary for communicating status to gitlab
        gitLabConnection('fda-shield-group')
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
                    args '-u root:root'
                }
            }
            steps {
                sh '''
                mkdir -p $(pwd)/src/main/java
                protoc -I $(pwd) $(pwd)/Tinkar.proto \
                    --java_out=$(pwd)/src/main/java
                pwd
                ls -R /home/proto-builder/
                ls -R src/
                '''
                stash(name: "java-schema-proto", allowEmpty: false, useDefaultExcludes: false, includes: 'src/**')
            }
        }

        // Running protoc to generate C# generated classes.
        stage("Build CSharp Code") {
            agent {
                docker {
                    image 'tinkar-schema-protoc:latest'
                    reuseNode true
                    args '-u root:root'
                }
            }
            steps {
                sh '''
                mkdir -p $(pwd)/src/main/csharp
                protoc -I $(pwd) $(pwd)/Tinkar.proto \
                    --csharp_out=$(pwd)/src/main/csharp
                '''
                stash(name: "csharp-schema-proto", includes: 'src/**')
            }
        }

        stage('SonarQube Scan') {
            steps {
                configFileProvider([configFile(fileId: 'settings.xml', variable: 'MAVEN_SETTINGS')]) {
                    withSonarQubeEnv(installationName: 'EKS SonarQube', envOnly: true) {
                        // This expands the environment variables SONAR_CONFIG_NAME, SONAR_HOST_URL, SONAR_AUTH_TOKEN that can be used by any script.
                        sh """
                            mvn sonar:sonar \
                                -Dsonar.qualitygate.wait=true \
                                -Dsonar.token=${SONAR_AUTH_TOKEN} \
                                -s '${MAVEN_SETTINGS}' \
                                --batch-mode
                        """
                    }
                }
                script {
                    configFileProvider([configFile(fileId: 'settings.xml', variable: 'MAVEN_SETTINGS')]) {

                        def pmd = scanForIssues tool: [$class: 'Pmd'], pattern: '**/target/pmd.xml'
                        publishIssues issues: [pmd]

                        def spotbugs = scanForIssues tool: [$class: 'SpotBugs'], pattern: '**/target/spotbugsXml.xml'
                        publishIssues issues: [spotbugs]

                        publishIssues id: 'analysis', name: 'All Issues',
                                issues: [pmd, spotbugs],
                                filters: [includePackage('io.jenkins.plugins.analysis.*')]
                    }
                }
            }
        }

            // Generate and deploy a jar file
        stage("Deploy Java Code") {
            agent {
                docker {
                    image 'maven:3.8.7-eclipse-temurin-19-alpine'
                    args '-u root:root'
                }
            }

            steps {
                unstash(name: "java-schema-proto")

                configFileProvider([configFile(fileId: 'settings.xml', variable: 'MAVEN_SETTINGS')]) {
                    sh "ls -R ."
                    sh "mvn clean deploy -s '${MAVEN_SETTINGS}' --batch-mode"
                }
            }
        }

        // Building the .csproj file using dotnet commands.
//         stage("Deploy C# Code") {
//             when {
//                 branch "main"
//             }
//             agent {
//                 docker {
//                     image 'tinkar-schema-csharp:latest'
//                     args '-u root:root'
//
//                 }
//             }
//
//             steps {
//                 unstash(name: "csharp-schema-proto")
//                 sh '''
//                     /root/.dotnet/dotnet restore
//                     /root/.dotnet/dotnet build --no-restore
//                     /root/.dotnet/dotnet pack --no-restore --no-build -o /sln/artifacts
//                     '''
//             }
//         }
    }

    post {
        failure {
            updateGitlabCommitStatus name: 'build', state: 'failed'
            emailext(

                    recipientProviders: [requestor(), culprits()],
                    subject: "Build failed in Jenkins: ${env.JOB_NAME} - #${env.BUILD_NUMBER}",
                    body: """
                        Build failed in Jenkins: ${env.JOB_NAME} - #${BUILD_NUMBER}

                        See attached log or URL:
                        ${env.BUILD_URL}

                    """,
                    attachLog: true
            )
        }
        aborted {
            updateGitlabCommitStatus name: 'build', state: 'canceled'
        }
        unstable {
            updateGitlabCommitStatus name: 'build', state: 'failed'
            emailext(
                    subject: "Unstable build in Jenkins: ${env.JOB_NAME} - #${env.BUILD_NUMBER}",
                    body: """
                        See details at URL:
                        ${env.BUILD_URL}

                    """,
                    attachLog: true
            )
        }
        changed {
            updateGitlabCommitStatus name: 'build', state: 'success'
            emailext(
                    recipientProviders: [requestor(), culprits()],
                    subject: "Jenkins build is back to normal: ${env.JOB_NAME} - #${env.BUILD_NUMBER}",
                    body: """
                    Jenkins build is back to normal: ${env.JOB_NAME} - #${env.BUILD_NUMBER}

                    See URL for more information:
                    ${env.BUILD_URL}
                    """
            )
        }
        success {
            updateGitlabCommitStatus name: 'build', state: 'success'
        }
        cleanup {
            // Clean the workspace after build
            cleanWs(cleanWhenNotBuilt: false,
                    deleteDirs: true,
                    disableDeferredWipeout: true,
                    notFailBuild: true,
                    patterns: [
                            [pattern: '.gitignore', type: 'INCLUDE']
                    ])
        }
    }
}
