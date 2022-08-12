using System.Text;
using QueryBuilder;

namespace QueryBuilderTest
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
            StringBuilder transaction = new StringBuilder();

            transaction.AppendLine("BEGIN");

            foreach(IStatement statement in Statements) 
            {
                string literal = GetStatementLiteral(statement);
                transaction.AppendLine(literal);
            }

            transaction.AppendLine("END;");

            return transaction.ToString();
        }

        private string GetStatementLiteral(IStatement statement)
        {
            if (statement.GetType() == typeof(Insert))
            {
                Insert insert = (Insert)statement;
                return insert.ToString();
            }
            else
            {
                Update update = (Update)statement;
                return update.ToString();
            }
        }
    }
}