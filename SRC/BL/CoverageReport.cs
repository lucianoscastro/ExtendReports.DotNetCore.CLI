using Palmmedia.ReportGenerator.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExtendReports.DotNetCore.CLI.BL
{
    public class CoverageReport
    {
        public bool GenerateCoverageReport(string coverageFile, string diretoryOutputCoverage)
        {
            var args = new Dictionary<string, string>
            {
                { "reports", coverageFile },
                { "targetdir", diretoryOutputCoverage.Replace(@"""", string.Empty) },
                { "reporttypes", "HTMLSummary;Badges;HtmlInline" },
                { "plugins", "" }
            };

            var reportConfigurationBuilder = new ReportConfigurationBuilder();
            var configuration = reportConfigurationBuilder.Create(args);

            return new Generator().GenerateReport(configuration);

        }
    }
}
