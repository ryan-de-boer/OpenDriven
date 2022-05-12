using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OpenDriven.Commands
{
  /// <summary>
  /// Command handler
  /// </summary>
  internal sealed class DebugFileTestsCommand
  {
    /// <summary>
    /// Command ID.
    /// </summary>
    public const int CommandId = 4181;

    /// <summary>
    /// Command menu group (command set GUID).
    /// </summary>
    public static readonly Guid CommandSet = new Guid("c5bccf32-96d1-4e8a-93b2-a9c56ea803d9");

    /// <summary>
    /// VS Package that provides this command, not null.
    /// </summary>
    private readonly AsyncPackage package;

    /// <summary>
    /// Initializes a new instance of the <see cref="DebugFileTestsCommand"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    /// <param name="commandService">Command service to add command to, not null.</param>
    private DebugFileTestsCommand(AsyncPackage package, OleMenuCommandService commandService)
    {
      this.package = package ?? throw new ArgumentNullException(nameof(package));
      commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

      var menuCommandID = new CommandID(CommandSet, CommandId);
      //      var menuItem = new MenuCommand(this.Execute, menuCommandID);

      var menuItem = new OleMenuCommand(this.Execute, menuCommandID);
      menuItem.BeforeQueryStatus += MenuItem_BeforeQueryStatus;

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

        if (!RunFileTestsCommand.IsSingleProjectItemSelection(out hierarchy, out itemid)) return;
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
    public static DebugFileTestsCommand Instance
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
      // Switch to the main thread - the call to AddCommand in DebugFileTestsCommand's constructor requires
      // the UI thread.
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

      OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
      Instance = new DebugFileTestsCommand(package, commandService);
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
      string title = "DebugFileTestsCommand";

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

      IVsHierarchy hierarchy = null;
      uint itemid = VSConstants.VSITEMID_NIL;

      if (!RunFileTestsCommand.IsSingleProjectItemSelection(out hierarchy, out itemid)) return;
      // Get the file path
      string itemFullPath = null;
      ((IVsProject)hierarchy).GetMkDocument(itemid, out itemFullPath);


      string classWithNamespace = DebugTestsCommand.ExtractNamespaceClass(File.ReadAllText(itemFullPath));
      File.WriteAllText(@"C:\Program Files\OpenDriven\LastDebugTest.txt", $"{fileName}|{classWithNamespace}");

      if (File.Exists(@"C:\Program Files\OpenDriven\nunit-console-3.8\ReadyToAttach.txt"))
      {
        File.Delete(@"C:\Program Files\OpenDriven\nunit-console-3.8\ReadyToAttach.txt");
      }

      DebugTestsCommand.Build(_selectedProject1);

      System.Diagnostics.Process cmd = new System.Diagnostics.Process();
      cmd.StartInfo.FileName = @"C:\Program Files\OpenDriven\nunit-console-3.8\nunit3-console.exe";
      cmd.StartInfo.WorkingDirectory = @"C:\Program Files\OpenDriven\nunit-console-3.8";
      cmd.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
      cmd.StartInfo.CreateNoWindow = true;
      cmd.StartInfo.Arguments = $"{fileName} /test={classWithNamespace} --debug-agent";
      cmd.Start();

      while (!File.Exists(@"C:\Program Files\OpenDriven\nunit-console-3.8\ReadyToAttach.txt"))
      {
        System.Threading.Thread.Sleep(500);
      }

      DebugTestsCommand.Attach(DebugTestsCommand.s_dte);

      File.Delete(@"C:\Program Files\OpenDriven\nunit-console-3.8\ReadyToAttach.txt");

      //// Show a message box to prove we were here
      //VsShellUtilities.ShowMessageBox(
      //    this.package,
      //    message,
      //    title,
      //    OLEMSGICON.OLEMSGICON_INFO,
      //    OLEMSGBUTTON.OLEMSGBUTTON_OK,
      //    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
    }
  }
}
