using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using QueryBuilder.Statements;

namespace QueryBuilder
{
    public class Transaction
    {
        public List<IStatement> Statements { get; set; } = new();

        public string ToString(TimeZoneInfo timeZone)
        {
            return @$"BEGIN
                        {GetStatementLiterals(timeZone)}
                      END;";
        }

        private string GetStatementLiterals(TimeZoneInfo timeZone)
        {
            StringBuilder transaction = new();
            foreach (IStatement statement in Statements)
            {
                string literal = statement.ToString(timeZone);
                transaction.AppendLine(literal);
            }
            return transaction.ToString();
        }
    }
}