cd src\Lambda\BackendFunction

echo DotNet restore 
dotnet restore

echo DotNet install AWS Tools
dotnet tool install -g Amazon.Lambda.Tools --framework net6.0

echo DotNet Build
dotnet lambda package --configuration Release --framework net6.0 --output-package ../../dist/backendFunction.zip

echo Deploying via CDK
cd ../../../
cdk deploy --require-approval never

echo Solution Deployed
pause