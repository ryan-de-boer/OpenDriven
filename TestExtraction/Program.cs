using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestExtraction
{
  internal class Program
  {
    static void Main(string[] args)
    {
      string text = File.ReadAllText(@"..\..\Text.txt");

      string expectedNamespaceTest = "Hardware.Graphics.Tests.UnitTests.GfxUnitTests.TestCreateGraphicsFromJson";

      string actualNamespaceTest = ExtractNamespaceTest(text);

      if (actualNamespaceTest != expectedNamespaceTest)
      {
        MessageBox.Show("differs");
      }
      int a = 1;
      a++;
    }

    static string ExtractNamespaceTest(string text)
    {
      string namespaceText = text.Substring(text.IndexOf("namespace "));
      namespaceText = namespaceText.Substring("namespace ".Length);
      namespaceText = namespaceText.Substring(0, namespaceText.IndexOf("\n")).Trim();

      string className = text.Substring(text.IndexOf("public class"));
      className = className.Substring("public class".Length);
      className = className.Substring(0, className.IndexOf("\n")).Trim();

      string testName = text.Substring(text.LastIndexOf("[Test]"));
      testName = testName.Substring(testName.IndexOf("public void"));
      testName = testName.Substring("public void".Length);
      testName = testName.Substring(0, testName.IndexOf("\n")).Trim();
      testName = testName.Replace("()", "");

      return $"{namespaceText}.{className}.{testName}";
    }
  }
}
