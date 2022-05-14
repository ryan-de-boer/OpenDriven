using System;
using NUnit.Framework;

namespace OpenDriven.Framework35.Tests
{
    [TestFixture]
    [Category("Unit")]
    public class ClassTests
    {
        [Test]
        public void TestA()
        {
            //      bool it = true;
            //while (it)
            //      {
            //          System.Threading.Thread.Sleep(1000);
            //      }

            Assert.That("A", Is.EqualTo("B"), "Expected text differs");
        }
    }
}