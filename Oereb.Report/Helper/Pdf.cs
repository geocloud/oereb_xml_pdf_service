using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using GhostscriptSharp;
using iTextSharp.text.pdf;
using iTextSharp.text;

namespace Oereb.Report.Helper
{
    public static class Pdf
    {
        public static bool GetFromUrl(string url, string file)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            request.Method = "GET";
            request.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-gb,en;q=0.5");
            request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");

            request.Accept = "application/pdf";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:12.0) Gecko/20100101 Firefox/12.0";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            try
            {
                using (var stream = File.Create(file))
                {
                    response.GetResponseStream().CopyTo(stream);
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public static byte[] GetFromUrl(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            request.Method = "GET";
            request.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-gb,en;q=0.5");
            request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");

            request.Accept = "application/pdf";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:12.0) Gecko/20100101 Firefox/12.0";

            var response = (HttpWebResponse)request.GetResponse();

            if (response.GetResponseStream() == null)
            {
                return null;
            }

            using (var stream = new MemoryStream())
            {
                response.GetResponseStream().CopyTo(stream);
                return stream.ToArray();
            }
        }

        public static List<byte[]> GetImagesFromPpdf(string file)
        {
            var files = new List<byte[]>();

            if (!File.Exists(file))
            {
                return files;
            }

            var pdfReader = new PdfReader(file);
            var pagecount = pdfReader.NumberOfPages;

            var imagePath = new FileInfo(file);
            var multipleFiles = Path.Combine(imagePath.Directory.FullName, "output%d.jpg");

            GhostscriptWrapper.GeneratePageThumbs(
                file,
                multipleFiles, 
                1,
                pagecount, 
                200, 
                200
            );

            for (var i = 1; i <= pagecount; i++)
            {
                var filename = Path.Combine(imagePath.Directory.FullName, $"output{i}.jpg");

                if (!File.Exists(filename))
                {
                    continue;
                }

                var image = System.Drawing.Image.FromFile(filename);
                byte[] array;

                using (MemoryStream ms = new MemoryStream())
                {
                    image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    array = ms.ToArray();
                }

                files.Add(array);
            }

            return files;
        }

        public static void AddAttachments(string fileSource, string fileOutput, Dictionary<string,string> packageitems)
        {
            try
            {
                PdfReader reader = new PdfReader(fileSource);
                FileStream outputstream = new FileStream(fileOutput, FileMode.Create);

                PdfStamper stamp = new PdfStamper(reader, outputstream);

                stamp.Writer.ViewerPreferences = PdfWriter.FitWindow | PdfWriter.HideToolbar | PdfWriter.PageModeUseAttachments | PdfWriter.PageLayoutSinglePage;

                PdfWriter attachment = stamp.Writer;

                foreach (var packageItem in packageitems)
                {
                    var filename = $"{packageItem.Key}.pdf".Replace("(siehe im PDF Anhang)",""); //todo remove
                    PdfFileSpecification pdfAttachent = PdfFileSpecification.FileEmbedded(attachment, packageItem.Value, filename, null, "application/pdf",new PdfDictionary() {}, 0 );
                    stamp.AddFileAttachment(packageItem.Key, pdfAttachent);
                }

                stamp.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static byte[] AddAttachments(byte[] xmlContent, Dictionary<string, byte[]> packageitems)
        {
            using (var outputstream = new MemoryStream())
            {
                try
                {
                    PdfReader reader = new PdfReader(xmlContent);

                    PdfStamper stamp = new PdfStamper(reader, outputstream);

                    stamp.Writer.ViewerPreferences = PdfWriter.FitWindow | PdfWriter.HideToolbar |
                                                     PdfWriter.PageModeUseAttachments | PdfWriter.PageLayoutSinglePage;

                    PdfWriter attachment = stamp.Writer;

                    foreach (var packageItem in packageitems)
                    {
                        PdfFileSpecification pdfAttachent = PdfFileSpecification.FileEmbedded(attachment, null,
                            packageItem.Key, packageItem.Value);
                        stamp.AddFileAttachment(packageItem.Key, pdfAttachent);
                    }

                    stamp.Close();
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                return outputstream.ToArray();
            }
        }

        public static void FromPdf(string originalPdf, string insertPdf, int page, string outputPdf)
        {
            //using (var sourceDocumentStream1 = new FileStream(originalPdf, FileMode.Open))
            //{
            //    using (var sourceDocumentStream2 = new FileStream(insertPdf, FileMode.Open))
            //    {
            //        using (var destinationDocumentStream = new FileStream(outputPdf, FileMode.Create))
            //        {
            //            var pdfConcat = new PdfConcatenate(destinationDocumentStream);
            //            var pdfReader = new PdfReader(sourceDocumentStream1);

            //            var pages = new List<int>();
            //            for (int i = 0; i < pdfReader.NumberOfPages; i++)
            //            {
            //                pages.Add(i);
            //            }

            //            pdfReader.SelectPages(pages);
            //            pdfConcat.AddPages(pdfReader);

            //            pdfReader = new PdfReader(sourceDocumentStream2);

            //            pages = new List<int>();
            //            for (int i = 0; i < pdfReader.NumberOfPages; i++)
            //            {
            //                pages.Add(i);
            //            }

            //            pdfReader.SelectPages(pages);
            //            pdfConcat.AddPages(pdfReader);

            //            pdfReader.Close();
            //            pdfConcat.Close();
            //        }
            //    }
            //}
        }
    }
}
