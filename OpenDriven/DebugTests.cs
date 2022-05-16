using EnvDTE;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenDriven
{
  internal class DebugTests
  {
    //return true for >= dot net 5
    public static bool NewDotNet(string fileName)
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

    public static bool DotNetStandard(string fileName)
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

    public static bool DotNetFramework(string fileName)
    {
      string sub = fileName.Substring(fileName.IndexOf("bin\\Debug\\") + "bin\\Debug\\".Length);
      if (sub.Contains("\\"))
      {
        return false;
      }
      return true;
    }

    //https://docs.microsoft.com/en-us/dotnet/api/envdte.process.attach?view=visualstudiosdk-2022
    public static void Attach(DTE dte)
    {
      ThreadHelper.ThrowIfNotOnUIThread();
      EnvDTE.Processes processes = dte.Debugger.LocalProcesses;
      foreach (EnvDTE.Process proc in processes)
        if (proc.Name.IndexOf("nunit-agent.exe") != -1)
          proc.Attach();
    }

    public static void AttachConsole(DTE dte)
    {
      ThreadHelper.ThrowIfNotOnUIThread();
      EnvDTE.Processes processes = dte.Debugger.LocalProcesses;
      foreach (EnvDTE.Process proc in processes)
        if (proc.Name.IndexOf("nunit3-console.exe") != -1)
          proc.Attach();
    }

    public static void PreDebug()
    {
      if (File.Exists(@"C:\Program Files\OpenDriven\nunit-console-3.8\ReadyToAttach.txt"))
      {
        File.Delete(@"C:\Program Files\OpenDriven\nunit-console-3.8\ReadyToAttach.txt");
      }
      if (File.Exists(@"C:\Program Files\OpenDriven\nunit-console-3.15.0\net6.0\ReadyToAttach.txt"))
      {
        File.Delete(@"C:\Program Files\OpenDriven\nunit-console-3.15.0\net6.0\ReadyToAttach.txt");
      }
    }

    public static void Debug(string fileName, string testWithNamespace, DTE dte)
    {
      System.Diagnostics.Process process;
      if (DotNetFramework(fileName))
      {
        string arguments = $"{fileName} /test={testWithNamespace} --debug-agent";
        if (testWithNamespace=="_PROJECT_")
        {
          arguments = $"{fileName} --debug-agent";
        }
        System.Diagnostics.Process cmd = new System.Diagnostics.Process();
        cmd.StartInfo.FileName = @"C:\Program Files\OpenDriven\nunit-console-3.8\nunit3-console.exe";
        cmd.StartInfo.WorkingDirectory = @"C:\Program Files\OpenDriven\nunit-console-3.8";
        cmd.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
        cmd.StartInfo.CreateNoWindow = true;
        cmd.StartInfo.Arguments = arguments;
        cmd.Start();
      }
      else //.net 5+ or netstandard
      {
        // Does not support nunit2 format.
        string filePath = Path.GetDirectoryName(fileName);
        string netFrameworkDll = Path.Combine(filePath, "nunit.framework.dll");
        if (!File.Exists(netFrameworkDll))
        {
          // For some reason this dependency dll does not get copied to output folder.
          File.Copy(@"C:\Program Files\OpenDriven\nunit-console-3.15.0\net6.0\nunit.framework.dll", netFrameworkDll);
        }

        string arguments = $"{fileName} /test={testWithNamespace} --debug-agent";
        if (testWithNamespace == "_PROJECT_")
        {
          arguments = $"{fileName} --debug-agent";
        }
        System.Diagnostics.Process cmd = new System.Diagnostics.Process();
        cmd.StartInfo.FileName = @"C:\Program Files\OpenDriven\nunit-console-3.15.0\net6.0\nunit3-console.exe";
        cmd.StartInfo.WorkingDirectory = @"C:\Program Files\OpenDriven\nunit-console-3.15.0\net6.0";
        cmd.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
        cmd.StartInfo.CreateNoWindow = true;
        cmd.StartInfo.Arguments = arguments;
        cmd.Start();
      }

      if (DotNetFramework(fileName))
      {
        while (!File.Exists(@"C:\Program Files\OpenDriven\nunit-console-3.8\ReadyToAttach.txt"))
        {
          System.Threading.Thread.Sleep(500);
        }

        Attach(dte);

        File.Delete(@"C:\Program Files\OpenDriven\nunit-console-3.8\ReadyToAttach.txt");
      }
      else
      {
        //.net 5+ or netstandard
        while (!File.Exists(@"C:\Program Files\OpenDriven\nunit-console-3.15.0\net6.0\ReadyToAttach.txt"))
        {
          System.Threading.Thread.Sleep(500);
        }

        AttachConsole(dte);

        File.Delete(@"C:\Program Files\OpenDriven\nunit-console-3.15.0\net6.0\ReadyToAttach.txt");
      }
    }
  }
}
