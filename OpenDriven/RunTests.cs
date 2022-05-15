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
    public static string Run(string fileName, string testWithNamespace)
    {
      System.Diagnostics.Process process;
      if (fileName.Contains("net6.0") || fileName.Contains("net5.0") || fileName.Contains("netstandard")) // eg netstandard2.1
      {
        // Does not support nunit2 format.
        string filePath = Path.GetDirectoryName(fileName);
        string netFrameworkDll = Path.Combine(filePath, "nunit.framework.dll");
        if (!File.Exists(netFrameworkDll))
        {
          // For some reason this dependency dll does not get copied to output folder.
          File.Copy(@"C:\Program Files\OpenDriven\nunit-console-3.15.0\net6.0\nunit.framework.dll", netFrameworkDll);
        }

        var processStartInfo = new ProcessStartInfo
        {
          FileName = @"C:\Program Files\OpenDriven\nunit-console-3.15.0\net6.0\nunit3-console.exe",
          Arguments = $"{fileName} /test={testWithNamespace} -result:\"C:\\Program Files\\OpenDriven\\outputv3.xml\"",
          WorkingDirectory = @"C:\Program Files\OpenDriven\nunit-console-3.15.0\net6.0",
          RedirectStandardOutput = true,
          UseShellExecute = false,
          CreateNoWindow = true,
        };
        process = System.Diagnostics.Process.Start(processStartInfo);
      }
      else
      {
        var processStartInfo = new ProcessStartInfo
        {
          FileName = @"C:\Program Files\OpenDriven\nunit-console-3.8\nunit3-console.exe",
          Arguments = $"{fileName} /test={testWithNamespace} -result:\"C:\\Program Files\\OpenDriven\\output.xml\";format=nunit2",
          WorkingDirectory = @"C:\Program Files\OpenDriven\nunit-console-3.8",
          RedirectStandardOutput = true,
          UseShellExecute = false,
          CreateNoWindow = true,
        };
        process = System.Diagnostics.Process.Start(processStartInfo);
      }

      string output = process.StandardOutput.ReadToEnd();
      process.WaitForExit();

      if (fileName.Contains("net6.0") || fileName.Contains("net5.0") || fileName.Contains("netstandard")) // eg netstandard2.1
      {
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
