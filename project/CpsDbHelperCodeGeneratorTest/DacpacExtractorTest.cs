using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CpsDbHelper.CodeGenerator;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CpsDbHelperCodeGeneratorTest
{
    [TestClass]
    public class DacpacExtractorTest
    {
        [TestMethod]
        public void TestExtractDacpack()
        {
            const string path = @"../../../CpsDbHelper.TestDatabase/bin/Debug/CpsDbHelper.TestDatabase.dacpac";
            DacpacExtractor.ParseDacpac(path, "namespaces_example");
        }
    }
}
