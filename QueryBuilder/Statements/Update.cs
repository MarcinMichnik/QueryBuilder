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

        override public string ToString()
        {
            if (WhereClauses.Count == 0)
                throw new Exception("WhereClauses must not be empty!");

            string primaryKeyLookups = GetPrimaryKeyLookups();
            string columns = GetColumns();

            return @$"UPDATE {TableName} SET
                          {columns} WHERE {primaryKeyLookups};";
        }

        private string GetColumns()
        {
            StringBuilder columns = new StringBuilder();
            foreach (KeyValuePair<string, JToken> column in Columns)
            {
                string convertedValue = ConvertJTokenToString(column.Value);
                string columnLiteral = $"{column.Key} = {convertedValue},";
                columns.AppendLine(columnLiteral);
            }

            // remove last comma and newline
            int newLineLength = Environment.NewLine.Length;
            columns.Length = columns.Length - newLineLength - 1;

            return columns.ToString();
        }

        private string GetPrimaryKeyLookups()
        {
            StringBuilder primaryKeyLookups = new StringBuilder();
            foreach (KeyValuePair<string, KeyValuePair<string, JToken>> primaryKeyLookup in WhereClauses)
            {
                string arithmeticSign = primaryKeyLookup.Value.Key;
                string convertedValue = ConvertJTokenToString(primaryKeyLookup.Value.Value);
                string primaryKeyLookupLiteral = $"{primaryKeyLookup.Key} {arithmeticSign} {convertedValue} AND ";
                primaryKeyLookups.Append(primaryKeyLookupLiteral);
            }

            // remove last AND
            primaryKeyLookups.Length -= 5;

            return primaryKeyLookups.ToString();
        }
    }
}