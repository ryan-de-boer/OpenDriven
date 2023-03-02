using System;
using NUnit.Framework;

namespace OpenDriven._5.Tests
{

  [TestFixture]
  [Category("Unit")]
  public class Class2Tests : IInterface
  {
    public void Check()
    {
    }

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
