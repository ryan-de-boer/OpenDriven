using NUnit.Engine.Addins;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace OpenDriven
{
  internal class RunTests
  {
    public static string Run(string fileName, string testWithNamespace, bool x86 = false)
    {
      System.Diagnostics.Process process;
      if (DebugTests.DotNetFramework(fileName))
      {
        string arguments = $"{fileName} /test={testWithNamespace} -result:\"C:\\Program Files\\OpenDriven\\output.xml\";format=nunit2";
        if (testWithNamespace == "_PROJECT_")
        {
          arguments = $"{fileName} -result:\"C:\\Program Files\\OpenDriven\\output.xml\";format=nunit2";
        }
        if (x86)
        {
          arguments += " --x86";
        }
        var processStartInfo = new ProcessStartInfo
        {
          FileName = @"C:\Program Files\OpenDriven\nunit-console-3.8\nunit3-console.exe",
          Arguments = arguments,
          WorkingDirectory = @"C:\Program Files\OpenDriven\nunit-console-3.8",
          RedirectStandardOutput = true,
          UseShellExecute = false,
          CreateNoWindow = true,
        };
        process = System.Diagnostics.Process.Start(processStartInfo);
      }
      else  //.net 5+ or netstandard
      {
        // Does not support nunit2 format.
        string filePath = Path.GetDirectoryName(fileName);
        string netFrameworkDll = Path.Combine(filePath, "nunit.framework.dll");
        if (!File.Exists(netFrameworkDll))
        {
          // For some reason this dependency dll does not get copied to output folder.
          File.Copy(@"C:\Program Files\OpenDriven\nunit-console-3.15.0\net6.0\nunit.framework.dll", netFrameworkDll);
        }

        string arguments = $"{fileName} /test={testWithNamespace} -result:\"C:\\Program Files\\OpenDriven\\outputv3.xml\"";
        if (testWithNamespace == "_PROJECT_")
        {
          arguments = $"{fileName} -result:\"C:\\Program Files\\OpenDriven\\outputv3.xml\"";
        }
        var processStartInfo = new ProcessStartInfo
        {
          FileName = @"C:\Program Files\OpenDriven\nunit-console-3.15.0\net6.0\nunit3-console.exe",
          Arguments = arguments,
          WorkingDirectory = @"C:\Program Files\OpenDriven\nunit-console-3.15.0\net6.0",
          RedirectStandardOutput = true,
          UseShellExecute = false,
          CreateNoWindow = true,
        };
        process = System.Diagnostics.Process.Start(processStartInfo);
      }

      string output = process.StandardOutput.ReadToEnd();
      process.WaitForExit();

      if (!DebugTests.DotNetFramework(fileName))
      {
        //.net 5+ or netstandard
        string outputv3 = "C:\\Program Files\\OpenDriven\\outputv3.xml";
        string outputv2 = "C:\\Program Files\\OpenDriven\\output.xml";
        var xmldoc = new XmlDataDocument();
        var fileStream
          = new FileStream(outputv3, FileMode.Open, FileAccess.Read);
        xmldoc.Load(fileStream);
        var xmlnode = xmldoc.GetElementsByTagName("test-run").Item(0);

        var writer = new NUnit2XmlResultWriter();
        writer.WriteResultFile(xmlnode, outputv2);
        fileStream.Close();
      }

      return output;
    }
  }
}
