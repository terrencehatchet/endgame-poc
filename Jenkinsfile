#!groovy

@Library('jira-shared-library@master') _
library('common-shared@master')

jiraURL = "https://jira.mvs.easlab.co.uk"
dockerRegistry = "registry.internallab.co.uk"
nexusURL = "https://nexus.mvs.easlab.co.uk/repository/dotnet-packages/"
gitHubURL = "https://api.github.com"
testFail = false

pipeline {
  agent any
  stages {
    stage('Set Vars') {
      steps {
        script {
            sh "env"
          deleteDir()
          checkout scm
          scmVars = setVars()
          jiraFields = readIssue(jiraURL,scmVars.ISSUE_ID,"customfield_10007")
          File bddFile = new File("${WORKSPACE}/features/${scmVars.ISSUE_ID}.feature")
          bddFile.write jiraFields.customfield_10007
        }
      }
    }
    stage('DotNet Restore') {
      steps {
        sh "dotnet restore"
      }
    }

    stage('DotNet Build') {
      steps {
        sh "dotnet build"
      }
    }


    stage('Static code analysis (Sonarqube)') {
      environment {
        scannerHome = tool 'sonar-scanner'
      }
      steps {
        withSonarQubeEnv('sonar') {
          sh "${scannerHome}/bin/sonar-scanner"
        }
        timeout(time: 2, unit: 'MINUTES') {
          waitForQualityGate abortPipeline: true
        }
      }
    }


    stage('DotNet XUnit Tests') {
      steps {
        script {
          try {
            sh "dotnet test"
          } catch (e) {
            if (scmVars.ACTION == "PR-TEST") {
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

    stage('DotNet Publish') {
      steps {
        sh "dotnet publish -c Release"
      }
    }

    stage('Docker Build') {
      steps {
        sh "docker -H docker:2375 build -t ${dockerRegistry}/mvs/${scmVars.PROJ_NAME}:${env.BUILD_NUMBER} ."
      }
    }

    stage('Docker Publish') {
      steps {
        sh "docker -H docker:2375 login -u jenkins -p Renegade187! ${dockerRegistry}"
        sh "docker -H docker:2375 push ${dockerRegistry}/mvs/${scmVars.PROJ_NAME}:${env.BUILD_NUMBER}"
      }
    }

    stage('Docker Deploy') {
      steps {
        script {
          sh "docker -H docker:2375 run -d -p 8080 --name ${scmVars.PROJ_NAME}-${scmVars.ISSUE_ID} ${dockerRegistry}/mvs/${scmVars.PROJ_NAME}:${env.BUILD_NUMBER}"
          DOCKER_PORT = sh(returnStdout: true, script: "docker -H docker:2375 port ${scmVars.PROJ_NAME}-${scmVars.ISSUE_ID} 8080 |sed 's/.*0.0.0.0://g' | tr -d ' ' ").replaceAll(' ', '')
        }
      }
    }

    stage('Functional Test') {
      steps {
        script {
          sh "curl http://docker:${DOCKER_PORT}/api/v1/Applications/10%2F50309612-09/Mortgage/status?intermediaryId=TJLP39928"
        }
      }
    }

    stage('Keyword Actions') {
      when {
        expression {
          !env.CHANGE_URL
        }
      }
      steps {
        script {
          if (env.GIT_BRANCH.contains('RELEASE')) {
            if (scmVars.ACTION == 'PR') {
              transitionIssue(jiraURL, scmVars.ISSUE_ID, "Merged to Parent")
              updateIssue(jiraURL, scmVars.ISSUE_ID, '{"description":"' + scmVars.COMMIT_MSG + '"}')
              sh "docker -H docker:2375 rm -f ${scmVars.PROJ_NAME}-${scmVars.ISSUE_ID}-PR"
            }

            if (scmVars.ACTION == 'PR-TEST' && testFail) {
              transitionIssue(jiraURL, scmVars.ISSUE_ID, "Tests Defined")
              updateIssue(jiraURL, scmVars.ISSUE_ID, '{"description":"' + scmVars.COMMIT_MSG + '"}')
            }
          } else {
            if (scmVars.ACTION == 'PR') {
              transitionIssue(jiraURL, scmVars.ISSUE_ID, "Peer Review")
              updateIssue(jiraURL, scmVars.ISSUE_ID, '{"description":"' + scmVars.COMMIT_MSG + '"}')
              sh "docker -H docker:2375 run -d -p 8080 --name ${scmVars.PROJ_NAME}-${scmVars.ISSUE_ID}-PR ${dockerRegistry}/mvs/${scmVars.PROJ_NAME}:${env.BUILD_NUMBER}"
              PR_DOCKER_PORT = sh(returnStdout: true, script: "docker -H docker:2375 port ${scmVars.PROJ_NAME}-${scmVars.ISSUE_ID}-PR 8080 |sed 's/.*0.0.0.0://g' | tr -d ' ' ").replaceAll(' ', '')
              createGitHubPR(gitHubURL, "development", scmVars.PROJ_NAME, "terrencehatchet", "RELEASE-19.05", env.GIT_BRANCH, scmVars.ISSUE_ID, "Development PR for " + jiraURL + "/browse/" + scmVars.ISSUE_ID + " Available on: http://docker:" + PR_DOCKER_PORT)
            }

            if (scmVars.ACTION == 'PR-TEST' && testFail) {
              transitionIssue(jiraURL, scmVars.ISSUE_ID, "Tests Peer Review")
              updateIssue(jiraURL, scmVars.ISSUE_ID, '{"description":"' + scmVars.COMMIT_MSG + '"}')
              //need to get parent branch in SCMVARS
              createGitHubPR(gitHubURL, "test", scmVars.PROJ_NAME, "terrencehatchet", "RELEASE-19.05", env.GIT_BRANCH, scmVars.ISSUE_ID, "TEST PR for " + jiraURL + "/browse/" + scmVars.ISSUE_ID)
            }
            if (scmVars.ACTION == 'PR-TEST' && !testFail) {
              currentBuild.result = 'FAILURE'
              echo "tests should fail during this run but didn't"
              sh "exit 1"
            }
          }
        }
      }
    }

    stage('Publish Artefact') {
      when {
        branch "master"
      }
      steps {
        script {
          buildDotNetDeb(scmVars.PROJ_NAME, "0.1", nexusURL, "ubuntu.16.04-x64", "netcoreapp2.2")
          buildDotNetRPM(scmVars.PROJ_NAME, "0.1", nexusURL, "ubuntu.16.04-x64", "netcoreapp2.2")
          publishTarball(scmVars.PROJ_NAME, "0.1", nexusURL)
          def mergedIssues = getAllMergedIssues(scmVars.COMMIT_HASH)
          def mergedIssueSize = mergedIssues.size()
          mergedIssueSize.times {
            transitionIssue(jiraURL, mergedIssues[it], "Done")
          }
        }
      }
    }
  }
  post {
    always {
      script {
        sh "docker -H docker:2375 rm -f ${scmVars.PROJ_NAME}-${scmVars.ISSUE_ID}"
      }
    }
  }


}
