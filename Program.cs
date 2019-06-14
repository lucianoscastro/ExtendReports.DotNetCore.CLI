using Microsoft.Extensions.CommandLineUtils;
using ExtendReports.DotNetCore.CLI.BL;
using System;

namespace ExtendReports.DotNetCore.CLI
{
    internal class Program
    {

        static void Main(string[] args)
        {
            var cli = new CommandLineApplication(false);

            cli.HelpOption("-? | -h | --help");
            var operationReportName = cli.Option("-r", "Nome para identificar o relatório que será gerado", CommandOptionType.SingleValue);
            var operationDiretoryTRX = cli.Option("-s", "Diretório com os arquivos TRX e COBERTURA.XML para gerar o relatório", CommandOptionType.SingleValue);
            var operationDiretoryOutPut = cli.Option("-o", "Diretório onde o relatório será gerado", CommandOptionType.SingleValue);
             var operationMongoDBServer = cli.Option("-m", "Endereço do servidor mongoDB para armazenar informações exemplo  mongodb://127.0.0.1:27017", CommandOptionType.SingleValue);
            var operationklovServer = cli.Option("-k", "Endereço do servidor klov de visualização de dados exemplo http://127.0.0.1:2510", CommandOptionType.SingleValue);


            cli.OnExecute(() =>
            {
                try
                {
                    var p = new CLIReportxUnit(operationReportName.Value(), operationDiretoryTRX.Value(), operationDiretoryOutPut.Value(), operationMongoDBServer.Value(), operationklovServer.Value());
                    p.GenerateReport();
                }
                catch (System.Exception e)
                {
                    Console.Write(e.Message);
                }
                return 0;
            });

            cli.Execute(args);
        }

    }
}





