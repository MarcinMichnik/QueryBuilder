﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
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
        private string TableName { get; set; } = "";
        private List<string> PrimaryKeyIdentifiers { get; set; } = new();
        private SqlFunction CurrentTimestampCall { get; set; } = new SqlFunction("CURRENT_TIMESTAMP()");

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

        public override string ToString()
        {
            StringBuilder text = new();
            foreach (Transaction t in Transactions)
            {
                text.AppendLine(t.ToString());
            }
            return text.ToString();
        }

        private void InitializeTransactions()
        {
            Transaction transaction = new();

            for (int i = 0; i < IncomingEntities.Count; i++)
            {
                JToken entity = IncomingEntities[i];
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
