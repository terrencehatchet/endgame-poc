#!groovy

@Library('jira-shared-library@master') _
library('common-shared@master')

//Endpoints
jiraURL = "https://jira.mvs.easlab.co.uk"
dockerRegistry = "registry.internallab.co.uk"
nexusURL = "https://nexus.mvs.easlab.co.uk/repository/dotnet-packages/"
gitHubURL = "https://api.github.com"


pipeline {
    agent any
    stages {
        stage ('Set Vars') {
            steps {
                script {
                    deleteDir()
                    checkout scm
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
                
                sh "docker -H docker:2375 build -t ${dockerRegistry}/mvs/${scmVars.PROJ_NAME}:${env.BUILD_NUMBER} ."
            }
        }
        
        stage ('Docker Publish') {
            steps {
                sh "docker -H docker:2375 login -u jenkins -p Renegade187! ${dockerRegistry}"
                sh "docker -H docker:2375 push ${dockerRegistry}/mvs/${scmVars.PROJ_NAME}:${env.BUILD_NUMBER}"
            }
        }
        
        stage ('Docker Deploy') {
            steps {
                sh "docker -H docker:2375 rm -f ${scmVars.PROJ_NAME}"
                sh "docker -H docker:2375 run -d -p 8080:8080 --name ${scmVars.PROJ_NAME} ${dockerRegistry}/mvs/${scmVars.PROJ_NAME}:${env.BUILD_NUMBER}"
            }
        }
        
        stage ('Functional Test') {
            steps {
                sh "curl curl https://endgame-poc.mvs.easlab.co.uk/api/v1/Applications/10%2F50309612-09/Mortgage/status?intermediaryId=TJLP39928"
            }
        }
        
        stage ('Keyword Actions') {
            when {
                expression { !env.CHANGE_URL }
            }
            steps{
                script{
                    if(env.GIT_BRANCH.contains('RELEASE')){
                        if(scmVars.ACTION == 'PR'){
                            transitionIssue(jiraURL,scmVars.ISSUE_ID,"Merged to Parent")
                            updateIssue(jiraURL,scmVars.ISSUE_ID,'{"description":"' + scmVars.COMMIT_MSG + '"}')
                        }
                    
                        if(scmVars.ACTION == 'PR-TEST' && testFail){
                            transitionIssue(jiraURL,scmVars.ISSUE_ID,"Tests Defined")
                            updateIssue(jiraURL,scmVars.ISSUE_ID,'{"description":"' + scmVars.COMMIT_MSG + '"}')
                        }
                    } else {
                        if(scmVars.ACTION == 'PR'){
                            transitionIssue(jiraURL,scmVars.ISSUE_ID,"Peer Review")
                            updateIssue(jiraURL,scmVars.ISSUE_ID,'{"description":"' + scmVars.COMMIT_MSG + '"}')
                            createGitHubPR(gitHubURL,"development",scmVars.PROJ_NAME,"terrencehatchet","RELEASE-19.05",env.GIT_BRANCH,scmVars.ISSUE_ID,"Development PR for "+scmVars.ISSUE_ID)
                        }

                        if(scmVars.ACTION == 'PR-TEST' && testFail){
                            transitionIssue(jiraURL,scmVars.ISSUE_ID,"Tests Peer Review")
                            updateIssue(jiraURL,scmVars.ISSUE_ID,'{"description":"' + scmVars.COMMIT_MSG + '"}')
                            //need to get parent branch in SCMVARS
                            createGitHubPR(gitHubURL,"test",scmVars.PROJ_NAME,"terrencehatchet","RELEASE-19.05",env.GIT_BRANCH,scmVars.ISSUE_ID,"TEST PR for "+scmVars.ISSUE_ID)
                        }
                        if(scmVars.ACTION == 'PR-TEST' && !testFail){
                            currentBuild.result = 'FAILURE'
                            echo "tests should fail during this run but didn't"
                            sh "exit 1"
                        }
                    }
                }
            }
        }
        
        stage ('Publish Artefact') {
            when {
                branch "master"
            }
            steps {
                script{
                    buildDotNetDeb(scmVars.PROJ_NAME,"0.1",nexusURL,"ubuntu.16.04-x64","netcoreapp2.2")
                    buildDotNetRPM(scmVars.PROJ_NAME,"0.1",nexusURL,"ubuntu.16.04-x64","netcoreapp2.2")
                    publishTarball(scmVars.PROJ_NAME,"0.1",nexusURL)
                    def mergedIssues = getAllMergedIssues(scmVars.COMMIT_HASH)
                    def mergedIssueSize = mergedIssues.size()
                    mergedIssueSize.times {
                        transitionIssue(jiraURL,mergedIssues[it],"Done")
                    }
                }
            }
        }
        
    }
}
