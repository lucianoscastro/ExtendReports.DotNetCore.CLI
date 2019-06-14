# ExtendReports.DotNetCore.CLI

Projeto para desenvolver um dotnet tool com objetivo de gerar relatórios da execução detestes unitários e de comportamento. Para gerar os relatórios o aplicativo utiliza o arquivo TRX e o XML gerados como output dos testes, com base nos arquivos TRX e COVERAGE, gerar relatórios com os testes unitários e de comportamento


Primeiro você deve executar os testes xUnit com o seguinte comando:
```
dotnet test --logger "trx;LogFileName=TestResults.trx" --results-directory {path_to_output_test_result} /p:CollectCoverage=true  /p:CoverletOutput={path_to_output_test_result}\  /p:CoverletOutputFormat=cobertura  /p:Exclude="[xunit.*]*
```

Depois você deve executar o gerador de relatório com o seguinte comando:
```
dotnet testReport -s {path_to_output_test_result}  -o {path_to_output_html_report}
```


