using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Xml.Serialization;
namespace ExtendReports.DotNetCore.CLI.DTO
{
    [Serializable()]
    [XmlRoot("TestRun", Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
    public partial class TestResultContentFile
    {
        [XmlArrayItem("UnitTestResult", IsNullable = false)]
        public List<TestRunUnitTestResult> Results { get; set; }


        [XmlArrayItem("UnitTest", IsNullable = false)]
        public List<TestRunUnitTest> TestDefinitions { get; set; }
    }

    [Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
    public partial class TestRunUnitTestResult
    {
        public TestRunUnitTestResultOutput Output { get; set; }
        [XmlAttribute("executionId")]
        public string ExecutionId { get; set; }
        [XmlAttribute("testName")]
        public string TestName { get; set; }
        [XmlAttribute("startTime")]
        public System.DateTime StartTime { get; set; }
        [XmlAttribute("endTime")]
        public System.DateTime EndTime { get; set; }
    }
    [Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
    public partial class TestRunUnitTestResultOutput
    {
        public string StdOut { get; set; }
        public TestRunUnitTestResultOutputErrorInfo ErrorInfo { get; set; }
    }
    [System.Serializable()]
    [System.ComponentModel.DesignerCategory("code")]
    [System.Xml.Serialization.XmlType(AnonymousType = true, Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
    public partial class TestRunUnitTestResultOutputErrorInfo
    {
        public string Message { get; set; }
        public string StackTrace { get; set; }
    }
    [Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
    public partial class TestRunUnitTest
    {
        public TestRunUnitTestExecution Execution { get; set; }
        public TestRunUnitTestTestMethod TestMethod { get; set; }
    }
    [Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
    public partial class TestRunUnitTestExecution
    {
        [XmlAttribute("id")]
        public string Id { get; set; }
    }
    [Serializable()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010")]
    public partial class TestRunUnitTestTestMethod
    {
        [XmlAttribute("className")]
        public string ClassName { get; set; }
    }
}
