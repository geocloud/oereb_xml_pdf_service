using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;

namespace Oereb.Service.Controllers
{    
    public class ReportController : ApiController
    {
        /// <summary>
        /// entrypoint of webservice, send a post with a valid oereb xml extract 
        /// </summary>
        /// <param name="flavour">complete |completeAttached | reduced</param>
        /// <returns></returns>

        [HttpPost]
        public HttpResponseMessage GetPdfFromXml(string flavour)
        {
            var httpRequest = HttpContext.Current.Request;

            if (httpRequest.Files.Count != 1)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new StringContent("only upload from one file allowed"));
            }

            byte[] postedFile;

            using (var binaryReader = new BinaryReader(httpRequest.Files[0].InputStream))
            {
                postedFile = binaryReader.ReadBytes(httpRequest.Files[0].ContentLength);
            }

            string xmlContent = System.Text.Encoding.Unicode.GetString(postedFile);

            //remove BOM if available

            if (xmlContent[0] == 65279)
            {
                xmlContent = xmlContent.Substring(1);
            }

            if (string.IsNullOrEmpty(xmlContent))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new StringContent("posted content is empty"));
            }

            var validflavours = new string[] {"complete", "completeAttached", "reduced"};

            if (!validflavours.Contains(flavour))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new StringContent("bad flavour complete | completeAttached | reduced"));
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

            //todo validation, at this time the schema in the xml file does not exist

            var content =  Report.ReportBuilder.GeneratePdf(xmlContent.TrimStart(), complete, attached);

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);

            var ms = new MemoryStream(content);
            response.Content = new StreamContent(ms);
            response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");

            var mimeTypeObject = new MediaTypeHeaderValue("application/pdf");

            response.Content.Headers.ContentType = mimeTypeObject;
            response.Content.Headers.ContentLength = ms.Length;
            response.Content.Headers.ContentDisposition.FileName = $"{Guid.NewGuid()}.pdf";
            return response;
        }
    }
}
