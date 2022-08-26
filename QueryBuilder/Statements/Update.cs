using System.Text;
using Microsoft.VisualBasic;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;

namespace QueryBuilder.Statements
{
    public class Update : Statement, IStatement
    {
        // list of pairs where key is primaryKeyName, value is filter for updating
        public Dictionary<string, JToken> WhereClauses { get; set; } = new();

        public Update(string tableName)
        {
            TableName = tableName;
        }

        override public string ToString()
        {
            if (WhereClauses.Count == 0)
                throw new Exception("Primary key lookup list (where clauses) must not be empty!");

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
            foreach (KeyValuePair<string, JToken> primaryKeyLookup in WhereClauses)
            {
                string convertedValue = ConvertJTokenToString(primaryKeyLookup.Value);
                string primaryKeyLookupLiteral = $"{primaryKeyLookup.Key} = {convertedValue} AND ";
                primaryKeyLookups.Append(primaryKeyLookupLiteral);
            }

            // remove last AND
            primaryKeyLookups.Length = primaryKeyLookups.Length - 5;

            return primaryKeyLookups.ToString();
        }
    }
}