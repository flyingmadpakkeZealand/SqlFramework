using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlFramework;
using TestModelLib;

namespace SqlFrameworkTests
{
    [TestClass]
    public static class TestsSetup
    {
        public static Action None = () => { };

        public const string PERSONS_TABLE = "Persons";
        public static Person[] TestsPersons;

        [AssemblyInitialize]
        public static void Init(TestContext context)
        {
            Setup.ConnectionString = ConnectionString.CONNECTION_STRING;

            SetupPersonTable();
        }

        private static void SetupPersonTable()
        {
            TestsPersons = new[]
            {
                new Person(1, "Bob", 32),
                new Person(2, "Alice", 27),
                new Person(3, "foo", Byte.MaxValue)
            };

            Assert.That.RanToCompletion(() =>
            {
                try
                {
                    CreateTable();
                }
                catch (SqlException)
                {
                    DeleteTable(PERSONS_TABLE);
                    CreateTable();
                }
            });

            void CreateTable()
            {
                SqlLine initLine = new SqlLine($"CREATE TABLE [dbo].[{PERSONS_TABLE}] (" +
                                               "[person_id] INT NOT NULL, " +
                                               "[person_age] TINYINT NULL, " +
                                               "[person_name] NVARCHAR (50) NULL, " +
                                               "PRIMARY KEY CLUSTERED ([person_id] ASC))");

                initLine.ExecuteNonQuery();
            }


            Assert.That.RanToCompletion(() =>
            {
                SqlLine deleteLine = new SqlLine($"Delete from {PERSONS_TABLE}");

                deleteLine.ExecuteNonQuery();
            });


            Assert.That.RanToCompletion(() =>
            {
                SqlLine insertLine = new SqlLine($"Insert into {PERSONS_TABLE}").Values.Param<Person>(InsertPerson);

                insertLine.ExecuteNonQuery(TestsPersons[0]);
                insertLine.ExecuteNonQuery(TestsPersons[1]);
                insertLine.ExecuteNonQuery(TestsPersons[2]);
            });
        }

        public static SqlContainer InsertPerson(Person person)
        {
            return new SqlContainer()
            {
                new Parameter("person_id", person.Id),
                new Parameter("person_age", person.Age),
                new Parameter("person_name", person.Name)
            };
        }

        public static Person ReadPerson(SqlDataReader reader)
        {
            return new Person
            {
                Id = reader.GetInt32(reader.GetOrdinal("person_id")),
                Age = reader.GetByte(reader.GetOrdinal("person_age")),
                Name = reader.GetString(reader.GetOrdinal("person_name"))
            };
        }

        private static void DeleteTable(string tableName)
        {
            SqlLine deleteLine = new SqlLine($"Drop Table [dbo].[{tableName}]");

            deleteLine.ExecuteNonQuery();
        }

        public static void RanToCompletion(this Assert assert, Action code)
        {
            Assert.ThrowsException<RanToCompletionException>(() =>
            {
                code();
                throw new RanToCompletionException();
            });
        }
    }
}
