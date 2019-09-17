using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Oereb.Report.Helper.Exceptions
{
    public class ImageConversionException : Exception
    {
        public ImageConversionException(string url, string description = "") : base("Could not process image from URL (wrong format): " + url + ". " + description)
        {

        }
    }
}