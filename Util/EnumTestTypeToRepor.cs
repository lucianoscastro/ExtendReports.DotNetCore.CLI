using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ExtendReports.DotNetCore.CLI.Util
{
    public enum ExtendReportType
    {
        [Description("Todos os tipos de tests")]
        A,
        [Description("Somente testes unitários")]
        U,
        [Description("Somente testes de comportamento")]
        B

    }
}
