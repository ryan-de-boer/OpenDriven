using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OpenDriven.Commands
{
  /// <summary>
  /// Command handler
  /// </summary>
  internal sealed class RunFolderTestsCommand
  {
    /// <summary>
    /// Command ID.
    /// </summary>
    public const int CommandId = 4130;

    /// <summary>
    /// Command menu group (command set GUID).
    /// </summary>
    public static readonly Guid CommandSet = new Guid("23807277-b10c-4815-af55-28c7a85ddc34");

    /// <summary>
    /// VS Package that provides this command, not null.
    /// </summary>
    private readonly AsyncPackage package;

    /// <summary>
    /// Initializes a new instance of the <see cref="RunFolderTestsCommand"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    /// <param name="commandService">Command service to add command to, not null.</param>
    private RunFolderTestsCommand(AsyncPackage package, OleMenuCommandService commandService)
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
    public static RunFolderTestsCommand Instance
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
      // Switch to the main thread - the call to AddCommand in RunFolderTestsCommand's constructor requires
      // the UI thread.
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

      OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
      Instance = new RunFolderTestsCommand(package, commandService);
    }

    public static EnvDTE.UIHierarchyItem GetSelectedSolutionExplorerItem()
    {
      EnvDTE.UIHierarchy solutionExplorer = DebugTestsCommand.s_dte2.ToolWindows.SolutionExplorer;
      object[] items = solutionExplorer.SelectedItems as object[];
      if (items.Length != 1)
        return null;
      return items[0] as EnvDTE.UIHierarchyItem;
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
      string title = "RunFolderTestsCommand";

      string folderName = GetSelectedSolutionExplorerItem().Name;

      string file = "";
      foreach (EnvDTE.UIHierarchyItem i in GetSelectedSolutionExplorerItem().UIHierarchyItems)
      {
        EnvDTE.ProjectItem projectItem = i.Object as EnvDTE.ProjectItem;

        if (projectItem != null)
        {
          file =  (string)projectItem.Properties.Item("FullPath").Value;
          break;
          //VSLangProj.VSProjectItem vsProjectItem = projectItem.Object as VSLangProj.VSProjectItem;
          //if (vsProjectItem != null)
          //{
          //  file =  vsProjectItem.ProjectItem.Document.FullName;
          //  break;
          //}
        }


        break;
      }

      Track.TrackFile();
      EnvDTE.Project _selectedProject1 = null;
      string fileName = "";
      Array _projects = DebugTestsCommand.s_dte.ActiveSolutionProjects as Array;
      if (_projects.Length != 0 && _projects != null)
      {
        EnvDTE.Project _selectedProject = _projects.GetValue(0) as EnvDTE.Project;
        _selectedProject1 = _selectedProject;
        //get the project path

        fileName = DebugTestsCommand.GetAssemblyPath(_selectedProject);

      }

      string text = File.ReadAllText(file);
      string namespaceFolder = ExtractNamespaceFolder(text, folderName);

      File.WriteAllText(@"C:\Program Files\OpenDriven\LastRunTest.txt", $"{fileName}|{namespaceFolder}");

      if (!DebugTestsCommand.Build(_selectedProject1))
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

      string output = RunTests.Run(fileName, namespaceFolder);

      Window window = DebugTestsCommand.s_dte.Windows.Item(EnvDTE.Constants.vsWindowKindOutput);
      OutputWindow outputWindow = (OutputWindow)window.Object;
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

      int a = 1;
      a++;

      // Show a message box to prove we were here
      //VsShellUtilities.ShowMessageBox(
      //    this.package,
      //    message+"\r\n"+ file+"\r\n"+ folderName,
      //    title,
      //    OLEMSGICON.OLEMSGICON_INFO,
      //    OLEMSGBUTTON.OLEMSGBUTTON_OK,
      //    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
    }
    public static string ExtractNamespaceFolder(string text, string folder)
    {
      string endNamespace = "." + folder;
      string namespaceText = text.Substring(text.IndexOf("namespace "));
      namespaceText = namespaceText.Substring("namespace ".Length);
      namespaceText = namespaceText.Substring(0, namespaceText.LastIndexOf(endNamespace) + endNamespace.Length).Trim();

      return namespaceText;
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
  }
}
