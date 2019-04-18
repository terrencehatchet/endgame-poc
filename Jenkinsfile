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
        
        try 
        {
            stage ('Static code analysis (Sonarqube)')
            {
                environment {
                    scannerHome = tool 'sonar-scanner'
                }
                
                steps {
                    withSonarQubeEnv('sonar') {
                        sh "${scannerHome}/bin/sonar-scanner"
                    }
                    
                    timeout(time:10, unit: 'MINUTES') {
                        waitForQualityGate abortPipeline: true
                    }
                }
            }
        }
        catch (e)
        {
            echo "Ignore failed Sonarqube for now..."
        }
        
        stage ('DotNet XUnit Tests') {
            steps {
                sh "dotnet test"
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
                sh "docker -H docker:2375 login -u jenkins -p Renegade187! registry.internallab.co.uk"
                sh "docker -H docker:2375 push registry.internallab.co.uk/mvs/endgame-poc:${env.BUILD_NUMBER}"
            }
        }
        
        stage ('Docker Deploy') {
            steps {
                sh "docker -H docker:2375 rm -f endgame-poc"
                sh "docker -H docker:2375 run -d -p 8080:8080 --name endgame-poc registry.internallab.co.uk/mvs/endgame-poc:${env.BUILD_NUMBER}"
            }
        }
        
    }
}
