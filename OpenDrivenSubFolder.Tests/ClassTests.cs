using System;
using NUnit.Framework;

namespace OpenDrivenSubFolder.Tests
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

            Assert.That("A", Is.EqualTo("A"), "Expected text differs");
        }

        [Test]
        public void TestB()
        {
            Assert.That("A", Is.EqualTo("A"), "Expected text differs");
        }

        [Test]
        public void TestC()
        {
            Assert.That("A", Is.EqualTo("A"), "Expected text differs");
        }
    }
}