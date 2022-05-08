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
  internal sealed class ToolbarOpenReportPassedCommand
  {
    /// <summary>
    /// Command ID.
    /// </summary>
    public const int CommandId = 4129;

    /// <summary>
    /// Command menu group (command set GUID).
    /// </summary>
    public static readonly Guid CommandSet = new Guid("c5bccf32-96d1-4e8a-93b2-a9c56ea803d9");

    /// <summary>
    /// VS Package that provides this command, not null.
    /// </summary>
    private readonly AsyncPackage package;


    /// <summary>
    /// Initializes a new instance of the <see cref="ToolbarOpenReportPassedCommand"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    /// <param name="commandService">Command service to add command to, not null.</param>
    private ToolbarOpenReportPassedCommand(AsyncPackage package, OleMenuCommandService commandService)
    {
      ThreadHelper.ThrowIfNotOnUIThread();
      this.package = package ?? throw new ArgumentNullException(nameof(package));
      commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

      var menuCommandID = new CommandID(CommandSet, CommandId);
      var menuItem = new MenuCommand(this.Execute, menuCommandID);
      commandService.AddCommand(menuItem);
      m_menuItem = menuItem;

      //System.IServiceProvider serviceProvider = package as System.IServiceProvider;
      //OleMenuCommandService mcs = serviceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
      //if (null != mcs)
      //{
      //  // Create the command for the menu item.
      //  CommandID menuCommandID = new CommandID(CommandSet, CommandId);
      //  var menuItem = new OleMenuCommand(Execute, menuCommandID);
      //  menuItem.BeforeQueryStatus += MenuItem_BeforeQueryStatus; ;
      //  mcs.AddCommand(menuItem);
      //  m_menuItem = menuItem;
      //}




      //   DebugCommand.s_dte.Events.DocumentEvents.DocumentOpened += DocumentEvents_DocumentOpened;
      //  DebugCommand.s_dte.Events.SolutionEvents.Opened += SolutionEvents_Opened;

      //Thread thread = new Thread(() => 
      //{
      //  while (true)
      //  {
      //    if (File.Exists(@"C:\Program Files\OpenDriven\LastRunTestResult.txt"))
      //    {
      //      if (File.ReadAllText(@"C:\Program Files\OpenDriven\LastRunTestResult.txt") == "PASS")
      //      {
      //        m_menuItem.Visible = true;
      //      }
      //    }
      //    System.Threading.Thread.Sleep(1000);
      //  }
      //});
      //thread.IsBackground = true;
      //thread.Start();
    }

   // OleMenuCommand m_menuItem;

    private void MenuItem_BeforeQueryStatus(object sender, EventArgs e)
    {
      if (File.Exists(@"C:\Program Files\OpenDriven\LastRunTestResult.txt"))
      {
        if (File.ReadAllText(@"C:\Program Files\OpenDriven\LastRunTestResult.txt") == "PASS")
        {
          m_menuItem.Visible = true;
        }
      }
    }

    //private void DocumentEvents_DocumentOpened(EnvDTE.Document Document)
    //{
    //  if (File.Exists(@"C:\Program Files\OpenDriven\LastRunTestResult.txt"))
    //  {
    //    if (File.ReadAllText(@"C:\Program Files\OpenDriven\LastRunTestResult.txt") == "PASS")
    //    {
    //      m_menuItem.Visible = true;
    //    }
    //  }
    //}

    private void DTEEvents_OnStartupComplete()
    {
 
    }

    MenuCommand m_menuItem;

    private void SolutionEvents_Opened()
    {

    }

    /// <summary>
    /// Gets the instance of the command.
    /// </summary>
    public static ToolbarOpenReportPassedCommand Instance
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
      // Switch to the main thread - the call to AddCommand in ToolbarTestCommand's constructor requires
      // the UI thread.
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

      OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
      Instance = new ToolbarOpenReportPassedCommand(package, commandService);
    }

    public static void OpenTestReport()
    {
      ThreadHelper.ThrowIfNotOnUIThread();
      if (File.Exists("C:\\Program Files\\OpenDriven\\TestReport.html"))
      {

        //        DebugTestsCommand.s_dte.ItemOperations.Navigate("http://www.google.com.au");
        try
        {
          DebugTestsCommand.s_dte.ItemOperations.Navigate("file:///C:/Program%20Files/OpenDriven/TestReport.html", EnvDTE.vsNavigateOptions.vsNavigateOptionsDefault);
        }
        catch (Exception ex)
        {
          int a = 1;
          a++;
        }
        //        DebugTestsCommand.s_dte.ItemOperations.Navigate("C:\\Program Files\\OpenDriven\\TestReport.html", EnvDTE.vsNavigateOptions.vsNavigateOptionsDefault);
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
      string title = "ToolbarTestCommand";

      OpenTestReport();

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
