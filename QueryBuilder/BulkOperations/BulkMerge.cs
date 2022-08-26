using System.Text;
using Newtonsoft.Json.Linq;
using QueryBuilder.DataTypes;
using QueryBuilder.Statements;

namespace QueryBuilder.BulkOperations
{
    public class BulkMerge
    {
        private JArray IncomingEntities { get; } = new();
        private JArray ExistingTableState { get; } = new();
        public List<Transaction> Transactions { get; set; } = new();
        public ushort MaxTransactionSize { get; } = 512;
        private string TableName { get; set; } = "EXAMPLE_TABLE_NAME";
        private List<string> PrimaryKeyIdentifiers { get; set; } = new();
        private SqlFunction CurrentTimestampCall { get; set; } = new("CURRENT_TIMESTAMP()");

        public BulkMerge() { }

        public BulkMerge(
            JArray incomingEntities,
            JArray existingTableState,
            string tableName,
            List<string> primaryKeyIdentifiers)
        {
            IncomingEntities = incomingEntities;
            ExistingTableState = existingTableState;
            TableName = tableName;
            PrimaryKeyIdentifiers = primaryKeyIdentifiers;

            InitializeTransactions();
        }

        private void InitializeTransactions()
        {
            Transaction transaction = new();

            for (int i = 0; i < IncomingEntities.Count; i++)
            {
                JToken entity = IncomingEntities[i];
                IEnumerable<JToken> matches = FindMatches(entity);
                IStatement? statement = TryGetStatement(entity, matches);

                if (statement != null)
                    transaction.Statements.Add(statement);

                if (transaction.Statements.Count % MaxTransactionSize == 0)
                {
                    Transactions.Add(transaction);
                    transaction = new Transaction();
                }
                if (i == IncomingEntities.Count - 1)
                    Transactions.Add(transaction);
            }
        }

        private IEnumerable<JToken> FindMatches(JToken original)
        {
            IEnumerable<JToken> matches = ExistingTableState;

            if (PrimaryKeyIdentifiers.Count == 0)
                return new List<JToken>();

            foreach (string identifier in PrimaryKeyIdentifiers)
            {
                matches = matches.Where(
                    x => x[identifier].ToString() == original[identifier].ToString());
            }

            return matches;
        }

        private IStatement? TryGetStatement(JToken entity, IEnumerable<JToken> matches)
        {
            if (!matches.Any())
                return GetInsertFrom(entity);

            JToken match = matches.First();
            if (!JToken.DeepEquals(entity, match))
                return GetUpdateFrom(entity);

            return null;
        }
        private Insert GetInsertFrom(JToken token)
        {
            Insert insert = new(TableName);
            foreach (JProperty prop in token.Cast<JProperty>())
            {
                insert.AddColumn(prop.Name, prop.Value);
            }

            insert.AddColumn("MODIFIED_AT", CurrentTimestampCall);
            insert.AddColumn("MODIFIED_BY", "NOT LOGGED IN");

            return insert;
        }

        private Update GetUpdateFrom(JToken token)
        {
            Update update = new(TableName);
            foreach (JProperty prop in token.Cast<JProperty>())
            {
                update.AddColumn(prop.Name, prop.Value);
            }

            update.AddColumn("MODIFIED_AT", CurrentTimestampCall);
            update.AddColumn("MODIFIED_BY", "NOT LOGGED IN");

            foreach (string identifier in PrimaryKeyIdentifiers)
            {
                update.Where(identifier, "=", token[identifier]);
            }

            return update;
        }

        public string ToString(TimeZoneInfo timeZone)
        {
            StringBuilder text = new();
            foreach (Transaction t in Transactions)
            {
                string transactionStr = t.ToString(timeZone);
                text.AppendLine(transactionStr);
            }
            return text.ToString();
        }
    }
}
