using AventStack.ExtentReports.Reporter;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Palmmedia.ReportGenerator.Core;
using ExtendReports.DotNetCore.CLI.Util;
using EnumsNET;
using System.Text;
using Palmmedia.ReportGenerator.Core.Parser;

namespace ExtendReports.DotNetCore.CLI.BL
{
    public class CLIReportxUnit : IDisposable
    {
        public CLIReportxUnit(string reportName, string directoryLogs, string directoryOutput, string mongoDbServer, string klovServer)
        {
            _reportName = reportName;
            _diretoryOutput = directoryOutput;
            _directoryLogs = directoryLogs;

            _mongoDBServer = mongoDbServer;
            _klovServer = klovServer;


            _reportName = string.IsNullOrEmpty(_reportName) ? "ExtendReports.DotNetCore.CLI" : _reportName;
            _directoryLogs = string.IsNullOrEmpty(_directoryLogs) ? Path.GetFullPath("ExtendReports.DotNetCore.CLI") : _directoryLogs;
            _diretoryOutput = string.IsNullOrEmpty(_diretoryOutput) ? Path.GetFullPath("ExtendReports.DotNetCore.CLI") : _diretoryOutput;



            _diretoryOutputCoverage = Path.Combine(_diretoryOutput, "coverage");


            ConfigLog();
            ConfigHtml();
            ConfigKlov();
            ConfigReport();
        }

        public CLIReportxUnit() : this(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty)
        {

        }

        private readonly string trxFilePattern = "*.trx";
        private readonly string coverageFilePattern = "*.cobertura.xml";

        private ILogger _logger;

        private ExtentHtmlReporter _htmlReporter;
        private ExtentKlovReporter _klovReporter;
        private AventStack.ExtentReports.ExtentReports _extentReports;

        private readonly string _reportName;
        private readonly string _directoryLogs;
        private readonly string _diretoryOutput;
        private readonly string _diretoryOutputCoverage;

        private readonly string _mongoDBServer;
        private readonly string _klovServer;

        public void GenerateReport()
        {
            AddSystemInformation();

            ProcessFileType(ReportType.COVERAGE);

            ProcessFileType(ReportType.TRX);

            FixFiles();

        }

     

        private void ProcessFileType(ReportType reportType)
        {
            var reportTypeName = Enums.GetName<ReportType>(reportType);

            _logger.LogInformation($"Iniciando o processamento dos arquivos do tipo: {reportTypeName} ");
            var searchPattern = reportType == ReportType.TRX ? trxFilePattern : coverageFilePattern;
            var filesToProcess = Directory.Exists(_directoryLogs) ? Directory.GetFiles(_directoryLogs, searchPattern, SearchOption.TopDirectoryOnly).ToList() : new List<string>();
            _logger.LogInformation($"Encontrado um total de {filesToProcess.Count} no diretório {_directoryLogs}, procurando por aquivos com a extensão {searchPattern}");

            filesToProcess.ForEach(file =>
            {
                _logger.LogInformation($"Iniciando o processamento do arquivo {file}");
                try
                {
                    if (reportType == ReportType.TRX)
                        ProcessTrxFile(file);
                    else
                        ProcessCoverageFile(file);

                    _logger.LogInformation($"Arquivo {file} processado com sucesso");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Erro ao processar o arquivo {file}");
                }

            });

            if (filesToProcess.Count > 0)
                _logger.LogInformation($"Um total de {filesToProcess.Count} arquivos do tipo {reportTypeName} processados e o elatório foi gerado no diretório {_diretoryOutput}");
            else
                _logger.LogInformation($"Nenhum arqvuiso do tipo {reportTypeName} processados");

        }



        private void ProcessCoverageFile(string coverageFile)
        {
            new CoverageReport().GenerateCoverageReport(coverageFile, _diretoryOutputCoverage);
        }


        private void ProcessTrxFile(string trxFile)
        {

            new TrxParser(_extentReports, _logger).ParseTestRunnerOutput(trxFile);
            _extentReports.Flush();
        }

        private void FixFiles()
        {
            new FixHtmlReports(_diretoryOutput).FixFiles();
        }

        private void AddSystemInformation()
        {
            _extentReports.AddSystemInfo("OS Version", Environment.OSVersion.ToString());
            _extentReports.AddSystemInfo("CLR Version", Environment.Version.ToString());
            _extentReports.AddSystemInfo("Machine Name", Environment.MachineName.ToString());
            _extentReports.AddSystemInfo("User", Environment.UserName);
            _extentReports.AddSystemInfo("User Domain", Environment.UserDomainName);
        }


        private void ConfigKlov()
        {

            if (!string.IsNullOrEmpty(_mongoDBServer) || !string.IsNullOrEmpty(_klovServer))
            {
                _klovReporter = new ExtentKlovReporter
                {
                    ProjectName = GetReportName(),
                    ReportName = GetReportName()
                };
            }

            if (!string.IsNullOrEmpty(_mongoDBServer))
                _klovReporter.InitMongoDbConnection(_mongoDBServer);

            if (!string.IsNullOrEmpty(_klovServer))
                _klovReporter.InitKlovServerConnection(_klovServer);

        }


        private void ConfigHtml()
        {
            var finalPath = Path.GetFullPath(_diretoryOutput);
            finalPath = finalPath.EndsWith("\\") || finalPath.EndsWith("/") ? finalPath : finalPath + "\\";

            _htmlReporter = new ExtentHtmlReporter(finalPath);
            _htmlReporter.Config.Theme = AventStack.ExtentReports.Reporter.Configuration.Theme.Dark;
            _htmlReporter.Config.EnableTimeline = true;
            _htmlReporter.Config.DocumentTitle = GetReportName();
        }

        private string GetReportName()
        {
            return $"{_reportName}";
        }

        private void ConfigLog()
        {
            using (var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole().AddDebug()))
            {
                _logger = loggerFactory.CreateLogger<Program>();
            }
        }

        private void ConfigReport()
        {
            _extentReports = new AventStack.ExtentReports.ExtentReports();

            if (_htmlReporter != null)
                _extentReports.AttachReporter(_htmlReporter);

            if (_klovReporter != null)
                _extentReports.AttachReporter(_klovReporter);
        }


        public void Dispose()
        {
            _extentReports = null;
            _logger = null;
            _htmlReporter = null;
            _klovReporter = null;
        }

    }
}
