using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ExtendReports.DotNetCore.CLI.BL
{
    public class FixHtmlReports
    {
        public FixHtmlReports(string direcotryBase)
        {

            _directoryBase = direcotryBase;
        }

        private readonly string _directoryBase;

        private readonly string BaseLinkCoverage = "<a id=\"nav-coverage\" class=\"dropdown-toggle\" href=\"coverage.html\"> <span class=\"icon-holder\"><i class=\"fa fa-vial\"></i> </span> <span class=\"title\">Coverage</span> </a>";
        private readonly string jqueryCN = "<script src=\"https://code.jquery.com/jquery-3.4.1.slim.min.js\"></script>";
        private readonly string fontAwesomeOld = "https://stackpath.bootstrapcdn.com/font-awesome/4.7.0/css/font-awesome.min.css";
        private readonly string fontAwesomeNew = "https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.8.2/css/all.min.css";
        private readonly string iconDashOld = "fa fa-bar-chart";
        private readonly string iconDashNew = "fa fa-tachometer-alt";
        private readonly string iframe = "<iframe src=\"coverage/index.htm\" frameborder=\"0\" style=\"overflow:hidden;height:100%;width:100%\" height=\"100%\" width=\"100%\"></iframe>";
        private readonly string jsSetHeight = "$(function(){{ $(\".{0}\").height($(\".{0}\").parent().height()); }});";

        public void FixFiles()
        {
            var fileIndex = Path.Combine(_directoryBase, "index.html");
            var fileDash = Path.Combine(_directoryBase, "dashboard.html");
            var fileCoverage = Path.Combine(_directoryBase, "coverage.html");

            GenerateCoverage(fileIndex, fileCoverage);
           // FixDashboardSize(fileDash);
            FixMenu(fileIndex);
            FixMenu(fileDash);
            FixMenu(fileCoverage);

        }


        private void GenerateCoverage(string fileIndex, string fileCoverage)
        {
            var doc = new HtmlDocument();

            //busca o arquivo index
            doc.Load(fileIndex);

            //adiciona a referencia do jquery
            var cabecalho = doc.DocumentNode.SelectSingleNode("//head");
            var nodejsJquery = HtmlNode.CreateNode(jqueryCN);
            cabecalho.AppendChild(nodejsJquery);

            //remover o conteudo principal
            var conteudo = doc.DocumentNode.SelectSingleNode("//*[contains(@class,'test-wrapper')]");
            conteudo.RemoveAllChildren();
            conteudo.InnerHtml = iframe;

            //adiciona o JS que vai ajusatar o tamanho do iframe
            var jsContent = doc.DocumentNode.SelectSingleNode("//script[contains(@type,'text/javascript')]");
            jsContent.InnerHtml = string.Format(jsSetHeight, "test-wrapper");

            //salva com o nome de coverage
            doc.Save(fileCoverage);


        }


        private void FixDashboardSize(string file)
        {
            HtmlDocument doc = new HtmlDocument();

            doc.Load(file);

            //adiciona a referencia do jquery
            var cabecalho = doc.DocumentNode.SelectSingleNode("//head");
            var nodejsJquery = HtmlNode.CreateNode(jqueryCN);
            cabecalho.AppendChild(nodejsJquery);

            //adicionar o JS que ajusta o tamanho de altura
            var jsContent = doc.DocumentNode.SelectSingleNode("//script[contains(@type,'text/javascript')]");
            jsContent.InnerHtml = string.Format(jsSetHeight, "p-4");

            doc.Save(file);

        }
        
        private void FixMenu(string file)
        {
            HtmlDocument doc = new HtmlDocument();

            var fileContent = File.ReadAllText(file);

            fileContent = fileContent.Replace(fontAwesomeOld, fontAwesomeNew).Replace(iconDashOld, iconDashNew);

            using (var stream = GenerateStreamFromString(fileContent))
            {

                doc.Load(stream);
                //remover o conteudo principal
                //doc.DocumentNode.SelectSingleNode("//*[contains(@class,'test-wrapper')]").RemoveAllChildren();

                //busca o menu para dicionar mais um item
                var menu = doc.DocumentNode.SelectSingleNode("//ul[contains(@class,'side-nav-menu')]");
                if (menu != null && menu.ChildNodes.Count > 1)
                {
                    var itemCoverage = menu.ChildNodes[1].Clone();
                    itemCoverage.InnerHtml = BaseLinkCoverage;
                    menu.ChildNodes.Append(itemCoverage);
                }

                doc.Save(file);
            }
        }

        private Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

    }
}
