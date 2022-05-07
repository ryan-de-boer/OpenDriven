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
  internal sealed class ToolbarCommand2
  {
    /// <summary>
    /// Command ID.
    /// </summary>
    public const int CommandId = 4177;

    /// <summary>
    /// Command menu group (command set GUID).
    /// </summary>
    public static readonly Guid CommandSet = new Guid("c5bccf32-96d1-4e8a-93b2-a9c56ea803d9");

    /// <summary>
    /// VS Package that provides this command, not null.
    /// </summary>
    private readonly AsyncPackage package;

    /// <summary>
    /// Initializes a new instance of the <see cref="ToolbarCommand2"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    /// <param name="commandService">Command service to add command to, not null.</param>
    private ToolbarCommand2(AsyncPackage package, OleMenuCommandService commandService)
    {
      this.package = package ?? throw new ArgumentNullException(nameof(package));
      commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

      var menuCommandID = new CommandID(CommandSet, CommandId);
      var menuItem = new MenuCommand(this.Execute, menuCommandID);
      commandService.AddCommand(menuItem);
      //m_menuItem = menuItem;


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




      //      DebugCommand.s_dte.Events.SolutionEvents.
      //DebugCommand.s_dte.Events.SolutionEvents.Opened += SolutionEvents_Opened;

    }

    OleMenuCommand m_menuItem;

    private void MenuItem_BeforeQueryStatus(object sender, EventArgs e)
    {
      if (File.Exists(@"C:\Program Files\OpenDriven\LastRunTestResult.txt"))
      {
        if (File.ReadAllText(@"C:\Program Files\OpenDriven\LastRunTestResult.txt") == "FAIL")
        {
          m_menuItem.Visible = true;
        }
      }
    }

    //private void DocumentEvents_DocumentOpened(EnvDTE.Document Document)
    //{
    //  if (File.Exists(@"C:\Program Files\OpenDriven\LastRunTestResult.txt"))
    //  {
    //    if (File.ReadAllText(@"C:\Program Files\OpenDriven\LastRunTestResult.txt") == "FAIL")
    //    {
    //      m_menuItem.Visible = true;
    //    }
    //  }
    //}

    private void DTEEvents_OnStartupComplete()
    {

    }

 //   MenuCommand m_menuItem;

    private void SolutionEvents_Opened()
    {

    }


    /// <summary>
    /// Gets the instance of the command.
    /// </summary>
    public static ToolbarCommand2 Instance
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
      // Switch to the main thread - the call to AddCommand in ToolbarCommand2's constructor requires
      // the UI thread.
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

      OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
      Instance = new ToolbarCommand2(package, commandService);
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
      string title = "ToolbarCommand2";

      // Show a message box to prove we were here
      VsShellUtilities.ShowMessageBox(
          this.package,
          message,
          title,
          OLEMSGICON.OLEMSGICON_INFO,
          OLEMSGBUTTON.OLEMSGBUTTON_OK,
          OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
    }
  }
}
