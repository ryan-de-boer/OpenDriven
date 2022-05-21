using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenDriven.Commands
{
  /// <summary>
  /// Command handler
  /// </summary>
  internal sealed class RunFileTestsCommand
  {
    /// <summary>
    /// Command ID.
    /// </summary>
    public const int CommandId = 4180;

    /// <summary>
    /// Command menu group (command set GUID).
    /// </summary>
    public static readonly Guid CommandSet = new Guid("c5bccf32-96d1-4e8a-93b2-a9c56ea803d9");

    /// <summary>
    /// VS Package that provides this command, not null.
    /// </summary>
    private readonly AsyncPackage package;

    /// <summary>
    /// Initializes a new instance of the <see cref="RunFileTestsCommand"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    /// <param name="commandService">Command service to add command to, not null.</param>
    private RunFileTestsCommand(AsyncPackage package, OleMenuCommandService commandService)
    {
      this.package = package ?? throw new ArgumentNullException(nameof(package));
      commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

      var menuCommandID = new CommandID(CommandSet, CommandId);
      //     var menuItem = new MenuCommand(this.Execute, menuCommandID);

      var menuItem = new OleMenuCommand(this.Execute, menuCommandID);
      //menuItem.BeforeQueryStatus += MenuItem_BeforeQueryStatus;
      menuItem.Visible = true;
      menuItem.Enabled = true;

      commandService.AddCommand(menuItem);
    }

    private void MenuItem_BeforeQueryStatus(object sender, EventArgs e)
    {
      // get the menu that fired the event
      var menuCommand = sender as OleMenuCommand;
      if (menuCommand != null)
      {
        // start by assuming that the menu will not be shown
        menuCommand.Visible = false;
        menuCommand.Enabled = false;

        IVsHierarchy hierarchy = null;
        uint itemid = VSConstants.VSITEMID_NIL;

        if (!IsSingleProjectItemSelection(out hierarchy, out itemid)) return;
        // Get the file path
        string itemFullPath = null;
        ((IVsProject)hierarchy).GetMkDocument(itemid, out itemFullPath);

        // then check if the file is named '*cs'
        bool isCs = itemFullPath.EndsWith(".cs");

        // if not leave the menu hidden
        if (!isCs) return;

        menuCommand.Visible = true;
        menuCommand.Enabled = true;
      }
    }

    /// <summary>
    /// Gets the instance of the command.
    /// </summary>
    public static RunFileTestsCommand Instance
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
      // Switch to the main thread - the call to AddCommand in RunFileTestsCommand's constructor requires
      // the UI thread.
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

      OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
      Instance = new RunFileTestsCommand(package, commandService);
    }

    public static bool IsSingleProjectItemSelection(out IVsHierarchy hierarchy, out uint itemid)
    {
      hierarchy = null;
      itemid = VSConstants.VSITEMID_NIL;
      int hr = VSConstants.S_OK;

      var monitorSelection = Package.GetGlobalService(typeof(SVsShellMonitorSelection)) as IVsMonitorSelection;
      var solution = Package.GetGlobalService(typeof(SVsSolution)) as IVsSolution;
      if (monitorSelection == null || solution == null)
      {
        return false;
      }

      IVsMultiItemSelect multiItemSelect = null;
      IntPtr hierarchyPtr = IntPtr.Zero;
      IntPtr selectionContainerPtr = IntPtr.Zero;

      try
      {
        hr = monitorSelection.GetCurrentSelection(out hierarchyPtr, out itemid, out multiItemSelect, out selectionContainerPtr);

        if (ErrorHandler.Failed(hr) || hierarchyPtr == IntPtr.Zero || itemid == VSConstants.VSITEMID_NIL)
        {
          // there is no selection
          return false;
        }

        // multiple items are selected
        if (multiItemSelect != null) return false;

        // there is a hierarchy root node selected, thus it is not a single item inside a project

        if (itemid == VSConstants.VSITEMID_ROOT) return false;

        hierarchy = Marshal.GetObjectForIUnknown(hierarchyPtr) as IVsHierarchy;
        if (hierarchy == null) return false;

        Guid guidProjectID = Guid.Empty;

        if (ErrorHandler.Failed(solution.GetGuidOfProject(hierarchy, out guidProjectID)))
        {
          return false; // hierarchy is not a project inside the Solution if it does not have a ProjectID Guid
        }

        // if we got this far then there is a single project item selected
        return true;
      }
      finally
      {
        if (selectionContainerPtr != IntPtr.Zero)
        {
          Marshal.Release(selectionContainerPtr);
        }

        if (hierarchyPtr != IntPtr.Zero)
        {
          Marshal.Release(hierarchyPtr);
        }
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
      string title = "RunFileTestsCommand";

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
      StringBuilder sb = new StringBuilder();
      List<string> itemPaths = new List<string>();
      foreach (EnvDTE.SelectedItem selectedItem in DebugTestsCommand.s_dte.SelectedItems)
      {
        if (selectedItem.ProjectItem is ProjectItem)
        {
          string itemPath = ((ProjectItem)selectedItem.ProjectItem).FileNames[0];
          sb.AppendLine(itemPath);
          itemPaths.Add(itemPath);
        }
      }
      
      if (itemPaths.Count > 0)
      {
        List<string> classesWithNamespaces = new List<string>();
        foreach (string itemPath in itemPaths)
        {
          string oneClassWithNamespace = DebugTestsCommand.ExtractNamespaceClass(File.ReadAllText(itemPath));
          classesWithNamespaces.Add(oneClassWithNamespace);
        }
        string joinString = string.Join(",", classesWithNamespaces);

        File.WriteAllText(@"C:\Program Files\OpenDriven\LastRunTest.txt", $"{fileName}|{joinString}");

        DebugTestsCommand.Build(_selectedProject1);

        string output1 = RunTests.Run(fileName, joinString);

        Window window1 = DebugTestsCommand.s_dte.Windows.Item(EnvDTE.Constants.vsWindowKindOutput);
        OutputWindow outputWindow1 = (OutputWindow)window1.Object;
        EnvDTE.OutputWindowPane owp1;
        bool found1 = false;
        foreach (EnvDTE.OutputWindowPane x in outputWindow1.OutputWindowPanes)
        {
          if (x.Name == "Test Output")
          {
            x.Activate();
            x.Clear();
            x.OutputString(output1);
            found1 = true;
            break;
          }
        }
        if (!found1)
        {
          owp1 = outputWindow1.OutputWindowPanes.Add("Test Output");
          owp1.OutputString(output1);
        }

        HtmlReportCreator.ParseUnitTestResultsFolder("C:\\Program Files\\OpenDriven");

        if (output1.Contains("Failed: 0,") && output1.Contains("Overall result: Passed"))
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

        int a1 = 1;
        a1++;

        //VsShellUtilities.ShowMessageBox(
        //    this.package,
        //    message + "\r\n" + sb.ToString(),
        //    title,
        //    OLEMSGICON.OLEMSGICON_INFO,
        //    OLEMSGBUTTON.OLEMSGBUTTON_OK,
        //    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

        return;
      }

      // 1 path.

      IVsHierarchy hierarchy = null;
      uint itemid = VSConstants.VSITEMID_NIL;

      if (!IsSingleProjectItemSelection(out hierarchy, out itemid)) return;
      // Get the file path
      string itemFullPath = null;
      ((IVsProject)hierarchy).GetMkDocument(itemid, out itemFullPath);


      string classWithNamespace = DebugTestsCommand.ExtractNamespaceClass(File.ReadAllText(itemFullPath));
      File.WriteAllText(@"C:\Program Files\OpenDriven\LastRunTest.txt", $"{fileName}|{classWithNamespace}");

      DebugTestsCommand.Build(_selectedProject1);

      string output = RunTests.Run(fileName, classWithNamespace);

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
      //    message+"\r\n"+ itemFullPath,
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
  }


}
