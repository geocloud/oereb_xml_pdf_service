using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Oereb.Report.Helper.Exceptions
{
    public class AttachmentRequestException : Exception
    {
        public AttachmentRequestException(string url, string description = "") : base("Could not load document for attachment at URL: " + url + ". " + description)
        {

        }
    }
}