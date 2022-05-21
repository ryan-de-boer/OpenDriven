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

    [Test]
    public void TestLastRunMultiProject()
    {
      string text = File.ReadAllText(@"D:\code\other\OpenDriven\git\OpenDriven\OpenDriven.Tests\TestData\LastRunTest.txt");

      List<string> expectedPaths = new List<string>();
      expectedPaths.Add(@"D:\code\other\OpenDriven\git\OpenDriven\OpenDriven.5.Tests\bin\Debug\net5.0\OpenDriven.5.Tests.dll");
      expectedPaths.Add(@"D:\code\other\OpenDriven\git\OpenDriven\OpenDriven.Framework35.Tests\bin\Debug\OpenDriven.Framework35.Tests.dll");

      List<string> actualPaths = RunMultiProjectTestsCommand.ExtractPaths(text, out int numProjects);

      Assert.That(numProjects, Is.EqualTo(2), "Expected numProjects differs");
      Assert.That(actualPaths, Is.EqualTo(expectedPaths), "Expected paths differs");
    }


  }
}