#!groovy

@Library('jira-shared-library@master') _
library('common-shared@master')

pipeline {
    agent any
    stages {
        stage ('Set Vars') {
            steps {
                script {
                    scmVars = setVars()
                    testFail = false
                }
            }
        }
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
        

            stage ('Static code analysis (Sonarqube)')
            {
                environment {
                    scannerHome = tool 'sonar-scanner'
                }
                
                steps {
                    withSonarQubeEnv('sonar') {
                        sh "${scannerHome}/bin/sonar-scanner"
                    }
                    
                    timeout(time:2, unit: 'MINUTES') {
                        waitForQualityGate abortPipeline: true
                    }
                }
            }

        
        stage ('DotNet XUnit Tests') {
            steps {
                script{
                    try {
                        sh "dotnet test"
                    } catch(e){
                        if(scmVars.ACTION == "PR-TEST"){
                            currentBuild.result = 'SUCCESS'
                            testFail = true
                            sh "exit 0"
                        } else {
                            sh "exit 1"
                        }
                    }
                    
                }
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
        
        stage ('Keyword Actions') {
            steps{
                script{
                    println scmVars.ISSUE_ID
                    if(scmVars.ACTION == 'PR'){
                        transitionIssue("https://jira.mvs.easlab.co.uk",scmVars.ISSUE_ID,"Peer Review")
                        updateIssue("https://jira.mvs.easlab.co.uk",scmVars.ISSUE_ID,'{"description":"' + scmVars.COMMIT_MSG + '"}')
                    }
                    
                    if(scmVars.ACTION == 'PR-TEST' && testFail){
                        transitionIssue("https://jira.mvs.easlab.co.uk",scmVars.ISSUE_ID,"Tests Peer Review")
                        updateIssue("https://jira.mvs.easlab.co.uk",scmVars.ISSUE_ID,'{"description":"' + scmVars.COMMIT_MSG + '"}')
                        createGitHubPR("https://api.github.com","test","endgame-poc","terrencehatchet","master","rhys-test",scmVars.ISSUE_ID,"TEST PR for "+scmVars.ISSUE_ID)
                    }
                    if(testFail){
                        echo "tests failed"
                    }
                }
            }
        }
        
    }
}
