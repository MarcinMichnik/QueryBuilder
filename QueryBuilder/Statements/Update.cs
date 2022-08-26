using System.Text;
using Microsoft.VisualBasic;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;

namespace QueryBuilder.Statements
{
    public class Update : Statement, IStatement
    {
        // Dict where key is FilteredColumnName,
        // value is a pair where key is an arithmetic operator sign
        // and value is used on the right side of the where clause
        private Dictionary<string, KeyValuePair<string, JToken>> WhereClauses { get; } = new();

        public Update(string tableName)
        {
            TableName = tableName;
        }

        public void Where(string columnName, string arithmeticSign, JToken value)
        {
            KeyValuePair<string, JToken> pair = new(arithmeticSign, value);
            WhereClauses.Add(columnName, pair);
        }

        public string ToString(TimeZoneInfo timeZone)
        {
            if (WhereClauses.Count == 0)
            {
                throw new Exception(
                    "WhereClauses must not be empty or else the update will affect the entire table!");
            }

            string primaryKeyLookups = SerializeWhereClauses(timeZone);
            string columns = SerializeColumns(timeZone);

            return @$"UPDATE {TableName} SET
                          {columns} 
                      WHERE {primaryKeyLookups};";
        }

        private string SerializeColumns(TimeZoneInfo timeZone)
        {
            StringBuilder columns = new StringBuilder();
            foreach (KeyValuePair<string, JToken> column in Columns)
            {
                string convertedValue = ConvertJTokenToString(column.Value, timeZone);
                string columnLiteral = $"{column.Key} = {convertedValue},";
                columns.AppendLine(columnLiteral);
            }

            // remove last comma and newline
            int newLineLength = Environment.NewLine.Length;
            int valueToSubtract = newLineLength + 1;
            columns.Length -= valueToSubtract;

            return columns.ToString();
        }

        private string SerializeWhereClauses(TimeZoneInfo timeZone)
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
    }
}