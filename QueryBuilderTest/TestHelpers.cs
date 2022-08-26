using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QueryBuilderTest
{
    static class TestHelpers
    {
        public static TimeZoneInfo TimeZone { get; set; }
            = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");

        public static string RemoveWhitespace(string str)
        {
            return Regex.Replace(str, @"\s", "");
        }
    }
}
