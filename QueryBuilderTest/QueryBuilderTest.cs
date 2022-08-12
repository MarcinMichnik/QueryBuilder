using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using QueryBuilder;

namespace QueryBuilderTest
{
    public class QueryBuilderTest
    {
        public string TableName { get; set; }

        [SetUp]
        public void Setup()
        {
            TableName = "\"APP\".\"EXAMPLARY_TABLE_NAME\"";
        }

        [Test]
        public void TestQueryBuilderInsert()
        {
            Statement query = GetExamplaryInsert();

            string actual = query.ToString();

            string expected = @$"INSERT INTO {TableName} (
                                    MASTER_ID, ID, NAME, SAVINGS, MODIFIED_AT, MODIFIED_BY
                                ) 
                                VALUES (
                                    SEQ.NEXT_VAL,
                                    1,
                                    'HANNAH',
                                    12.1,
                                    TO_DATE('2021-01-01T00:00:00', 'YYYY-MM-DD""T""HH24:MI:SS'),
                                    'NOT LOGGED IN'
                                );";

            string actualEscaped = Regex.Replace(actual, @"\s", "");
            string expectedEscaped = Regex.Replace(expected, @"\s", "");

            Console.WriteLine(actualEscaped);
            Console.WriteLine(expectedEscaped);

            Assert.That(actualEscaped, Is.EqualTo(expectedEscaped));
        }

        [Test]
        public void TestQueryBuilderInsertWithoutMasterPrimaryKey()
        {
            Statement query = GetInsertWithoutMasterPrimaryKey();

            string actual = query.ToString();

            string expected = @$"INSERT INTO {TableName} (
                                    ID, NAME, SAVINGS, MODIFIED_AT, MODIFIED_BY
                                ) 
                                VALUES (
                                    1,
                                    'HANNAH',
                                    12.1,
                                    TO_DATE('2021-01-01T00:00:00', 'YYYY-MM-DD""T""HH24:MI:SS'),
                                    'NOT LOGGED IN'
                                );";

            string actualEscaped = Regex.Replace(actual, @"\s", "");
            string expectedEscaped = Regex.Replace(expected, @"\s", "");

            Console.WriteLine(actualEscaped);
            Console.WriteLine(expectedEscaped);

            Assert.That(actualEscaped, Is.EqualTo(expectedEscaped));
        }

        [Test]
        public void TestQueryBuilderUpdateWithOnePrimaryKey()
        {
            Update query = GetUpdateWithOnePrimaryKey();

            string actual = query.ToString();

            string expected = @$"UPDATE {TableName} SET
                                    NAME = 'HANNAH',
                                    SAVINGS = 12.1,
                                    MODIFIED_AT = TO_DATE('2021-01-01T00:00:00', 'YYYY-MM-DD""T""HH24:MI:SS'),
                                    MODIFIED_BY = 'NOT LOGGED IN'
                                WHERE ID = 1;";

            string actualEscaped = Regex.Replace(actual, @"\s", "");
            string expectedEscaped = Regex.Replace(expected, @"\s", "");

            Assert.That(actualEscaped, Is.EqualTo(expectedEscaped));
        }

        [Test]
        public void TestQueryBuilderUpdateWithManyPrimaryKeys()
        {
            Update query = GetUpdateWithManyPrimaryKeys();

            string actual = query.ToString();


            string expected = @$"UPDATE {TableName} SET
                                    NAME = 'HANNAH',
                                    SAVINGS = 12.1,
                                    MODIFIED_AT = TO_DATE('2021-01-01T00:00:00', 'YYYY-MM-DD""T""HH24:MI:SS'),
                                    MODIFIED_BY = 'NOT LOGGED IN'
                                WHERE ID = 1 AND EXTERNAL_ID = 301;";

            string actualEscaped = Regex.Replace(actual, @"\s", "");
            string expectedEscaped = Regex.Replace(expected, @"\s", "");

            Assert.That(actualEscaped, Is.EqualTo(expectedEscaped));
        }

        private Insert GetExamplaryInsert()
        {
            Insert query = new Insert(TableName);

            query.MasterPrimary = new KeyValuePair<string, string>("MASTER_ID", "SEQ.NEXT_VAL");

            query.AddColumn("ID", 1);
            query.AddColumn("NAME", "HANNAH");
            query.AddColumn("SAVINGS", 12.1);

            DateTime modifiedAt = DateTime.Parse("2021-01-01T00:00:00+00:00");
            query.AddColumn("MODIFIED_AT", modifiedAt);

            query.AddColumn("MODIFIED_BY", "NOT LOGGED IN");

            return query;
        }

        private Insert GetInsertWithoutMasterPrimaryKey()
        {
            Insert query = new Insert(TableName);

            query.AddColumn("ID", 1);
            query.AddColumn("NAME", "HANNAH");
            query.AddColumn("SAVINGS", 12.1);

            DateTime modifiedAt = DateTime.Parse("2021-01-01T00:00:00+00:00");
            query.AddColumn("MODIFIED_AT", modifiedAt);

            query.AddColumn("MODIFIED_BY", "NOT LOGGED IN");

            return query;
        }

        private Update GetUpdateWithOnePrimaryKey()
        {
            Update query = new Update(TableName);

            KeyValuePair<string, JToken> primaryKeyLookup
                = new KeyValuePair<string, JToken>("ID", 1);
            query.PrimaryKeyLookups.Add(primaryKeyLookup);

            query.AddColumn("NAME", "HANNAH");
            query.AddColumn("SAVINGS", 12.1);

            DateTime modifiedAt = DateTime.Parse("2021-01-01T00:00:00+00:00");
            query.AddColumn("MODIFIED_AT", modifiedAt);

            query.AddColumn("MODIFIED_BY", "NOT LOGGED IN");

            return query;
        }

        private Update GetUpdateWithManyPrimaryKeys()
        {
            Update query = new Update(TableName);

            KeyValuePair<string, JToken> primaryKeyLookup 
                = new KeyValuePair<string, JToken>("ID", 1);
            query.PrimaryKeyLookups.Add(primaryKeyLookup);

            KeyValuePair<string, JToken> primaryKeyLookup2 
                = new KeyValuePair<string, JToken>("EXTERNAL_ID", 301);
            query.PrimaryKeyLookups.Add(primaryKeyLookup2);

            query.AddColumn("NAME", "HANNAH");
            query.AddColumn("SAVINGS", 12.1);

            DateTime modifiedAt = DateTime.Parse("2021-01-01T00:00:00+00:00");
            query.AddColumn("MODIFIED_AT", modifiedAt);

            query.AddColumn("MODIFIED_BY", "NOT LOGGED IN");

            return query;
        }
    }
}