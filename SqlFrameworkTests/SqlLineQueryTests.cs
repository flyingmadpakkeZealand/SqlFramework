using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlFramework;
using SqlFramework.Line;
using TestModelLib;
using static SqlFrameworkTests.TestsSetup;

namespace SqlFrameworkTests
{
    [TestClass]
    public class SqlLineQueryTests
    {
        [TestMethod]
        public void SelectTest()
        {
            SqlLine sqlLine = SqlLine.Build().Select(PERSONS_TABLE);

            List<Person> persons = sqlLine.ExecuteQuery(ReadPerson);

            Assert.AreEqual(TestsPersons.Length, persons.Count);
            Assert.AreNotEqual(null, persons.Find(p => p.Name == TestsPersons[0].Name));
        }
    }
}