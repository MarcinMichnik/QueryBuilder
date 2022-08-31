using System;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using QueryBuilder.DataTypes;

namespace QueryBuilder.Statements
{
    public abstract class Statement
    {
        protected Dictionary<string, JToken> Columns { get; set; } = new();

        // Dict where key is FilteredColumnName,
        // value is a pair where key is an arithmetic operator sign
        // and value is used on the right side of the where clause
        protected Dictionary<string, KeyValuePair<string, JToken>> WhereClauses { get; } = new();
        protected string TableName { get; set; } = "EXAMPLE_TABLE_NAME";

        public void Where(string columnName, string arithmeticSign, JToken value)
        {
            KeyValuePair<string, JToken> pair = new(arithmeticSign, value);
            WhereClauses.Add(columnName, pair);
        }

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

        protected string ConvertJTokenToString(JToken token, TimeZoneInfo timeZone)
        {
            switch (token.Type)
            {
                case JTokenType.String:
                    return JTokenTypeStringToString(token);
                case JTokenType.Integer:
                    return token.ToString();
                case JTokenType.Float:
                    return JtokenTypeFloatToString(token);
                case JTokenType.Date:
                    return JTokenTypeDateToString(token, timeZone);
                default:
                    throw new Exception($"NOT EXPECTED TYPE: {token.Type}");
            }
        }

        protected string SerializeWhereClauses(TimeZoneInfo timeZone)
        {
            StringBuilder whereClauseLiterals = new StringBuilder();
            foreach (KeyValuePair<string, KeyValuePair<string, JToken>> primaryKeyLookup in WhereClauses)
            {
                string arithmeticSign = primaryKeyLookup.Value.Key;
                string convertedValue = ConvertJTokenToString(primaryKeyLookup.Value.Value, timeZone);
                string whereClauseLiteral = $"{primaryKeyLookup.Key} {arithmeticSign} {convertedValue} AND ";
                whereClauseLiterals.Append(whereClauseLiteral);
            }

            whereClauseLiterals.Length -= 5; // remove last " AND "

            return whereClauseLiterals.ToString();
        }

        private string JTokenTypeStringToString(JToken token)
        {
            string strToken = token.ToString();

            // If JTopkeType.String has a function call prefix, it needs to be read differently
            if (strToken.StartsWith(SqlFunction.FunctionLiteralPrefix))
            {
                return strToken[SqlFunction.FunctionLiteralPrefix.Length..];
            }

            return $"'{strToken}'";
        }

        private string JtokenTypeFloatToString(JToken token)
        {
            string stringLiteral = token.ToString();
            return stringLiteral.Replace(",", ".");
        }

        private string JTokenTypeDateToString(JToken token, TimeZoneInfo timeZone)
        {
            DateTime datetimeValue = (DateTime)token;
            DateTime.SpecifyKind(datetimeValue, DateTimeKind.Unspecified);
            TimeSpan offset = timeZone.GetUtcOffset(datetimeValue);
            DateTimeOffset dto = new(datetimeValue, offset);

            string date = dto.ToString("yyyy-MM-dd");
            string time = dto.ToString("HH:mm:ss");
            string dateTimeStr = $"{date}\"T\"{time}";
            return $"TO_DATE('{dateTimeStr}', 'YYYY-MM-DD\"T\"HH24:MI:SS')";
        }
    }
}