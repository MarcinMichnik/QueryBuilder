using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using QueryBuilder;
using QueryBuilder.BulkOperations;

namespace QueryBuilderTest
{
    internal class BulkMergeTest
    {

        public SqlFunction SqlFunctionEntity { get; set; } = new("CURRENT_TIMESTAMP()");

        [Test]
        public void TestSimpleMerge()
        {
            JArray incoming = GetArrayOfEntities(1);
            JArray existing = GetArrayOfEntities(2);

            string tableName = "APP";
            List<string> primaryKeyIdentifiers = new() { "ID" };

            BulkMerge dm = new(
                incoming, existing, tableName, primaryKeyIdentifiers);
            Transaction actual = dm.Transactions.First();

            Transaction expected = new();
            Insert expectedInput = new(tableName);
            expectedInput.AddColumn("ID", 1);
            expectedInput.AddColumn("NAME", "HANNAH");
            expectedInput.AddColumn("MODIFIED_AT", SqlFunctionEntity);
            expectedInput.AddColumn("MODIFIED_BY", "NOT LOGGED IN");
            expected.Statements.Add(expectedInput);

            string actualEscaped = Regex.Replace(actual.ToString(), @"\s", "");
            string expectedEscaped = Regex.Replace(expected.ToString(), @"\s", "");

            Assert.That(actualEscaped, Is.EqualTo(expectedEscaped));
        }

        [Test]
        public void TestBulkMerge()
        {
            int operationSize = 1000;

            JArray incoming = new();
            for (int i = 1; i <= operationSize; i++)
            {
                JObject o = new()
                {
                    ["ID"] = i,
                    ["NAME"] = "HANNAH"
                };
                incoming.Add(o);
            }

            JArray existing = new();
            for (int i = 2; i < operationSize; i++)
            {
                JObject o = new()
                {
                    ["ID"] = i,
                    ["NAME"] = "HANNAH"
                };
                existing.Add(o);
            }

            string tableName = "APP";
            List<string> primaryKeyIdentifiers = new() { "ID" };

            BulkMerge dm = new(
                incoming, existing, tableName, primaryKeyIdentifiers);
            Transaction actual = dm.Transactions.First();

            Transaction expected = new();
            Insert expectedInputOne = new(tableName);
            expectedInputOne.AddColumn("ID", 1);
            expectedInputOne.AddColumn("NAME", "HANNAH");
            expectedInputOne.AddColumn("MODIFIED_AT", SqlFunctionEntity);
            expectedInputOne.AddColumn("MODIFIED_BY", "NOT LOGGED IN");
            expected.Statements.Add(expectedInputOne);

            Insert expectedInputTwo = new(tableName);
            expectedInputTwo.AddColumn("ID", operationSize);
            expectedInputTwo.AddColumn("NAME", "HANNAH");
            expectedInputTwo.AddColumn("MODIFIED_AT", SqlFunctionEntity);
            expectedInputTwo.AddColumn("MODIFIED_BY", "NOT LOGGED IN");
            expected.Statements.Add(expectedInputTwo);

            string actualEscaped = Regex.Replace(actual.ToString(), @"\s", "");
            string expectedEscaped = Regex.Replace(expected.ToString(), @"\s", "");

            Assert.That(actualEscaped, Is.EqualTo(expectedEscaped));
        }

        private JArray GetArrayOfEntities(int id)
        {
            JArray array = new();
            JObject element = new()
            {
                ["ID"] = id,
                ["NAME"] = "HANNAH"
            };
            array.Add(element);
            return array;
        }
    }
}
