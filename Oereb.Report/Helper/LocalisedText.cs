using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oereb.Report.Helper
{
    public static class LocalisedText
    {
        public static string GetStringFromArray(Service.DataContracts.Model.v10.LocalisedText[] localisedTexts, string language, string defaultValue = "-")
        {
            if (localisedTexts == null)
            {
                return defaultValue;
            }

            if (localisedTexts.Any(x=> x.Language.ToString() == language))
            {
                return localisedTexts.First().Text;
            }

            return defaultValue;
        }
    }
}
