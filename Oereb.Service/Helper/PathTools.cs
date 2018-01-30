using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Oereb.Service.Helper
{
    public class PathTools
    {
        public static string Rootpath()
        {
            var binPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);

            if (binPath.StartsWith(@"file:\"))
            {
                binPath = binPath.Replace(@"file:\", "");
            }

            return binPath;
        }
    }
}