using AventStack.ExtentReports;
using AventStack.ExtentReports.Gherkin.Model;
using AventStack.ExtentReports.Model;
using ExtendReports.DotNetCore.CLI.DTO;
using ExtendReports.DotNetCore.CLI.Util;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Serialization;
namespace ExtendReports.DotNetCore.CLI.BL
{
    public class TrxParser
    {
        private readonly AventStack.ExtentReports.ExtentReports _extentReports;
        private readonly ILogger _logger;
        public TrxParser(AventStack.ExtentReports.ExtentReports extentReports, ILogger logger)
        {
            _extentReports = extentReports;
            _logger = logger;
        }
        public void ParseTestRunnerOutput(string trxFile)
        {
            var trxObj = GetTesteResultContentFile(trxFile);
            var unitTestsToReport = GetUnitTestesToReport(trxObj);
            var specFlowTestsToReport = GetSpecflowTestesToReport(trxObj);
            var testsToReport = unitTestsToReport.Concat(specFlowTestsToReport).ToList();
            _logger.LogInformation($"Total de testes unitários reportados: {unitTestsToReport.Count()}");
            _logger.LogInformation($"Total de testes comportamentos reportados: {specFlowTestsToReport.Count()}");
            testsToReport.ForEach(test =>
            {
                var feature = _extentReports.CreateTest<Feature>(test.Feature);
                SetStartEndTime(feature.Model, test.Scenario.SelectMany(x => x.Steps).Min(x => x.StartTime), test.Scenario.SelectMany(x => x.Steps).Max(x => x.EndTime));
                _logger.LogInformation($"Processando a funcionalidade: {feature.Model.Name}");
                test.Scenario.ForEach(sc =>
                {
                    var scenario = feature.CreateNode<Scenario>(sc.Name);
                    SetStartEndTime(scenario.Model, sc.Steps.Min(x => x.StartTime), sc.Steps.Max(x => x.EndTime));
                    _logger.LogInformation($"Processando o cenário: {scenario.Model.Name}");
                    sc.Steps.ForEach(s =>
                    {
                        var step = scenario.CreateNode(s.Keyword, s.Name, s.Description);
                        if (s.Error != null)
                        {
                            var errorMessage = $"{WebUtility.HtmlEncode(s.Error.Message)}<br/><br/>{WebUtility.HtmlEncode(s.Error.StackTrace)}<br/>";
                            step.Fail(errorMessage);
                        }
                        SetStartEndTime(step.Model, s.StartTime, s.EndTime);
                        _logger.LogInformation($"Processando o passo: {step.Model.Name}");
                    });
                });
            });
        }
        private void SetStartEndTime(Test model, DateTime starttime, DateTime endtime)
        {
            model.LogContext.All().ForEach(lc =>
            {
                lc.Timestamp = endtime;
            });
            model.StartTime = starttime;
            model.EndTime = endtime;
        }
        private TestResultContentFile GetTesteResultContentFile(string trxFile)
        {
            TestResultContentFile trxObj = null;
            var fileContent = File.ReadAllText(trxFile);
            using (var stringReader = new StringReader(fileContent))
            {
                var serializer = new XmlSerializer(typeof(TestResultContentFile));
                trxObj = (TestResultContentFile)serializer.Deserialize(stringReader);
            }
            return trxObj;
        }
        private void AddTestToReport(bool unitTest, string featureName, string scenarioName, List<TestStepDTO> steps, List<TestInformationReportDTO> testList)
        {
            TestInformationReportDTO testFeature = null;
            TestScenarioDTO scenario = null;
            testFeature = testList.FirstOrDefault(x => x.Feature.ToUpper().Equals(featureName.ToUpper()));
            if (unitTest && (testFeature != null))
            {
                scenario = testFeature.Scenario.FirstOrDefault(x => x.Name.ToUpper().Equals(scenarioName.ToUpper()));
            }
            if (testFeature == null)
            {
                testFeature = new TestInformationReportDTO { Feature = featureName };
                testList.Add(testFeature);
            }
            if (!unitTest || scenario == null)
            {
                scenario = new TestScenarioDTO { Name = scenarioName };
                testFeature.Scenario.Add(scenario);
            }
            scenario.Steps.AddRange(steps);
        }

        private string GetSpecScenarioName(string testeName)
        {
            var firstParenteseIndex = testeName.IndexOf('(');
            var finishInParenteses = testeName.EndsWith(')');
            var name = testeName.Split('(')[0];
            //se tem ( e termina em ) significa que é um scenario outline
            if (firstParenteseIndex >= 0 && finishInParenteses)
            {
                var options = testeName.Substring(firstParenteseIndex + 1, testeName.Length - firstParenteseIndex - 2);
                var objetivoParameter = options.Split(",").Where(x => x.ToUpper().Trim().StartsWith("OBJETIVO: ")).FirstOrDefault();
                if (objetivoParameter != null)
                    name = $"{name} - {objetivoParameter.Split(":")[1].Trim()}";
            }
            return name;
        }
        private string GetSpecFeatureName(string featureName)
        {
            return featureName.Substring(featureName.LastIndexOf(".") + 1).Replace("Feature", string.Empty).Replace("_", " ");
        }



        private string GetStepTypeWord(string currentStep, string lastStep)
        {
            //se o step atual é um AND, deve trocar para o ultimo sep tipado (GIVEN, WHEN, THEN)
            if ((currentStep.ToUpper() == ExtendReportConsts.AND) || (currentStep.ToUpper() == ExtendReportConsts.E))
                return lastStep;
            else  //caso o step atual não seja um AND então armazena para usar
                return currentStep;
        }

        private GherkinKeyword GetStepGherkinKeyword(string stepTyeWord)
        {
            var step = stepTyeWord.ToUpper().Replace("DADO", "GIVEN").Replace("QUANDO", "WHEN").Replace("ENTÃO", "THEN");
            return new GherkinKeyword(step);
        }

        private TestStepErrorDTO GetErrorInStep(TestRunUnitTestResult test, string stepResults = " ")
        {
            TestStepErrorDTO erro = null;
            var resultStepIsError = (!string.IsNullOrEmpty(stepResults)) && (stepResults.IndexOf(ExtendReportConsts.Error) >= 0);
            var outPutStepIsError = test.Output != null && test.Output.ErrorInfo != null;
            if (resultStepIsError)
                erro = new TestStepErrorDTO() { Message = stepResults.Replace(ExtendReportConsts.Error, string.Empty) };
            if (outPutStepIsError)
            {
                if (erro == null)
                    erro = new TestStepErrorDTO() { Message = test.Output.ErrorInfo.Message, StackTrace = test.Output.ErrorInfo.StackTrace };
                else
                    erro.StackTrace = test.Output.ErrorInfo.StackTrace;
            }
            return erro;
        }
        private List<TestInformationReportDTO> GetUnitTestesToReport(TestResultContentFile trxInformation)
        {
            var testList = new List<TestInformationReportDTO>();
            if (trxInformation != null)
            {
                var unitTests = trxInformation.Results.Where(x => x.Output == null || (x.Output.StdOut == null)).ToList();
                unitTests.ForEach(t =>
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(t.TestName))
                        {
                            var splitedTestName = t.TestName.Split(".");
                            var stepName = splitedTestName[splitedTestName.Length - 1];
                            var scenarioName = splitedTestName[splitedTestName.Length - 2];
                            var featureName = t.TestName.Replace($".{scenarioName}.{stepName}", string.Empty);
                            var step = new TestStepDTO()
                            {
                                Name = stepName,
                                Description = stepName,
                                EndTime = t.EndTime,
                                StartTime = t.StartTime,
                                Error = GetErrorInStep(t)
                            };
                            AddTestToReport(true, featureName, scenarioName, new List<TestStepDTO> { step }, testList);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, $"Erro ao processar o teste UNITÁRIO {t.TestName}");
                    }
                });
            }
            return testList;
        }
        private List<TestInformationReportDTO> GetSpecflowTestesToReport(TestResultContentFile trxObj)
        {
            var testList = new List<TestInformationReportDTO>();
            if (trxObj != null)
            {
                var specFlowTest = trxObj.Results.Where(x => x.Output != null && !string.IsNullOrEmpty(x.Output.StdOut) && (x.Output.StdOut.IndexOf(ExtendReportConsts.Given) == 0 || x.Output.StdOut.IndexOf(ExtendReportConsts.Dado) == 0)).ToList();
                specFlowTest.ForEach(t =>
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(t.TestName))
                        {
                            var scenarioName = GetSpecScenarioName(t.TestName);
                            var executationId = t.ExecutionId;
                            var testDefinition = trxObj.TestDefinitions.Where(x => x.Execution.Id == executationId).FirstOrDefault();
                            if (testDefinition != null)
                            {
                                var featureName = GetSpecFeatureName(testDefinition.TestMethod.ClassName);
                                var listaSteps = new List<TestStepDTO>();
                                var rawStdOutRows = t.Output.StdOut.Split(Environment.NewLine);
                                var lastStepType = string.Empty;
                                for (var i = 0; i < rawStdOutRows.Length; i += 2)
                                {
                                    var stepInfo = rawStdOutRows[i];
                                    var stepResult = rawStdOutRows[i + 1];
                                    var stepName = stepInfo;
                                    var currentStepType = stepInfo.Split(" ")[0];

                                    var stepTypeToUse = GetStepTypeWord(currentStepType, lastStepType);
                                    lastStepType = stepTypeToUse;
                                    var gherkinStep = GetStepGherkinKeyword(stepTypeToUse);
                                    var step = new TestStepDTO(gherkinStep)
                                    {
                                        Name = stepName,
                                        Description = stepName,
                                        EndTime = t.EndTime,
                                        StartTime = t.StartTime,
                                        Error = GetErrorInStep(t, stepResult)
                                    };
                                    listaSteps.Add(step);
                                }
                                AddTestToReport(false, featureName, scenarioName, listaSteps, testList);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, $"Erro ao processar o teste SPEC {t.TestName}");
                    }
                });
            }
            return testList;
        }
    }
}
