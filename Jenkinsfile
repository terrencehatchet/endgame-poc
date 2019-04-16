pipeline {
    agent any
    stages {
        stage ('DotNet Restore') {
            steps {
                sh "dotnet restore"
            }
        }
        
        stage ('DotNet Build') {
            steps {
                sh "dotnet build"
            }
        }
        
        stage ('DotNet Publish') {
            steps {
                sh "dotnet publish -c Release"
                sh "echo ${WORKSPACE}"
                sh "ls ./bin/Release/netcoreapp2.2/publish"                
            }
        }
        
        stage ('Docker Build') {
            steps {
                sh "docker -H docker:2375 build -t registry.internallab.co.uk/mvs/endgame-poc:${env.BUILD_NUMBER} ."
            }
        }
        
        stage ('Docker Publish') {
            steps {
                sh "docker -H docker:2375 push registry.internallab.co.uk/mvs/endgame-poc:${env.BUILD_NUMBER}"
            }
        }
        
    }
}
