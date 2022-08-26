using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using QueryBuilder.Statements;

namespace QueryBuilder
{
    public class Transaction
    {
        public List<IStatement> Statements { get; set; } = new();

        public override string ToString()
        {
            return @$"BEGIN
                        {GetStatementLiterals()}
                      END;";
        }

        private string GetStatementLiterals()
        {
            StringBuilder transaction = new StringBuilder();
            foreach (IStatement statement in Statements)
            {
                string literal = statement.ToString();
                transaction.AppendLine(literal);
            }
            return transaction.ToString();
        }
    }
}