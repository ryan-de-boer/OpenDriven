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
      for (int i=0;i<names.Count; i++)
      {
        sb.AppendLine(names[i]+":");
        sb.AppendLine(errors[i]);
      }
      string output = sb.ToString();

      ExtractFileLine(output, out string file, out int lineNumber);
      int v = 1;
      v++;
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

      string testName = text.Substring(text.LastIndexOf("[Test]"));
      testName = testName.Substring(testName.IndexOf("public void"));
      testName = testName.Substring("public void".Length);
      testName = testName.Substring(0, testName.IndexOf("\n")).Trim();
      testName = testName.Replace("()", "");

      return $"{namespaceText}.{className}.{testName}";
    }
  }
}
