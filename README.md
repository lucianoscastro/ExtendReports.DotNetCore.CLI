# ExtendReports.DotNetCore.CLI

Projeto para desenvolver um dotnet tool com objetivo de gerar relatórios da execução detestes unitários e de comportamento. Para gerar os relatórios o aplicativo utiliza o arquivo TRX e o XML gerados como output dos testes, com base nos arquivos TRX e COVERAGE, gerar relatórios com os testes unitários e de comportamento

```
dotnet test --logger "trx;LogFileName=TestResults.trx" --results-directory ./TestResult  /p:CollectCoverage=true  /p:CoverletOutput=./TestResult\  /p:CoverletOutputFormat=cobertura  /p:Exclude="[xunit.*]*
```

https://github.com/simplytest/extentreports-csharp
