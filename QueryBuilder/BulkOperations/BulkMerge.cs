using System.Text;
using Newtonsoft.Json.Linq;
using QueryBuilder.Statements;

namespace QueryBuilder.BulkOperations
{
    public class BulkMerge : AbstractBase
    {
        // Can produce many sql transactions in case there are too many statements
        private JArray IncomingEntities { get; } = new();
        private JArray ExistingTableState { get; } = new();
        private List<Transaction> Transactions { get; set; } = new();
        public ushort MaxTransactionSize { get; } = 2048;
        private List<string> PrimaryKeyIdentifiers { get; set; } = new();
        private Dictionary<OperationResult, int> OperationResults { get; } = new() 
        {
            { OperationResult.INSERTED, 0 },
            { OperationResult.UPDATED, 0 },
            { OperationResult.SKIPPED, 0 }
        };

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
                CountOperationResult(statement);

                if (statement != null)
                    transaction.AddStatement(statement);

                if (transaction.GetStatementCount() % MaxTransactionSize == 0)
                {
                    Transactions.Add(transaction);
                    transaction = new Transaction();
                }

                if (isLastLoopIteration(i))
                    Transactions.Add(transaction);
            }
        }

        private void CountOperationResult(IStatement? statement)
        {
            if (statement == null)
            {
                OperationResults[OperationResult.SKIPPED]++;
                return;
            }

            if (statement.GetType() == typeof(Insert))
            {
                OperationResults[OperationResult.INSERTED]++;
                return;
            }
            else { // Update
                OperationResults[OperationResult.UPDATED]++;
                return;
            }
        }

        private bool isLastLoopIteration(int i)
        {
            return i == IncomingEntities.Count - 1;
        }

        public void AddTransaction(Transaction transaction)
        {
            Transactions.Add(transaction);
        }

        public int GetTransactionCount()
        { 
            return Transactions.Count;
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
                return GetInsertFromToken(entity);

            JToken match = matches.First();
            if (!JToken.DeepEquals(entity, match))
                return GetUpdateFromToken(entity);

            return null;
        }
        private Insert GetInsertFromToken(JToken token)
        {
            Insert insert = new(TableName);
            foreach (JProperty prop in token.Cast<JProperty>())
            {
                insert.AddColumn(prop.Name, prop.Value);
            }

            insert.AddColumn("MODIFIED_AT", CurrentTimestampCall);
            insert.AddColumn("MODIFIED_BY", ModifiedBy);

            return insert;
        }

        private Update GetUpdateFromToken(JToken token)
        {
            Update update = new(TableName);
            foreach (JProperty prop in token.Cast<JProperty>())
            {
                if (PrimaryKeyIdentifiers.Contains(prop.Name))
                    continue;

                update.AddColumn(prop.Name, prop.Value);
            }

            update.AddColumn("MODIFIED_AT", CurrentTimestampCall);
            update.AddColumn("MODIFIED_BY", ModifiedBy);

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

        public int GetInsertCount()
        {
            return OperationResults[OperationResult.INSERTED];
        }

        public int GetUpdateCount()
        {
            return OperationResults[OperationResult.UPDATED];
        }

        public int GetSkipCount()
        {
            return OperationResults[OperationResult.SKIPPED];
        }
    }
}
