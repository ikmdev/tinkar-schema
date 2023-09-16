#!groovy

@Library("titan-library") _

pipeline {
    agent any

    environment {
        SONAR_AUTH_TOKEN    = credentials('sonarqube_pac_token')
        SONARQUBE_URL       = "${GLOBAL_SONARQUBE_URL}"
        SONAR_HOST_URL      = "${GLOBAL_SONARQUBE_URL}"

        GPG_PASSPHRASE      = credentials('sonarqube_pac_token')
    }

    triggers {
        //cron(cron_string)
        gitlab(triggerOnPush: true, triggerOnMergeRequest: true, branchFilterType: 'All')
    }

    options {
        // Set this to true if you want to clean workspace during the prep stage
        skipDefaultCheckout(true)

        // Console debug options
        timestamps()
        ansiColor('xterm')

        // necessary for communicating status to gitlab
        gitLabConnection('fda-shield-group')
    }

    stages {

        stage("Checkout") {
            steps {
                // Clean before build
                cleanWs()
                // We need to explicitly checkout from SCM here
                checkout scm
                echo "Building ${env.JOB_NAME}..."
            }
        }

        stage("Build ProtoC Image") {
            steps {
                script {
                    docker.build("tinkar-schema-protoc:latest", "-f protoc.dockerfile")
                }
            }
        }

        stage("Build GPG Image") {
            steps {
                script {
                    docker.build("tinkar-gpg:latest", "-f alpine-gpg.dockerfile")
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
                mkdir -p $(pwd)/src/main/java-generated
                protoc -I $(pwd) $(pwd)/Tinkar.proto \
                    --java_out=$(pwd)/src/main/java-generated
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
                mkdir -p $(pwd)/src/main/csharp-generated
                protoc -I $(pwd) $(pwd)/Tinkar.proto \
                    --csharp_out=$(pwd)/src/main/csharp-generated
                '''
                stash(name: "csharp-schema-proto", includes: 'src/**')
            }
        }

        stage('Maven Build') {
            steps {
                updateGitlabCommitStatus name: 'build', state: 'running'
                script{
                    configFileProvider([configFile(fileId: 'settings.xml', variable: 'MAVEN_SETTINGS')]) {
                        sh """
                            mvn clean install \
                                -s '${MAVEN_SETTINGS}' \
                                --batch-mode \
                                -e \
                                -Dorg.slf4j.simpleLogger.log.org.apache.maven.cli.transfer.Slf4jMavenTransferListener=warn \
                                -Dmaven.build.cache.enabled=false \
                                -PcodeQuality
                        """
                    }
                }
            }
            post {
                always {
                    archiveArtifacts artifacts: 'target/*.jar', fingerprint: true
                }
            }
        }

       stage('SonarQube Scan') {
           steps{
               configFileProvider([configFile(fileId: 'settings.xml', variable: 'MAVEN_SETTINGS')]) {
                   withSonarQubeEnv(installationName: 'EKS SonarQube', envOnly: true) {
                       // This expands the environment variables SONAR_CONFIG_NAME, SONAR_HOST_URL, SONAR_AUTH_TOKEN that can be used by any script.
                       sh """
                           mvn sonar:sonar \
                               -Dsonar.qualitygate.wait=true \
                               -Dsonar.token=${SONAR_AUTH_TOKEN} \
                               -s '${MAVEN_SETTINGS}' \
                               -Dmaven.build.cache.enabled=false \
                               -Dsonar.sources=protoc.dockerfile,csharp.dockerfile \
                               --batch-mode
                       """
                   }
               }
               script{
                   configFileProvider([configFile(fileId: 'settings.xml', variable: 'MAVEN_SETTINGS')]) {

                       def pmd = scanForIssues tool: [$class: 'Pmd'], pattern: '**/target/pmd.xml'
                       publishIssues issues: [pmd]

                       def spotbugs = scanForIssues tool: [$class: 'SpotBugs'], pattern: '**/target/spotbugsXml.xml'
                       publishIssues issues:[spotbugs]

                       publishIssues id: 'analysis', name: 'All Issues',
                               issues: [pmd, spotbugs],
                               filters: [includePackage('io.jenkins.plugins.analysis.*')]
                   }
               }
           }

           post {
               always {
                   echo "post always SonarQube Scan"
               }
           }
       }

        stage("Publish to Nexus Repository Manager") {
            steps {
                script {
                    pomModel = readMavenPom(file: 'pom.xml')
                    artifactId = pomModel.getArtifactId()
                    pomVersion = pomModel.getVersion()
                    isSnapshot = pomVersion.contains("-SNAPSHOT")
                    repositoryId = 'maven-releases'

                    if (isSnapshot) {
                        repositoryId = 'maven-snapshots'
                    }

                    configFileProvider([configFile(fileId: 'settings.xml', variable: 'MAVEN_SETTINGS')]) {
                        sh """

                            gpg --batch --delete-secret-key 00ACE017BE807B2BB1B8A1374145A7659BCFC9C5
                            gpg --batch --delete-secret-key 0B1C412FF2CBE084F8BFF36C0F2DE3547A50209A
                            gpg --batch --delete-secret-key 91020F5A4A6BA038230616AD7A922ACB195EC3B1
                            gpg --batch --delete-secret-key 792E8E5DDA00C7B795BADE67FB05F644C9B16E34
                            gpg --batch --delete-secret-key AA69531E7FDFC041FF9C80E791754F312385E64D
                            gpg --batch --delete-secret-key 67E4C2DF503127A6DD1628825A596D4F42F44685
                            gpg --batch --delete-secret-key EEDB95950F0C24DA2BB965B617A244AEA3147166
                            gpg --batch --delete-secret-key C3FE958343E991582CC4F0986B9E15981B49E1F6
                            gpg --batch --delete-secret-key D25CA71F450858FB105ED75A4EE138EF93AD4463
                            gpg --batch --delete-secret-key EB405F9E6AE3D717E1C17CA7BD01582F6654FD2A
                            gpg --batch --delete-secret-key FF250EC2D76061D2544AA5D1EAC9388F80235541
                            gpg --batch --delete-secret-key D44D989EA06FDB86A85D00E15D4A9ACBB0A71B89
                            gpg --batch --delete-secret-key 431D0DFF8D01332A4EE99DCAD1A9202D9E0CCA0E
                            gpg --batch --delete-secret-key D9829D15DB1D8A4CAE1F088D1D517151B6EBD1F8
                            gpg --batch --delete-secret-key 16AF8A92F2E1B232C2909D5202B48DF147EC9C26
                            gpg --batch --delete-secret-key D0CA31C065CB0A7A162605B0136EF5EAECB12EB3
                            gpg --batch --delete-secret-key C1C42DC39AB14556242BB8CA2796D9B7A8F86723
                            gpg --batch --delete-secret-key 64BA12715D873EC5DDFF1A268BCA01ABC1C8ACDF
                            gpg --batch --delete-secret-key 22B41789DBD2ED86DF80EF3D85BB308A86BD14B2
                            gpg --batch --delete-secret-key FCF6521D3FE631299D55ED6FDDB735BA82A81FEB
                            gpg --batch --delete-secret-key 1E40FDAABDF890AD780821315550B2853CD1D3A9
                            gpg --batch --delete-secret-key 0589B3B808B8DDE73B8E0F13190CA9C8ED78B543
                            gpg --batch --delete-secret-key B0015C219F937D4C791FAD27E1A6107ADB3177E1
                            gpg --batch --delete-secret-key 4669A03F32393F51ADEF9944259A87751CB07B2D
                            gpg --batch --delete-secret-key 2ADEDAE031735570D056F8C543959BFB9FFD9809
                            gpg --batch --delete-secret-key 64A10A493E58403CD1C92C4DA0D8964173E71411
                            gpg --batch --delete-secret-key 4860035FD6BDA1765E0AE9B3C423B305FBFE1A65
                            gpg --batch --delete-secret-key 9E913156409A929378ED715BE23D5539E6638376
                            gpg --batch --delete-secret-key 6A56AF12CC867466F1CBC670A79297C53BB61174

                            ls -l
                            cat gen-key-script gpg_passphrase
                            sed "s/GPG_PASSPHRASE/$GPG_PASSPHRASE/g" gen-key-script | gpg --batch --generate-key
                            gpg --list-secret-keys --keyid-format=long --verbose
                            
                            mvn install \
                                --batch-mode \
                                -e \
                                -Dorg.slf4j.simpleLogger.log.org.apache.maven.cli.transfer.Slf4jMavenTransferListener=warn \
                                -Dmaven.build.cache.enabled=false \
                                -DskipTests \
                                -DskipITs \
                                -Dmaven.main.skip \
                                -Dmaven.test.skip \
                                -s '${MAVEN_SETTINGS}' \
                                -Dgpg.passphrase="$GPG_PASSPHRASE"  \
                                -DsignArtifacts1=true1
                                
                            gpg --yes --verbose --pinentry-mode loopback --output hi.sig --passphrase $GPG_PASSPHRASE --sign $WORKSPACE/target/tinkar-schema-1.14.0-SNAPSHOT.jar
    
                        """
                    }
                }
            }
        }

        stage("sign the artifacts") {
            agent {
                docker {
                    image 'tinkar-gpg:latest'
                    reuseNode false
                    args '-u root:root'
                }
            }
            steps {
                script {
                    /*
                    pomModel = readMavenPom(file: 'pom.xml')
                    artifactId = pomModel.getArtifactId()
                    pomVersion = pomModel.getVersion()
                    isSnapshot = pomVersion.contains("-SNAPSHOT")
                    repositoryId = 'maven-releases'

                    if (isSnapshot) {
                        repositoryId = 'maven-snapshots'
                    }*/

                    //configFileProvider([configFile(fileId: 'settings.xml', variable: 'MAVEN_SETTINGS')]) {
                        sh """
                            ls -l
                            cat /root/gen-key-script /root/gpg_passphrase
                            sed "s/GPG_PASSPHRASE/$GPG_PASSPHRASE/g" /root/gen-key-script | gpg --batch --generate-key
                            
                            gpg --list-secret-keys --keyid-format=long --verbose
                            
                            echo Hi > hi.txt
                            ls
                            gpg --yes --verbose --pinentry-mode loopback --output hi.sig --passphrase $GPG_PASSPHRASE --sign hi.txt
                            ls   
                        """
                    //}
                }
            }
        }

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
    }
}
