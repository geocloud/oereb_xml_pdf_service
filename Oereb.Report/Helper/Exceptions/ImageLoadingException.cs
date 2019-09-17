using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Oereb.Report.Helper.Exceptions
{
    public class ImageLoadingException : Exception
    {
        public ImageLoadingException(string url, string description = "") : base("Could not load image from URL: " + url + ". " + description)
        {

        }
    }
}