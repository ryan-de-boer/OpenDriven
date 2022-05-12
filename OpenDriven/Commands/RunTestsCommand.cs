using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using NUnit.Engine.Addins;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace OpenDriven.Commands
{
  /// <summary>
  /// Command handler
  /// </summary>
  internal sealed class RunTestsCommand
  {
    /// <summary>
    /// Command ID.
    /// </summary>
    public const int CommandId = 256;

    /// <summary>
    /// Command menu group (command set GUID).
    /// </summary>
    public static readonly Guid CommandSet = new Guid("c5bccf32-96d1-4e8a-93b2-a9c56ea803d9");

    /// <summary>
    /// VS Package that provides this command, not null.
    /// </summary>
    private readonly AsyncPackage package;

    /// <summary>
    /// Initializes a new instance of the <see cref="RunTestsCommand"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    /// <param name="commandService">Command service to add command to, not null.</param>
    private RunTestsCommand(AsyncPackage package, OleMenuCommandService commandService)
    {
      this.package = package ?? throw new ArgumentNullException(nameof(package));
      commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

      var menuCommandID = new CommandID(CommandSet, CommandId);
      var menuItem = new MenuCommand(this.Execute, menuCommandID);
      commandService.AddCommand(menuItem);

      //ChangeMyCommand(4129, true);
      //ChangeMyCommand(4177, false);
    }

    /// <summary>
    /// Gets the instance of the command.
    /// </summary>
    public static RunTestsCommand Instance
    {
      get;
      private set;
    }

    /// <summary>
    /// Gets the service provider from the owner package.
    /// </summary>
    private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
    {
      get
      {
        return this.package;
      }
    }

    /// <summary>
    /// Initializes the singleton instance of the command.
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    public static async Task InitializeAsync(AsyncPackage package)
    {
      // Switch to the main thread - the call to AddCommand in Command1's constructor requires
      // the UI thread.
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

      OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
      Instance = new RunTestsCommand(package, commandService);
    }

    public static EnvDTE.DTE s_dte;

    public static string GetError()
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
      return output;
    }

    /// <summary>
    /// This function is the callback used to execute the command when the menu item is clicked.
    /// See the constructor to see how the menu item is associated with this function using
    /// OleMenuCommandService service and MenuCommand class.
    /// </summary>
    /// <param name="sender">Event sender.</param>
    /// <param name="e">Event args.</param>
    private void Execute(object sender, EventArgs e)
    {
      ThreadHelper.ThrowIfNotOnUIThread();
      string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
      string title = "Command1";

      Track.TrackFile();
      EnvDTE.Project _selectedProject1 = null;
      string fileName = "";
      Array _projects = s_dte.ActiveSolutionProjects as Array;
      if (_projects.Length != 0 && _projects != null)
      {
        EnvDTE.Project _selectedProject = _projects.GetValue(0) as EnvDTE.Project;
        _selectedProject1 = _selectedProject;
        //get the project path

        fileName = DebugTestsCommand.GetAssemblyPath(_selectedProject);

      }

      //     string testWithNamespace = DebugCommand.InputBox("Test with namespace", "");

      var activePoint = ((EnvDTE.TextSelection)s_dte.ActiveDocument.Selection).ActivePoint;
      string text2 = activePoint.CreateEditPoint().GetLines(1, activePoint.Line + 2);

      string testWithNamespace = DebugTestsCommand.ExtractNamespaceTest(text2);

      File.WriteAllText(@"C:\Program Files\OpenDriven\LastRunTest.txt", $"{fileName}|{testWithNamespace}");

      DebugTestsCommand.Build(_selectedProject1);

      System.Diagnostics.Process process;
      if (fileName.Contains("net6.0"))
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

      if (fileName.Contains("net6.0"))
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
      }

      Window window = s_dte.Windows.Item(EnvDTE.Constants.vsWindowKindOutput);
      OutputWindow outputWindow = (OutputWindow)window.Object;
      //      outputWindow.ActivePane.Activate();
      //      outputWindow.ActivePane.OutputString(output);
      EnvDTE.OutputWindowPane owp;
      bool found = false;
      foreach (EnvDTE.OutputWindowPane x in outputWindow.OutputWindowPanes)
      {
        if (x.Name == "Test Output")
        {
          x.Activate();
          x.Clear();
          x.OutputString(output);
          found = true;
          break;
        }
      }
      if (!found)
      {
        owp = outputWindow.OutputWindowPanes.Add("Test Output");
        owp.OutputString(output);
      }

      HtmlReportCreator.ParseUnitTestResultsFolder("C:\\Program Files\\OpenDriven");

      if (output.Contains("Failed: 0,") && output.Contains("Overall result: Passed"))
      {
        File.WriteAllText(@"C:\Program Files\OpenDriven\LastRunTestResult.txt", "PASS");

        ChangeMyCommand(4129, true);
        ChangeMyCommand(4177, false);

        PassDialog dialog = new PassDialog();
        dialog.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        dialog.Show();

        //VsShellUtilities.ShowMessageBox(
        //  this.package,
        //  "PASS",
        //  "Test Result",
        //  OLEMSGICON.OLEMSGICON_INFO,
        //  OLEMSGBUTTON.OLEMSGBUTTON_OK,
        //  OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
      }
      else
      {
        File.WriteAllText(@"C:\Program Files\OpenDriven\LastRunTestResult.txt", "FAIL");

        ChangeMyCommand(4129, false);
        ChangeMyCommand(4177, true);



        FailDialog dialog = new FailDialog();
        dialog.ErrorTextBox.Text = GetError();
        dialog.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        dialog.Show();

        //VsShellUtilities.ShowMessageBox(
        //  this.package,
        //  "FAIL",
        //  "Test Result",
        //  OLEMSGICON.OLEMSGICON_CRITICAL,
        //  OLEMSGBUTTON.OLEMSGBUTTON_OK,
        //  OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
      }

      int a = 1;
      a++;

      //// Show a message box to prove we were here
      //VsShellUtilities.ShowMessageBox(
      //    this.package,
      //    message,
      //    title,
      //    OLEMSGICON.OLEMSGICON_INFO,
      //    OLEMSGBUTTON.OLEMSGBUTTON_OK,
      //    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
    }




    public const string guidOpenDrivenPackageCmdSet = "c5bccf32-96d1-4e8a-93b2-a9c56ea803d9";
    public bool ChangeMyCommand(int cmdID, bool enableCmd)
    {
      bool cmdUpdated = false;
      System.IServiceProvider serviceProvider = package as System.IServiceProvider;
      OleMenuCommandService mcs = (OleMenuCommandService)serviceProvider.GetService(typeof(IMenuCommandService));
      var newCmdID = new CommandID(new Guid(guidOpenDrivenPackageCmdSet), cmdID);
      MenuCommand mc = mcs.FindCommand(newCmdID);
      if (mc != null)
      {
        //mc.CommandChanged += Mc_CommandChanged;
        //        mc.Enabled = enableCmd;
        //        mc.Visible = enableCmd;
        mc.Visible = enableCmd;
        cmdUpdated = true;
      }
      return cmdUpdated;
    }

    //private void Mc_CommandChanged(object sender, EventArgs e)
    //{
    //  var myCommand = sender as MenuCommand;
    //  myCommand.Visible = false;

    //}
  }

}

