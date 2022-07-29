using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenDriven.Commands
{
  /// <summary>
  /// Command handler
  /// </summary>
  public sealed class RunMultiProjectTestsCommand
  {
    /// <summary>
    /// Command ID.
    /// </summary>
    public const int CommandId = 4133;

    /// <summary>
    /// Command menu group (command set GUID).
    /// </summary>
    public static readonly Guid CommandSet = new Guid("23807277-b10c-4815-af55-28c7a85ddc34");

    /// <summary>
    /// VS Package that provides this command, not null.
    /// </summary>
    private readonly AsyncPackage package;
    private static AsyncPackage s_package;

    /// <summary>
    /// Initializes a new instance of the <see cref="RunMultiProjectTestsCommand"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    /// <param name="commandService">Command service to add command to, not null.</param>
    private RunMultiProjectTestsCommand(AsyncPackage package, OleMenuCommandService commandService)
    {
      this.package = package ?? throw new ArgumentNullException(nameof(package));
      commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

      var menuCommandID = new CommandID(CommandSet, CommandId);
      var menuItem = new MenuCommand(this.Execute, menuCommandID);
      commandService.AddCommand(menuItem);
    }

    /// <summary>
    /// Gets the instance of the command.
    /// </summary>
    public static RunMultiProjectTestsCommand Instance
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
      s_package = package;
      // Switch to the main thread - the call to AddCommand in RunMultiProjectTestsCommand's constructor requires
      // the UI thread.
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

      OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
      Instance = new RunMultiProjectTestsCommand(package, commandService);
    }

    public static List<string> ExtractPaths(string text, out int numProjects)
    {
      List<string> paths = new List<string>();
      numProjects = 0;
      string[] lines = text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
      foreach (string line in lines)
      {
        string[] tokens = line.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
        paths.Add(tokens[0]);
        if (tokens[1] == "_PROJECT_")
        {
          numProjects++;
        }
      }
      return paths;
    }

    public static void Run(List<string> projectPaths)
    {
      Dictionary<string, string> pathToName = new Dictionary<string, string>();
      List<EnvDTE.Project> projects = RunSolutionTestsCommand.GetProjects(DebugTestsCommand.s_dte.Solution);
      foreach (EnvDTE.Project project in projects)
      {
        if (project.FileName.Trim() == "" || project.Kind == "{66A26720-8FB5-11D2-AA7E-00C04F688DDE}") //solution folders
        {
          int c = 1;
          c++;
        }
        else
        {
          string pOutfileName = DebugTestsCommand.GetAssemblyPath(project);
          if (projectPaths.Contains(pOutfileName))
          {
            pathToName[pOutfileName] = project.Name;
            DebugTestsCommand.Build(project);
          }
        }
      }

      string multiDir = "C:\\Program Files\\OpenDriven\\Multi";
      if (Directory.Exists(multiDir))
      {
        Directory.Delete(multiDir, true);
      }

      Window window = DebugTestsCommand.s_dte.Windows.Item(EnvDTE.Constants.vsWindowKindOutput);
      OutputWindow outputWindow = (OutputWindow)window.Object;
      foreach (EnvDTE.OutputWindowPane x in outputWindow.OutputWindowPanes)
      {
        if (x.Name == "Test Output")
        {
          x.Activate();
          x.Clear();
          break;
        }
      }

      bool failed = true;
      for (int i = 0; i < projectPaths.Count; i++)
      {
        string output = RunTests.Run(projectPaths[i], "_PROJECT_");

        if (output.Contains("Failed: 0,") && output.Contains("Overall result: Passed"))
        {
          failed = false;
        }

        if (!Directory.Exists(multiDir))
        {
          Directory.CreateDirectory(multiDir);
        }
        string outputFile = "C:\\Program Files\\OpenDriven\\output.xml";
        string destFile = Path.Combine(multiDir, pathToName[projectPaths[i]] + ".xml");
        File.Copy(outputFile, destFile);

        EnvDTE.OutputWindowPane owp;
        bool found = false;
        foreach (EnvDTE.OutputWindowPane x in outputWindow.OutputWindowPanes)
        {
          if (x.Name == "Test Output")
          {
            x.Activate();
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
      }

      HtmlReportCreator.ParseUnitTestResultsFolderStarXml("C:\\Program Files\\OpenDriven\\Multi");

      if (!failed)
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
        dialog.ErrorTextBox.Text = RunTestsCommand.GetError();
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
      string title = "RunMultiProjectTestsCommand";

      StringBuilder sb = new StringBuilder();
      List<string> projectPaths = new List<string>();
      List<string> names = new List<string>();
      Array _projects = DebugTestsCommand.s_dte.ActiveSolutionProjects as Array;
      if (_projects.Length != 0 && _projects != null)
      {
        File.WriteAllText(@"C:\Program Files\OpenDriven\LastRunTest.txt", $"");
        foreach (EnvDTE.Project project in _projects)
        {
          string projectPath = DebugTestsCommand.GetAssemblyPath(project);
          projectPaths.Add(projectPath);
          names.Add(project.Name);
          File.AppendAllText(@"C:\Program Files\OpenDriven\LastRunTest.txt", $"{projectPath}|_PROJECT_\r\n");
        }

        foreach (EnvDTE.Project project in _projects)
        {
          if (!DebugTestsCommand.Build(project))
          {
            VsShellUtilities.ShowMessageBox(
              this.package,
              DebugTestsCommand.BuildErrorMessage,
              DebugTestsCommand.BuildErrorTitle,
              DebugTestsCommand.BuildErrorIcon,
              OLEMSGBUTTON.OLEMSGBUTTON_OK,
              OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            return;
          }
          //DebugTestsCommand.Build(project);
        }
      }

      string multiDir = "C:\\Program Files\\OpenDriven\\Multi";
      if (Directory.Exists(multiDir))
      {
        Directory.Delete(multiDir, true);
      }


      Window window = DebugTestsCommand.s_dte.Windows.Item(EnvDTE.Constants.vsWindowKindOutput);
      OutputWindow outputWindow = (OutputWindow)window.Object;
      foreach (EnvDTE.OutputWindowPane x in outputWindow.OutputWindowPanes)
      {
        if (x.Name == "Test Output")
        {
          x.Activate();
          x.Clear();
          break;
        }
      }

      bool failed = true;
      for (int i = 0; i < projectPaths.Count; i++)
      {
        string output = RunTests.Run(projectPaths[i], "_PROJECT_");

        if (output.Contains("Failed: 0,") && output.Contains("Overall result: Passed"))
        {
          failed = false;
        }

        if (!Directory.Exists(multiDir))
        {
          Directory.CreateDirectory(multiDir);
        }
        string outputFile = "C:\\Program Files\\OpenDriven\\output.xml";
        string destFile = Path.Combine(multiDir, names[i] + ".xml");
        File.Copy(outputFile, destFile);

        EnvDTE.OutputWindowPane owp;
        bool found = false;
        foreach (EnvDTE.OutputWindowPane x in outputWindow.OutputWindowPanes)
        {
          if (x.Name == "Test Output")
          {
            x.Activate();
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
      }

      HtmlReportCreator.ParseUnitTestResultsFolderStarXml("C:\\Program Files\\OpenDriven\\Multi");

      if (!failed)
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
        dialog.ErrorTextBox.Text = RunTestsCommand.GetError();
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


      // Show a message box to prove we were here
    //  VsShellUtilities.ShowMessageBox(
    //      this.package,
    //      message+sb.ToString(),
    //      title,
    //      OLEMSGICON.OLEMSGICON_INFO,
    //      OLEMSGBUTTON.OLEMSGBUTTON_OK,
    //      OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
    }


    public const string guidOpenDrivenPackageCmdSet = "c5bccf32-96d1-4e8a-93b2-a9c56ea803d9";
    public static bool ChangeMyCommand(int cmdID, bool enableCmd)
    {
      bool cmdUpdated = false;
      //      System.IServiceProvider serviceProvider = package as System.IServiceProvider;
      System.IServiceProvider serviceProvider = s_package as System.IServiceProvider;
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

  }
}
