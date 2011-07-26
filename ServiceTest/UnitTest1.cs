using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DiversityService;

namespace ServiceTest
{
    [TestClass]
    public class UnitTest1
    {
        DivService _target;
        public UnitTest1()
        {
            _target = new DivService();
        }

        [TestMethod]
        public void TestIUInsert()
        {
        }
    }
}
