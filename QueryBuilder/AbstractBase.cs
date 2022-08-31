using Newtonsoft.Json.Linq;
using QueryBuilder.DataTypes;

namespace QueryBuilder
{
    public abstract class AbstractBase
    {
        protected string TableName { get; set; } = "EXAMPLE_TABLE_NAME";
        protected string ModifiedBy { get; set; } = "XYZ";
        protected SqlFunction CurrentTimestampCall { get; set; } = new("CURRENT_TIMESTAMP()");

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

        private string JTokenTypeStringToString(JToken token)
        {
            string strToken = token.ToString();

            // If JTopkeType.String has a function call prefix, it needs to be read differently
            if (strToken.StartsWith(SqlFunction.FunctionLiteralPrefix))
                return strToken[SqlFunction.FunctionLiteralPrefix.Length..];

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
