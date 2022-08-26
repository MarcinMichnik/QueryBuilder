using System.Text;
using System.Text.RegularExpressions;
using QueryBuilder.Statements;

namespace QueryBuilder
{
    public class Transaction
    {
        public List<IStatement> Statements { get; set; }

        public Transaction()
        {
            Statements = new List<IStatement>();
        }

        override public string ToString()
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
                string literal = GetStatementLiteral(statement);
                transaction.AppendLine(literal);
            }
            return transaction.ToString();
        }

        private string GetStatementLiteral(IStatement statement)
        {
            if (statement.GetType() == typeof(Insert))
            {
                Insert insert = (Insert)statement;
                return insert.ToString();
            }
            else {
                Update update = (Update)statement;
                return update.ToString();
            }
        }
    }
}