using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace Oereb.Report.Helper
{
    public class Content
    {
        public static Result GetFromUrl(string url, string file)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            request.Method = "GET";
            request.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-gb,en;q=0.5");
            request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");

            request.Accept = "application/pdf";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:12.0) Gecko/20100101 Firefox/12.0";

            request.KeepAlive = false;
            request.ProtocolVersion = HttpVersion.Version10;
            request.ServicePoint.ConnectionLimit = 1;

            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (Exception ex)
            {
                return new Result() { Successful = false };

            }

            //at this time we support only pdf content

            if (response.ContentType.ToLower() != "application/pdf")
            {
                return new Result() {Successful = false};
            }

            try
            {
                using (var stream = File.Create(file))
                {
                    response.GetResponseStream().CopyTo(stream);
                }
            }
            catch (Exception ex)
            {
                return new Result() { Successful = false };
            }

            return new Result() { Successful = true, ContentType = response.ContentType.ToLower() };
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

        public class Result
        {
            public bool Successful { get; set; }
            public string ContentType { get; set; }
        }
    }
}
