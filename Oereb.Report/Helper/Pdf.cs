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

        public static void AddAttachments(string fileSource, string fileOutput, List<FileContainer> fileContainers)
        {
            try
            {
                PdfReader reader = new PdfReader(fileSource);
                FileStream outputstream = new FileStream(fileOutput, FileMode.Create);

                PdfStamper stamp = new PdfStamper(reader, outputstream);

                stamp.Writer.ViewerPreferences = PdfWriter.FitWindow | PdfWriter.HideToolbar | PdfWriter.PageModeUseAttachments | PdfWriter.PageLayoutSinglePage;

                PdfWriter attachment = stamp.Writer;

                foreach (var fileContainer in fileContainers)
                {
                    var index = ".bin";

                    switch (fileContainer.ContentType.ToLower())
                    {
                        case "application/pdf":

                            index = ".pdf";
                            break;

                        case "text/html":

                            index = ".html";
                            break;
                    }

                    var filename = $"{fileContainer.Description}{index}";

                    PdfFileSpecification pdfAttachent = PdfFileSpecification.FileEmbedded(attachment, fileContainer.FilePath, filename, null, fileContainer.ContentType, new PdfDictionary() {}, 0 );
                    stamp.AddFileAttachment(fileContainer.Description, pdfAttachent);
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
