using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Xml.Linq;
using log4net;
using Oereb.Service.Helper;

namespace Oereb.Service.Controllers
{    
    public class ReportController : ApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// entrypoint of webservice, send a post with a valid oereb xml extract 
        /// </summary>
        /// <param name="flavour">complete |completeAttached | reduced</param>
        /// <param name="validate">per default is the xml validating</param>
        /// <param name="usewms">force using wms</param>
        /// <returns></returns>
        [HttpPost]
        public HttpResponseMessage Create([FromUri] string flavour = "reduced", [FromUri] bool validate = true,[FromUri] bool usewms = false)
        {
            var httpRequest = HttpContext.Current.Request;
            bool validToken = false;
            var logFilesXml2Pdf = ConfigurationManager.AppSettings["logFilesXml2Pdf"] == "true";
            var token = "";

            if (httpRequest.Form.AllKeys.Contains("token"))
            {
                if (DataContracts.Settings.AccessTokens.ContainsKey(httpRequest.Form["token"]))
                {
                    token = httpRequest.Form["token"];
                    validToken = true;
                }
            }
            else if (httpRequest.Headers.AllKeys.Contains("token"))
            {
                if (DataContracts.Settings.AccessTokens.ContainsKey(httpRequest.Headers["token"]))
                {
                    token = httpRequest.Headers["token"];
                    validToken = true;
                }
            }

            if (!validToken)
            {
                return this.Request.CreateResponse
                (
                    HttpStatusCode.BadRequest,
                    new
                    {
                        Status = "Submitted token is not valid or does not exist"
                    }
                );
            }

            if (httpRequest.Form.AllKeys.Contains("flavour"))
            {
                flavour = httpRequest.Form["flavour"];
            }

            if (httpRequest.Form.AllKeys.Contains("validate"))
            {
                validate = Convert.ToBoolean(httpRequest.Form["validate"]);
            }

            if (httpRequest.Form.AllKeys.Contains("usewms"))
            {
                usewms = Convert.ToBoolean(httpRequest.Form["usewms"]);
            }

            if (httpRequest.Files.Count != 1)
            {
                return this.Request.CreateResponse
                (
                    HttpStatusCode.BadRequest,
                    new
                    {
                        Status = "Only upload from one file allowed"
                    }
                );
            }

            var validflavours = new string[] { "complete", "completeAttached", "reduced" };

            if (!validflavours.Contains(flavour))
            {
                return this.Request.CreateResponse
                (
                    HttpStatusCode.BadRequest,
                    new
                    {
                        Status = "Bad flavour, valid values are complete | completeAttached | reduced"
                    }
                );
            }

            byte[] postedFile;

            using (var binaryReader = new BinaryReader(httpRequest.Files[0].InputStream))
            {
                postedFile = binaryReader.ReadBytes(httpRequest.Files[0].ContentLength);
            }

            string xmlContentOri = System.Text.Encoding.UTF8.GetString(postedFile);
            string xmlContent = System.Text.Encoding.UTF8.GetString(postedFile);

            //remove BOM if available

            if (xmlContent[0] == 65279)
            {
                xmlContent = xmlContent.Substring(1);
            }

            if (string.IsNullOrEmpty(xmlContent))
            {
                return this.Request.CreateResponse
                (
                    HttpStatusCode.BadRequest,
                    new
                    {
                        Status = "Posted content is empty"
                    }
                );
            }

            if (xmlContent.Contains("GetExtractByIdResponse"))
            {
                var document = XElement.Parse(xmlContent);

                var reader = document.CreateReader();
                reader.MoveToContent();
                xmlContent = reader.ReadInnerXml();
            }

            var outGuid = Guid.NewGuid();
            var pathLogFiles = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "LogFilesXml2Pdf");

            if (logFilesXml2Pdf && !Directory.Exists(pathLogFiles))
            {
                Directory.CreateDirectory(pathLogFiles);
            }

            if (logFilesXml2Pdf)
            {
                File.WriteAllText(Path.Combine(pathLogFiles, $"{token}_{outGuid}.xml"), xmlContentOri, Encoding.UTF8);
            }

            if (validate)
            {
                var binPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);

                if (binPath.StartsWith(@"file:\"))
                {
                    binPath = binPath.Replace(@"file:\", "");
                }

                var schemapath = System.IO.Path.Combine(new DirectoryInfo(binPath).Parent.FullName, "Checkfiles/schema_1.0/");

                var xmlValidation = new XmlValidation();
                var statusValidation = xmlValidation.Validate(xmlContent, $"file:/{schemapath}".Replace(@"\", "/").Replace("/", "//"));

                if (!statusValidation)
                {
                    if (logFilesXml2Pdf)
                    {
                        File.WriteAllText(Path.Combine(pathLogFiles, $"{token}_{outGuid}.xml"), "error validation: "+ xmlValidation.Messages.Aggregate((i, j) => i + "," + j), Encoding.UTF8);
                    }

                    return this.Request.CreateResponse
                    (
                        HttpStatusCode.BadRequest,
                        new
                        {
                            Status = $"Xml is not valid: {outGuid}",
                            Errors = xmlValidation.Messages
                        }
                   );
                }
            }

            var complete = false;
            var attached = false;

            if (flavour == "complete")
            {
                complete = true;
            }
            else if (flavour == "completeAttached")
            {
                complete = true;
                attached = true;
            }

            byte[] content;

            try
            {
                content = Report.ReportBuilder.GeneratePdf(xmlContent.TrimStart(), complete, attached, usewms);
            }
            catch (Exception ex)
            {
                Log.Error($"error reporting {ex.Message}");

                if (logFilesXml2Pdf)
                {
                    File.WriteAllText(Path.Combine(pathLogFiles, $"{token}_{outGuid}.txt"), ex.ToString(), Encoding.UTF8);
                }

                return this.Request.CreateResponse
                (
                    HttpStatusCode.BadRequest,
                    new
                    {
                        Status = $"error reporting: {outGuid}"
                    }
                );
            }

            if (logFilesXml2Pdf)
            {
                File.WriteAllBytes(Path.Combine(pathLogFiles, $"{token}_{outGuid}.pdf"), content);
            }

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);

            var ms = new MemoryStream(content);
            response.Content = new StreamContent(ms);
            //response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
            response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("inline");

            var mimeTypeObject = new MediaTypeHeaderValue("application/pdf");

            response.Content.Headers.ContentType = mimeTypeObject;
            response.Content.Headers.ContentLength = ms.Length;
            response.Content.Headers.ContentDisposition.FileName = $"{Guid.NewGuid()}.pdf";
            return response;
        }

        [HttpGet]
        public HttpResponseMessage Form()
        {
            var binPath = Helper.PathTools.Rootpath();
            var htmlpath = System.IO.Path.Combine(new DirectoryInfo(binPath).Parent.FullName, "Content/report-form.htm");
            var htmlContent = File.ReadAllText(htmlpath);

            var response = new HttpResponseMessage();
            response.Content = new StringContent(htmlContent);
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }
    }
}
