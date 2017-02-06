using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Oereb.Service.DataContracts;

namespace Oereb.Service.Tests.Helper
{
    public class TestUtilities
    {
        public static TException AssertErrorcodeEqual<TException>(Action action, string expectedErrorcode) where TException : ExtException
        {
            string thrownErrorcode = "";

            try
            {
                action();
            }
            catch (TException ex)
            {
                thrownErrorcode = ex.ErrorCode;

                if (ex.ErrorCode == expectedErrorcode)
                {
                    return ex;
                }
            }
            catch (System.Exception ex)
            {
                Assert.Fail($"System.Exception was happening, {ex.Message}");
                return null;
            }

            if (String.IsNullOrEmpty(thrownErrorcode))
            {
                Assert.Fail("no Exception was happening");
                return null;
            }

            Assert.Fail($"not the expected exception with code <{expectedErrorcode}> was thrown, the fired error code was <{thrownErrorcode}>");
            return null;
        }
    }
}
