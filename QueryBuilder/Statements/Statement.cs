using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using QueryBuilder.DataTypes;

namespace QueryBuilder.Statements
{
    public abstract class Statement
    {
        public Dictionary<string, JToken> Columns { get; set; } = new();
        public string TableName { get; set; } = "EXAMPLE_TABLE_NAME";

        public void AddColumn(string name, JToken value)
        {
            Columns.Add(name, value);
        }

        public void AddColumn(string name, SqlFunction function)
        {
            // Save function as JTokenType.String with a prefix
            string functionLiteral = function.GetPrefixedLiteral();
            Columns.Add(name, functionLiteral);
        }

        protected static string ConvertJTokenToString(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.String:
                    string strToken = token.ToString();

                    // If JTopkeType.String has a function call prefix, it needs to be read differently
                    if (strToken.StartsWith(SqlFunction.FunctionLiteralPrefix))
                    {
                        return strToken[SqlFunction.FunctionLiteralPrefix.Length..];
                    }

                    return $"'{strToken}'";

                case JTokenType.Integer:
                    return token.ToString();
                case JTokenType.Float:
                    string stringLiteral = token.ToString();
                    return stringLiteral.Replace(",", ".");
                case JTokenType.Date:
                    DateTime datetimeValue = (DateTime)token;
                    DateTime utcDateTime = datetimeValue.ToUniversalTime();
                    string utcDateTimeString = utcDateTime.ToString("yyyy-MM-dd'T'HH:mm:ss");
                    return $"TO_DATE('{utcDateTimeString}', 'YYYY-MM-DD\"T\"HH24:MI:SS')";
                default:
                    throw new Exception($"NOT EXPECTED TYPE: {token.Type}");
            }
        }
    }
}