using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using QueryBuilder.DataTypes;

namespace QueryBuilder.Statements
{
    public abstract class Statement : IStatement
    {
        public string TableName { get; set; }
        public Dictionary<string, JToken> Columns { get; set; } = new();

        // used to distinguish sql function calls from regular string values
        // since both are stored as a JTokenType.String
        // sql function calls will start with this prefix

        // IMPORTANT - this implementation presumes
        // that "\f\n" would never be used as first characters in a function literal name
        private readonly string functionLiteralPrefix = "\f\n";

        protected Statement(string tableName)
        {
            TableName = tableName;
        }

        public void AddColumn(string name, JToken value)
        {
            Columns.Add(name, value);
        }

        public void AddColumn(string name, SqlFunction function)
        {
            string functionLiteral = $"{functionLiteralPrefix}{function.Literal}";
            Columns.Add(name, functionLiteral);
        }

        protected string ConvertJTokenToString(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.String:
                    string strToken = token.ToString();
                    if (strToken.StartsWith(functionLiteralPrefix))
                        return strToken[functionLiteralPrefix.Length..];
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
                    throw new Exception("NOT EXPECTED");
            }
        }

        override public string ToString()
        {
            throw new Exception("NOT EXPECTED");
        }
    }
}