using System.Text;
using Newtonsoft.Json.Linq;
using QueryBuilder.DataTypes;

namespace QueryBuilder.Statements
{
    public abstract class Statement : AbstractBase
    {
        // Dict where key is FilteredColumnName,
        // value is a pair where key is an arithmetic operator sign
        // and value is used on the right side of the where clause

        // By default where clauses are not available
        protected Dictionary<string, KeyValuePair<string, JToken>>? WhereClauses { get; set; } = null;

        protected Dictionary<string, JToken>? Columns { get; set; } = null;

        public void AddColumn(string name, JToken value)
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                Columns.Add(name, value);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }

        public void AddColumn(string name, SqlFunction function)
        {
            // Save function as JTokenType.String with a prefix
            string functionLiteral = function.GetPrefixedLiteral();
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            Columns.Add(name, functionLiteral);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }

        public void Where(string columnName, string arithmeticSign, JToken value)
        {
            KeyValuePair<string, JToken> pair = new(arithmeticSign, value);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            WhereClauses.Add(columnName, pair);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }

        protected string SerializeWhereClauses(TimeZoneInfo timeZone)
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            if (WhereClauses.Count == 0)
                return "";
#pragma warning restore CS8602 // Dereference of a possibly null reference.

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
