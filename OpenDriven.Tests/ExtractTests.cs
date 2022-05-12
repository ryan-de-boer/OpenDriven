using System;
using NUnit.Framework;
using OpenDriven.Commands;

namespace OpenDriven.Tests
{
  [TestFixture]
  [Category("Unit")]
  public class ExtractTests
  {
    [Test]
    public void TestExtractNamespaceTest()
    {
      //      bool it = true;
      //while (it)
      //      {
      //          System.Threading.Thread.Sleep(1000);
      //      }

      string text = File.ReadAllText(@"D:\code\other\OpenDriven\git\OpenDriven\OpenDriven.Tests\TestData\Text.txt");

      string expectedNamespaceTest = "Hardware.Graphics.Tests.UnitTests.GfxUnitTests.TestCreateGraphicsFromJson";

      string actualNamespaceTest = DebugTestsCommand.ExtractNamespaceTest(text);

      Assert.That(actualNamespaceTest, Is.EqualTo(expectedNamespaceTest), "Expected namespace text differs");
    }
  }
}