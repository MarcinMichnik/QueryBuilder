﻿using System.Text;
using Newtonsoft.Json.Linq;
using QueryBuilder.DataTypes;

namespace QueryBuilder.Statements
{
    public abstract class Statement : AbstractBase
    {
        // Dict where key is FilteredColumnName,
        // value is a pair where key is an arithmetic operator sign
        // and value is used on the right side of the where clause

        // By default where clauses are not available; To be created in constructor
        protected Dictionary<string, KeyValuePair<string, JToken>>? WhereClauses { get; set; } = null;

        // By default column list is not available; To be created in constructor
        protected Dictionary<string, JToken>? Columns { get; set; } = null;

        public void AddColumn(string name, JToken value)
        {
            if (Columns == null)
                throw new Exception(nameof(Columns));

            Columns.Add(name, value);
        }

        public void AddColumn(string name, SqlFunction function)
        {
            if (Columns == null)
                throw new Exception(nameof(Columns));

            // Save function as JTokenType.String with a prefix
            string functionLiteral = function.GetPrefixedLiteral();
            Columns.Add(name, functionLiteral);
        }

        public void Where(string columnName, string arithmeticSign, JToken value)
        {
            if (WhereClauses == null)
                throw new Exception(nameof(WhereClauses));

            KeyValuePair<string, JToken> pair = new(arithmeticSign, value);
            WhereClauses.Add(columnName, pair);
        }

        protected string SerializeWhereClauses(TimeZoneInfo timeZone)
        {
            if (WhereClauses is null || WhereClauses.Count == 0)
                return string.Empty;

            StringBuilder whereClauseLiterals = new();
            whereClauseLiterals.Append("WHERE ");

            foreach (KeyValuePair<string, KeyValuePair<string, JToken>> primaryKeyLookup in WhereClauses)
            {
                string arithmeticSign = primaryKeyLookup.Value.Key;
                string convertedValue = QueryBuilderTools.ConvertJTokenToString(primaryKeyLookup.Value.Value, timeZone);
                string whereClauseLiteral = $"{primaryKeyLookup.Key} {arithmeticSign} {convertedValue} AND ";
                whereClauseLiterals.Append(whereClauseLiteral);
            }

            whereClauseLiterals.Length -= 5; // remove last " AND "

            return whereClauseLiterals.ToString();
        }
    }
}
