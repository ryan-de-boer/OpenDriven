using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace TestExtraction
{
  internal class Program
  {
    static void Main(string[] args)
    {
      //     string "D:\\code\\other\\OpenDriven\\git\\OpenDriven\\OpenDriven.5.Tests\\bin\\Debug\\net5.0\\OpenDriven.5.Tests.dll"

      /*
      string dn5 = "D:\\code\\other\\OpenDriven\\git\\OpenDriven\\OpenDriven.5.Tests\\bin\\Debug\\net5.0\\OpenDriven.5.Tests.dll";
      string framwork = "D:\\code\\other\\OpenDriven\\git\\OpenDriven\\OpenDriven.Framework48.Tests\\bin\\Debug\\OpenDriven.Framework48.Tests.dll";
      string netstandard = "D:\\code\\other\\OpenDriven\\git\\OpenDriven\\OpenDriven.Standard21.Tests\\bin\\Debug\\netstandard2.1\\OpenDriven.Standard21.Tests.dll";

      bool n = NewDotNet(dn5);
      bool n6 = NewDotNet("D:\\code\\other\\OpenDriven\\git\\OpenDriven\\OpenDriven.Tests\\bin\\Debug\\net6.0\\OpenDriven.Tests.dll");
      bool n7 = NewDotNet("D:\\code\\other\\OpenDriven\\git\\OpenDriven\\OpenDriven.Tests\\bin\\Debug\\net7.0\\OpenDriven.Tests.dll");
      bool s = DotNetStandard(netstandard);
      bool s2 = DotNetStandard(dn5);
      bool f = DotNetFramework(framwork);
      bool f2 = NewDotNet(framwork);
      */

      TestExtraction();

      int v = 1;
      v++;
    }

    //return true for >= dot net 5
    static bool NewDotNet(string fileName)
    {
      string sub = fileName.Substring(fileName.IndexOf("bin\\Debug\\") + "bin\\Debug\\".Length);
      if (!sub.Contains("\\"))
      {
        return false;
      }
      sub = sub.Substring(0, sub.IndexOf("\\"));
      if (!sub.StartsWith("net"))
      {
        return false;
      }
      string ver = sub.Substring("net".Length);
      decimal.TryParse(ver, out decimal result);
      return result >= new decimal(5.0);
    }

    static bool DotNetStandard(string fileName)
    {
      string sub = fileName.Substring(fileName.IndexOf("bin\\Debug\\") + "bin\\Debug\\".Length);
      if (!sub.Contains("\\"))
      {
        return false;
      }
      sub = sub.Substring(0, sub.IndexOf("\\"));
      if (!sub.StartsWith("netstandard"))
      {
        return false;
      }
      return true;
    }

    static bool DotNetFramework(string fileName)
    {
      string sub = fileName.Substring(fileName.IndexOf("bin\\Debug\\") + "bin\\Debug\\".Length);
      if (sub.Contains("\\"))
      {
        return false;
      }
      return true;
    }

    static void TestExtractNamespaceFolder()
    {
      string text = File.ReadAllText(@"..\..\Text.txt");

      string expectedNamespaceTest = "Hardware.Graphics.Tests.UnitTests";

      string actualNamespaceTest = ExtractNamespaceFolder(text, "UnitTests");

      if (actualNamespaceTest != expectedNamespaceTest)
      {
        MessageBox.Show("differs");
      }

    }

    static string ExtractNamespaceFolder(string text, string folder)
    {
      string endNamespace = "." + folder;
      string namespaceText = text.Substring(text.IndexOf("namespace "));
      namespaceText = namespaceText.Substring("namespace ".Length);
      namespaceText = namespaceText.Substring(0, namespaceText.LastIndexOf(endNamespace)+ endNamespace.Length).Trim();

      return namespaceText;
    }


    static void TestExtractFileLine()
    {
      XmlDocument xmlDoc = new XmlDocument();
      xmlDoc.Load(@"C:\Program Files\OpenDriven\junit-output.xml");

      List<string> names = new List<string>();
      List<string> errors = new List<string>();
      foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
      {
        if (node.Name == "testsuite")
        {
          foreach (XmlNode childNode in node.ChildNodes)
          {
            if (childNode.Name == "testcase")
            {
              string name = childNode.Attributes["name"].Value;
              foreach (XmlNode childNode2 in childNode.ChildNodes)
              {
                if (childNode2.Name == "failure")
                {
                  string error = childNode2.InnerText;
                  names.Add(name);
                  errors.Add(error);
                }
              }

              int b = 1;
              b++;
            }
          }

        }
        int a = 1;
        a++;
      }

      StringBuilder sb = new StringBuilder();
      for (int i = 0; i < names.Count; i++)
      {
        sb.AppendLine(names[i] + ":");
        sb.AppendLine(errors[i]);
      }
      string output = sb.ToString();

      ExtractFileLine(output, out string file, out int lineNumber);
    }

      static void ExtractFileLine(string output, out string file, out int lineNumber)
    {
      int lastLineIndex = output.LastIndexOf(":line");
      string subs = output.Substring(0, lastLineIndex);
      file = subs.Substring(subs.LastIndexOf("in ") + "in ".Length).Trim();
      string strLine = output.Substring(lastLineIndex + ":line ".Length);
      lineNumber = int.Parse(strLine);

        }

    static void TestExtraction()
    {
      string text = File.ReadAllText(@"..\..\Text.txt");

      string expectedNamespaceTest = "Hardware.Graphics.Tests.UnitTests.GfxUnitTests.TestCreateGraphicsFromJson";

      string actualNamespaceTest = ExtractNamespaceTest(text);

      if (actualNamespaceTest != expectedNamespaceTest)
      {
        MessageBox.Show("differs");
      }
    }

    static string ExtractNamespaceTest(string text)
    {
      string namespaceText = text.Substring(text.IndexOf("namespace "));
      namespaceText = namespaceText.Substring("namespace ".Length);
      namespaceText = namespaceText.Substring(0, namespaceText.IndexOf("\n")).Trim();

      string className = text.Substring(text.IndexOf("public class"));
      className = className.Substring("public class".Length);
      className = className.Substring(0, className.IndexOf("\n")).Trim();
      if (className.IndexOf(":") != -1)
      {
        className = className.Substring(0, className.IndexOf(":")).Trim();
      }

      string testName = text.Substring(text.LastIndexOf("[Test]"));
      testName = testName.Substring(testName.IndexOf("public void"));
      testName = testName.Substring("public void".Length);
      testName = testName.Substring(0, testName.IndexOf("\n")).Trim();
      testName = testName.Replace("()", "");

      return $"{namespaceText}.{className}.{testName}";
    }
  }
}
