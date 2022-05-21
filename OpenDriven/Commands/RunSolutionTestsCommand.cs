using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Project = EnvDTE.Project;

namespace OpenDriven.Commands
{
  /// <summary>
  /// Command handler
  /// </summary>
  internal sealed class RunSolutionTestsCommand
  {
    /// <summary>
    /// Command ID.
    /// </summary>
    public const int CommandId = 4132;

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
    /// Initializes a new instance of the <see cref="RunSolutionTestsCommand"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    /// <param name="commandService">Command service to add command to, not null.</param>
    private RunSolutionTestsCommand(AsyncPackage package, OleMenuCommandService commandService)
    {
      this.package = package ?? throw new ArgumentNullException(nameof(package));
      s_package = package;
      commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

      var menuCommandID = new CommandID(CommandSet, CommandId);
      var menuItem = new MenuCommand(this.Execute, menuCommandID);
      commandService.AddCommand(menuItem);
    }

    /// <summary>
    /// Gets the instance of the command.
    /// </summary>
    public static RunSolutionTestsCommand Instance
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
      // Switch to the main thread - the call to AddCommand in RunSolutionTestsCommand's constructor requires
      // the UI thread.
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

      OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
      Instance = new RunSolutionTestsCommand(package, commandService);
    }

    public static void BuildSln()
    {
      ThreadHelper.ThrowIfNotOnUIThread();
      Solution2 soln = (Solution2)DebugTestsCommand.s_dte.Solution;
      SolutionBuild2 sb = (SolutionBuild2)soln.SolutionBuild;
      BuildDependencies bld = sb.BuildDependencies;

      Window window = DebugTestsCommand.s_dte.Windows.Item(EnvDTE.Constants.vsWindowKindOutput);
      OutputWindow outputWindow = (OutputWindow)window.Object;

      EnvDTE.OutputWindowPane owp;
      bool found = false;
      foreach (EnvDTE.OutputWindowPane x in outputWindow.OutputWindowPanes)
      {
        if (x.Name == "Build")
        {
          x.Activate();
          x.Clear();
          x.OutputString("Building the solution in debug mode...");
          found = true;
          break;
        }
      }
      if (!found)
      {
        owp = outputWindow.OutputWindowPanes.Add("Build");
        owp.OutputString("Building the solution in debug mode...");
      }
      sb.Build(true);
    }

    //https://stackoverflow.com/questions/39126032/how-do-i-list-all-the-projects-in-the-current-solution-using-envdte

    /// <summary>
    /// Queries for all projects in solution, recursively (without recursion)
    /// </summary>
    /// <param name="sln">Solution</param>
    /// <returns>List of projects</returns>
    public static List<Project> GetProjects(EnvDTE.Solution sln)
    {
      ThreadHelper.ThrowIfNotOnUIThread();
      List<Project> list = new List<Project>();
      list.AddRange(sln.Projects.Cast<Project>());

      for (int i = 0; i < list.Count; i++)
        // OfType will ignore null's.
        list.AddRange(list[i].ProjectItems.Cast<ProjectItem>().Select(x =>
        {
          Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
          return x.SubProject;
        }).OfType<Project>());

      return list;
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

    public static void Run()
    {
      System.Diagnostics.Trace.WriteLine($"RunSolutionTestsCommand Run");
      BuildSln();
      System.Diagnostics.Trace.WriteLine($"RunSolutionTestsCommand 2");

      var projects = GetProjects(DebugTestsCommand.s_dte.Solution);
      System.Diagnostics.Trace.WriteLine($"RunSolutionTestsCommand 3");

      StringBuilder sb = new StringBuilder();
      List<string> fileNames = new List<string>();
      List<string> assemblyPaths = new List<string>();
      List<string> names = new List<string>();
      foreach (var p in projects)
      {
        if (p.FileName.Trim() == "" || p.Kind == "{66A26720-8FB5-11D2-AA7E-00C04F688DDE}") //solution folders
        {
          int c = 1;
          c++;
        }
        else
        {
          System.Diagnostics.Trace.WriteLine($"RunSolutionTestsCommand 3.1 {p.FileName}");
          string contents = File.ReadAllText(p.FileName);
          System.Diagnostics.Trace.WriteLine($"RunSolutionTestsCommand 3.2");
          if (contents.Contains("Reference Include=\"nunit.framework") || contents.Contains("PackageReference Include=\"NUnit\""))
          {
            string assemblyPath = DebugTestsCommand.GetAssemblyPath(p);

            sb.AppendLine(p.FileName);
            sb.AppendLine(assemblyPath);
            sb.AppendLine(p.FullName);

            fileNames.Add(p.FileName);
            assemblyPaths.Add(assemblyPath);
            names.Add(p.Name);
          }

        }
      }
      System.Diagnostics.Trace.WriteLine($"RunSolutionTestsCommand 4");

      Solution2 soln = (Solution2)DebugTestsCommand.s_dte.Solution;

      File.WriteAllText(@"C:\Program Files\OpenDriven\LastRunTest.txt", $"{soln.FileName}|_SOLUTION_");

      string multiDir = "C:\\Program Files\\OpenDriven\\Multi";
      if (Directory.Exists(multiDir))
      {
        Directory.Delete(multiDir, true);
      }
      System.Diagnostics.Trace.WriteLine($"RunSolutionTestsCommand 5");

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
      System.Diagnostics.Trace.WriteLine($"RunSolutionTestsCommand 6");

      bool failed = true;
      for (int i = 0; i < assemblyPaths.Count; i++)
      {
        string output = RunTests.Run(assemblyPaths[i], "_PROJECT_");

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

      System.Diagnostics.Trace.WriteLine($"RunSolutionTestsCommand 7");

      HtmlReportCreator.ParseUnitTestResultsFolderStarXml("C:\\Program Files\\OpenDriven\\Multi");
      System.Diagnostics.Trace.WriteLine($"RunSolutionTestsCommand 8");

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
      System.Diagnostics.Trace.WriteLine($"RunSolutionTestsCommand 9");

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
      string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()\r\n", this.GetType().FullName);
      string title = "RunSolutionTestsCommand";

      Run();

      /*
      // Show a message box to prove we were here
      VsShellUtilities.ShowMessageBox(
          this.package,
          message+sb.ToString(),
          title,
          OLEMSGICON.OLEMSGICON_INFO,
          OLEMSGBUTTON.OLEMSGBUTTON_OK,
          OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
      */
    }
  }
}
