using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Newtonsoft.Json.Linq;

namespace QueryBuilder.BulkOperations
{
    public class BulkMerge
    {
        public JArray IncomingEntities { get; }

        public JArray ExistingTableState { get; }

        public List<Transaction> Transactions { get; set; } = new();

        private ushort MaxTransactionSize { get; } = 512;

        public string TableName { get; set; }

        public List<string> PrimaryKeyIdentifiers { get; set; }

        private SqlFunction CurrentTimestampCall { get; set; } = new SqlFunction("CURRENT_TIMESTAMP()");

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

            Transaction transaction = InitializeTranzaction();
            Transactions.Add(transaction);
        }

        private Transaction InitializeTranzaction()
        {
            Transactions = new List<Transaction>();

            Transaction transaction = new();

            foreach (JToken entity in IncomingEntities)
            {
                IEnumerable<JToken> matches = FindMatches(entity);
                IStatement? statement = null;

                if (matches.Any())
                {
                    JToken match = matches.First();
                    if (!JToken.DeepEquals(entity, match))
                        statement = GetUpdateFrom(entity);
                }
                else
                    statement = GetInsertFrom(entity);

                if (statement != null)
                    transaction.Statements.Add(statement);
            }

            return transaction;
        }

        override public string ToString()
        {
            return Transactions.First().ToString();
        }

        public bool Equals(BulkMerge other)
        { 
            return other.ToString().Equals(ToString());
        }

        private IEnumerable<JToken> FindMatches(JToken original)
        {
            IEnumerable<JToken> matches = ExistingTableState;

            if (PrimaryKeyIdentifiers.Count == 0)
                return new List<JToken>();

            foreach (string identifier in PrimaryKeyIdentifiers)
                matches = matches.Where(x => x[identifier].ToString() == original[identifier].ToString());

            return matches;
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
                KeyValuePair<string, JToken> pair = new(identifier, token[identifier]);
                update.PrimaryKeyLookups.Add(pair);
            }

            return update;
        }
    }
}
