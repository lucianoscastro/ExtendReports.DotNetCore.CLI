using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ExtendReports.DotNetCore.CLI.Util
{
    public enum ReportType
    {
        [Description("Relatório de arquivos TRX")]
        TRX,

        [Description("Relatório de arquivos COVERAGE")]
        COVERAGE
        

    }
}
