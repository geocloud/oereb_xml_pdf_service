using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Oereb.Report.Helper.Exceptions
{
    public class WmsRequestException : Exception
    {
        public WmsRequestException(string url, string description = "") : base("Could not load from WMS URL: " + url + ". " + description)
        {

        }
    }
}