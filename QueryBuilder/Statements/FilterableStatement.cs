using System.Text;
using Newtonsoft.Json.Linq;

namespace QueryBuilder.Statements
{
    public abstract class FilterableStatement : AbstractBase
    {
        // Dict where key is FilteredColumnName,
        // value is a pair where key is an arithmetic operator sign
        // and value is used on the right side of the where clause
        protected Dictionary<string, KeyValuePair<string, JToken>> WhereClauses { get; } = new();

        public void Where(string columnName, string arithmeticSign, JToken value)
        {
            KeyValuePair<string, JToken> pair = new(arithmeticSign, value);
            WhereClauses.Add(columnName, pair);
        }

        protected string SerializeWhereClauses(TimeZoneInfo timeZone)
        {
            if (WhereClauses.Count == 0)
                return "";
            
            StringBuilder whereClauseLiterals = new();
            whereClauseLiterals.Append("WHERE ");

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
    }
}
