pipeline {
    agent any
    stages {
        stage ('Compile') {
            steps {
                echo "***** Building *****"
                sh "dotnet build endgame-poc.sln --source https://api.nuget.org/v3/index.json"
            }
        }
    }
}