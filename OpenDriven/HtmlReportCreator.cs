using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace OpenDriven
{
  /// <summary>
  /// Converts the nunit 2 xml files into a single easy to read html report that can open in Chrome, FireFox and IE.
  /// It uses some modified xsl files from various sources to convert into a junit format.
  /// junit-noframes.xsl had quite a few changes so it would render our test results correctly.
  /// 
  /// These were used:
  /// https://github.com/jenkinsci/nunit-plugin/blob/master/src/main/resources/hudson/plugins/nunit/nunit-to-junit.xsl
  /// https://raw.githubusercontent.com/apache/ant-antlibs-antunit/master/src/etc/junit-noframes.xsl (modified)
  /// </summary>
  public class HtmlReportCreator
  {
    /// <summary>
    /// Parses the supplied TestResults folder. It should contain all the "*_TestResult.xml" files that are output 
    /// from nunit console runner in v2 format. It outputs a single TestReport.html file you can open in your 
    /// preferred browser.
    /// </summary>
    /// <param name="folder">TestResults folder. It should contain all the "*_TestResult.xml" files that are output 
    /// from nunit console runner in v2 format.</param>
    public static void ParseUnitTestResultsFolder(string folder)
    {
      const string header = "<?xml version=\"1.0\" encoding=\"UTF-16\"?>\r\n<testsuites>\r\n";
      const string footer = "</testsuites>\r\n";

      string[] files = Directory.GetFiles(folder, "*.xml");
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append(header);
      string junitOutputFile = Path.Combine(folder, "junit-output.xml");
      string testOutputFile = Path.Combine(folder, "output.xml");

      //      foreach (string file in files)
      //      {
      MsXsl(testOutputFile);

        // We combine all the junit outputs into one big report.
        string junitOutput = File.ReadAllText(junitOutputFile);
        junitOutput = junitOutput.Replace(header, "");
        junitOutput = junitOutput.Replace(footer, "");
        // Some of the failure messages did not look very nice, so we replace them with nicer text so it is still 
        // compatible with our xsl files.
        junitOutput = junitOutput.Replace("<failure>\r\nMESSAGE:\r\n", "<failure>");
        junitOutput = junitOutput.Replace("+++++++++++++++++++\r\nSTACK TRACE:\r\n</failure>", "</failure>");
        junitOutput = junitOutput.Replace("\r\n\r\n+++++++++++++++++++\r\nSTACK TRACE:", "\r\n\r\nStack Trace:");
        junitOutput = junitOutput.Replace("+++++++++++++++++++\r\nSTACK TRACE:", "\r\nStack Trace:");
        stringBuilder.Append(junitOutput);
//      }
      stringBuilder.Append(footer);
      File.WriteAllText(junitOutputFile, stringBuilder.ToString(), Encoding.Unicode);

      string testReportPath = Path.Combine(folder, "TestReport.html");
      if (File.Exists(testReportPath))
      {
        File.Delete(testReportPath);
      }
      MsXslCombinedTestReport(junitOutputFile, testReportPath);
      FixTestReport(testReportPath);
 //     File.Copy(testReportPath, Path.Combine(folder, "TestReport.html"));
    }

    /// <summary>
    /// Parses the supplied TestResults folder. It should contain all the "*_TestResult.xml" files that are output 
    /// from nunit console runner in v2 format. It outputs a single TestReport.html file you can open in your 
    /// preferred browser.
    /// </summary>
    /// <param name="folder">TestResults folder. It should contain all the "*_TestResult.xml" files that are output 
    /// from nunit console runner in v2 format.</param>
    public static void ParseUnitTestResultsFolderStarXml(string folder)
    {
      const string header = "<?xml version=\"1.0\" encoding=\"UTF-16\"?>\r\n<testsuites>\r\n";
      const string footer = "</testsuites>\r\n";

      string[] files = Directory.GetFiles(folder, "*.xml");
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append(header);
      foreach (string file in files)
      {
        MsXsl(file);

        // We combine all the junit outputs into one big report.
        string junitOutput = File.ReadAllText("C:\\Program Files\\OpenDriven\\junit-output.xml");
        junitOutput = junitOutput.Replace(header, "");
        junitOutput = junitOutput.Replace(footer, "");
        // Some of the failure messages did not look very nice, so we replace them with nicer text so it is still 
        // compatible with our xsl files.
        junitOutput = junitOutput.Replace("<failure>\r\nMESSAGE:\r\n", "<failure>");
        junitOutput = junitOutput.Replace("+++++++++++++++++++\r\nSTACK TRACE:\r\n</failure>", "</failure>");
        junitOutput = junitOutput.Replace("\r\n\r\n+++++++++++++++++++\r\nSTACK TRACE:", "\r\n\r\nStack Trace:");
        junitOutput = junitOutput.Replace("+++++++++++++++++++\r\nSTACK TRACE:", "\r\nStack Trace:");
        stringBuilder.Append(junitOutput);
      }
      stringBuilder.Append(footer);
      File.WriteAllText("C:\\Program Files\\OpenDriven\\junit-output.xml", stringBuilder.ToString(), Encoding.Unicode);

      string testReportPath = Path.Combine("C:\\Program Files\\OpenDriven", "TestReport.html");
      if (File.Exists(testReportPath))
      {
        File.Delete(testReportPath);
      }
      MsXslCombinedTestReport("C:\\Program Files\\OpenDriven\\junit-output.xml", testReportPath);
      string multiPath = Path.Combine(folder, "TestReport.html");
      if (File.Exists (multiPath))
      {
        File.Delete(multiPath);
      }
      File.Copy(testReportPath, multiPath);
      FixTestReport(testReportPath);
      FixTestReport(multiPath);
    }

    private static void FixTestReport(string testReportPath)
    {
      File.WriteAllText(testReportPath, File.ReadAllText(testReportPath).Replace("verdana,", ""));
    }

    /// <summary>
    /// Uses microsofts command line xsl transformation tool to convert supplied nunit v2 xml format into a junit xml format.
    /// https://www.microsoft.com/en-au/download/details.aspx?id=21714
    /// </summary>
    /// <param name="testResultXml">Nunit v2 xml format xml to convert.</param>
    private static void MsXsl(string testResultXml)
    {
      ProcessStartInfo startInfo = new ProcessStartInfo();
      startInfo.FileName = @"C:\Program Files\OpenDriven\msxsl.exe";
      startInfo.WorkingDirectory = @"C:\Program Files\OpenDriven";
      startInfo.Arguments = $"\"{testResultXml}\" nunit2junit.xsl -o \"C:\\Program Files\\OpenDriven\\junit-output.xml\"";
      startInfo.CreateNoWindow = true;
      Console.WriteLine($"{startInfo.FileName} {startInfo.Arguments}");
      startInfo.UseShellExecute = false;
      startInfo.RedirectStandardOutput = true;
      using (Process process = Process.Start(startInfo))
      {
        using (StreamReader reader = process.StandardOutput)
        {
          string result = reader.ReadToEnd();
          Console.Write(result);
        }
      }
    }

    /// <summary>
    /// Uses microsofts command line xsl transformation tool to convert supplied junit xml format into a 
    /// TestReport.html html file that can open in Chrome, FireFox and IE.
    /// https://www.microsoft.com/en-au/download/details.aspx?id=21714
    /// </summary>
    /// <param name="testResultXml">Combined junit output xml to convert to html.</param>
    /// <param name="htmlTestReportPath">Html output test report file.</param>
    private static void MsXslCombinedTestReport(string testResultXml, string htmlTestReportPath)
    {
      ProcessStartInfo startInfo = new ProcessStartInfo();
      startInfo.FileName = @"C:\Program Files\OpenDriven\msxsl.exe";
      startInfo.WorkingDirectory = @"C:\Program Files\OpenDriven";
      startInfo.Arguments = $"\"{testResultXml}\" junit-noframes.xsl -o \"{htmlTestReportPath}\"";
      startInfo.CreateNoWindow = true;
      Console.WriteLine($"{startInfo.FileName} {startInfo.Arguments}");
      startInfo.UseShellExecute = false;
      startInfo.RedirectStandardOutput = true;
      using (Process process = Process.Start(startInfo))
      {
        using (StreamReader reader = process.StandardOutput)
        {
          string result = reader.ReadToEnd();
          Console.Write(result);
        }
      }
    }
  }
}
