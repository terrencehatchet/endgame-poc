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
