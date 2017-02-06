using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text.pdf;
using Telerik.Reporting;
using Telerik.Reporting.Processing;

namespace Oereb.Report.Helper
{
    public static class Report
    {
        public static int GetPageCountFromReport(Telerik.Reporting.Report report)
        {
            var reportProcessor = new ReportProcessor();
            var deviceInfo = new System.Collections.Hashtable();
            var instanceReportSource = new InstanceReportSource();

            instanceReportSource.ReportDocument = report;

            var result = reportProcessor.RenderReport("PDF", instanceReportSource, deviceInfo);

            var pdfReader = new PdfReader(result.DocumentBytes);
            return pdfReader.NumberOfPages;
        }

        public static void AddBookmark(Telerik.Reporting.Report report, string value)
        {
            report.BookmarkId = value;
            report.DocumentMapText = value;
            report.TocText = value;
        }
    }
}
