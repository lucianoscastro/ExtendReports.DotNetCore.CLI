using AventStack.ExtentReports;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExtendReports.DotNetCore.CLI.DTO
{
    public class TestInformationReportDTO
    {
        public TestInformationReportDTO()
        {
            Scenario = new List<TestScenarioDTO>();
        }
        public string Feature { get; set; }
        public List<TestScenarioDTO> Scenario { get; set; }

    }

    public class TestScenarioDTO
    {
        public TestScenarioDTO()
        {
            Steps = new List<TestStepDTO>();
        }

        public string Name { get; set; }
        public List<TestStepDTO> Steps { get; set; }

    }

    public class TestStepDTO
    {
        public TestStepDTO() : this(new GherkinKeyword("Then"))
        {

        }
        public TestStepDTO(GherkinKeyword step)
        {
            
            this.Keyword = step; 
        }

        public GherkinKeyword Keyword { get; private set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public TestStepErrorDTO Error { get; set; }

    }

    public class TestStepErrorDTO
    {
        public string Message { get; set; }
        public string StackTrace { get; set; }
    }
}
