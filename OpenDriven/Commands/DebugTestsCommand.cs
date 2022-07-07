using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenDriven.Commands
{
  /// <summary>
  /// Command handler
  /// </summary>
  public sealed class DebugTestsCommand
  {
    /// <summary>
    /// Command ID.
    /// </summary>
    public const int CommandId = 257;

    /// <summary>
    /// Command menu group (command set GUID).
    /// </summary>
    public static readonly Guid CommandSet = new Guid("c5bccf32-96d1-4e8a-93b2-a9c56ea803d9");

    /// <summary>
    /// VS Package that provides this command, not null.
    /// </summary>
    private readonly AsyncPackage package;

    public static AsyncPackage s_package;

    /// <summary>
    /// Initializes a new instance of the <see cref="DebugTestsCommand"/> class.
    /// Adds our command handlers for menu (commands must exist in the command table file)
    /// </summary>
    /// <param name="package">Owner package, not null.</param>
    /// <param name="commandService">Command service to add command to, not null.</param>
    private DebugTestsCommand(AsyncPackage package, OleMenuCommandService commandService)
    {
      this.package = package ?? throw new ArgumentNullException(nameof(package));
      s_package = package;
      commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

      var menuCommandID = new CommandID(CommandSet, CommandId);
      var menuItem = new MenuCommand(this.Execute, menuCommandID);
      commandService.AddCommand(menuItem);

      //     RestartServer();
    }

    //static SocketServer server;
    //private void RestartServer()
    //{
    //  return;

    //  try
    //  {
    //    if (server != null)
    //    {
    //      server.Received -= Server_Received;
    //      server.Stop();
    //    }

    //    bool portOk = false;


    //      for (int i = 9004; i < 100000 && !portOk; ++i)
    //      {
    //        try
    //        {
    //          server = new SocketServer(i);
    //          portOk = true;
    //          break;
    //        }
    //        catch (SocketException)
    //        {
    //          // in use
    //        }
    //      }


    //    server.Received += Server_Received;
    //    server.Start();
    //  }
    //  catch (Exception ex)
    //  {
    //    VsShellUtilities.ShowMessageBox(
    //        this.package,
    //        ex.ToString(),
    //        "Error loading server",
    //        OLEMSGICON.OLEMSGICON_INFO,
    //        OLEMSGBUTTON.OLEMSGBUTTON_OK,
    //        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
    //  }
    //}

    /// <summary>
    /// Gets the instance of the command.
    /// </summary>
    public static DebugTestsCommand Instance
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
      // Switch to the main thread - the call to AddCommand in DebugCommand's constructor requires
      // the UI thread.
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

      OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
      Instance = new DebugTestsCommand(package, commandService);
    }

    public static EnvDTE.DTE s_dte;
    public static EnvDTE80.DTE2 s_dte2;



    /// <summary>
    /// Opens an existing named event object.
    /// </summary>
    /// <param name="dwDesiredAccess">The access to the event object. The function fails if the security descriptor of the specified object does not permit the requested access for the calling process.</param>
    /// <param name="bInheritHandle">If this value is TRUE, processes created by this process will inherit the handle. Otherwise, the processes do not inherit this handle.</param>
    /// <param name="lpName">The name of the event to be opened. Name comparisons are case sensitive.</param>
    /// <returns>If the function succeeds, the return value is a handle to the event object. If the function fails, the return value is IntPtr.Zero.</returns>
    [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr OpenEvent(UInt32 dwDesiredAccess, bool bInheritHandle, String lpName);

    /// <summary>
    /// Opens the event handle for NUNIT_READY_TO_ATTACH. 
    /// </summary>
    /// <param name="eventHandle">Event handle to open.</param>
    /// <returns>Whether the event handle was opened.</returns>
    private static bool OpenEvent(out IntPtr eventHandle)
    {
      const int EVENT_ALL_ACCESS = 2031619; // Same as EVENT_ALL_ACCESS value in the Win32 realm.
      eventHandle = OpenEvent(EVENT_ALL_ACCESS, false, "NUNIT_READY_TO_ATTACH");
      return eventHandle != IntPtr.Zero;
    }

    /// <summary>
    /// Closes an open object handle.
    /// </summary>
    /// <param name="hObject">A valid handle to an open object.</param>
    /// <returns>Whether closing the handle succeeds.</returns>
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(IntPtr hObject);

    static bool s_eventOccurred = false;
    public static void WaitAttach()
    {

      // Wait until the NUNIT_READY_TO_ATTACH event occurs.
      IntPtr eventHandle;
      bool eventOccurred = false;
      if (OpenEvent(out eventHandle))
      {
        AutoResetEvent autoResetEvent = new AutoResetEvent(false);
        autoResetEvent.SafeWaitHandle = new Microsoft.Win32.SafeHandles.SafeWaitHandle(eventHandle, true);
        WaitHandle[] waitHandles = new WaitHandle[] { autoResetEvent };
        while (!eventOccurred)
        {
          int waitResult = WaitHandle.WaitAny(waitHandles, 1000, false);
          if (waitResult == 0)
          {
            // Event occurred.
            eventOccurred = true;
            s_eventOccurred = true;
            break;
          }
        }
      }

      //ThreadHelper.ThrowIfNotOnUIThread();
      //EnvDTE.Processes processes = dte.Debugger.LocalProcesses;
      //bool foundIt = false;
      //while (!foundIt)
      //{
      //  foreach (EnvDTE.Process proc in processes)
      //  {
      //    if (proc.Name.IndexOf("nunit-agent.exe") != -1)
      //    {
      //      foundIt = true;
      //      return;
      //    }
      //  }
      //  System.Threading.Thread.Sleep(1000);
      //  processes = dte.Debugger.LocalProcesses;
      //}
    }

    //private static void Server_Received(object sender, DataReceivedEventArgs e)
    //{
    //  string msg = $"{DateTimeOffset.Now.ToString()} Received: {e.Data}";
    //  s_eventOccurred = true;
    //}



    public static string GetAssemblyPath(EnvDTE.Project vsProject)
    {
      ThreadHelper.ThrowIfNotOnUIThread();
      try
      {
        const string VsProjectItemKindSolutionFolder = "{66A26720-8FB5-11D2-AA7E-00C04F688DDE}";
        const string VsProjectKindMisc = "{66A2671D-8FB5-11D2-AA7E-00C04F688DDE}";
        if (vsProject.Kind == VsProjectItemKindSolutionFolder ||
          vsProject.Kind == VsProjectKindMisc)
        {
          return "";
        }

        string fullPath = vsProject.Properties.Item("FullPath").Value.ToString();

        string outputPath = vsProject.ConfigurationManager.ActiveConfiguration.Properties.Item("OutputPath").Value.ToString();

        string outputDir = Path.Combine(fullPath, outputPath);

        string outputFileName = vsProject.Properties.Item("OutputFileName").Value.ToString();

        string assemblyPath = Path.Combine(outputDir, outputFileName);

        return assemblyPath;
      }
      catch (Exception ex)
      {
        return "";
      }
    }

    public static bool sm_escape = false;

    /// <summary>
    /// Displays a small input dialog with a text box for the user to enter a line of text.
    /// </summary>
    /// <param name="message">The message to display in the dialog caption.</param>
    /// <param name="initialText">Initial text to store in text box.</param>
    /// <returns>If the user presses ENTER in the text box, then the text box contents is returned.
    /// If the user presses ESCAPE in the text box, then an empty string is returned</returns>
    public static string InputBox(string message, string initialText)
    {
      //const int HEIGHT = 47;
      const int HEIGHT = 60;

      sm_escape = false;
      Form form = new Form();
      TextBox textBox = new TextBox();
      textBox.Text = initialText;
      form.Controls.Add(textBox);
      textBox.Dock = DockStyle.Fill;
      form.Text = message;
      form.Size = new System.Drawing.Size(187, HEIGHT);
      form.FormBorderStyle = FormBorderStyle.FixedDialog;
      form.ControlBox = false;
      form.StartPosition = FormStartPosition.CenterParent;
      textBox.KeyUp += new KeyEventHandler(delegate (object source, KeyEventArgs args)
      {
        if (args.KeyCode == Keys.Enter)
        {
          form.Close();
        }
        else if (args.KeyCode == Keys.Escape)
        {
          sm_escape = true;
          textBox.Text = string.Empty;
          form.Close();
        }
      });
      textBox.SelectAll();
      form.ShowDialog();
      return textBox.Text;
    }

    public static void Build(EnvDTE.Project _selectedProject1)
    {
      Solution2 soln = (Solution2)s_dte.Solution;
      SolutionBuild2 sb = (SolutionBuild2)soln.SolutionBuild;
      BuildDependencies bld = sb.BuildDependencies;
      //System.Windows.Forms.MessageBox.Show("The project " + bld.Item(1).Project.Name + " has "
      //       + bld.Count.ToString() + " build dependencies.");
      //System.Windows.Forms.MessageBox.Show("Building the project in debug mode...");
      // 

      Window window = s_dte.Windows.Item(EnvDTE.Constants.vsWindowKindOutput);
      OutputWindow outputWindow = (OutputWindow)window.Object;
      //      outputWindow.ActivePane.Activate();
      //      outputWindow.ActivePane.OutputString(output);
      EnvDTE.OutputWindowPane owp;
      bool found = false;
      foreach (EnvDTE.OutputWindowPane x in outputWindow.OutputWindowPanes)
      {
        if (x.Name == "Build")
        {
          x.Activate();
          x.Clear();
          x.OutputString("The project " + bld.Item(1).Project.Name + " has " + bld.Count.ToString() + " build dependencies.");
          x.OutputString("Building the project in debug mode...");
          found = true;
          break;
        }
      }
      if (!found)
      {
        owp = outputWindow.OutputWindowPanes.Add("Build");
        owp.OutputString("The project " + bld.Item(1).Project.Name + " has " + bld.Count.ToString() + " build dependencies.");
        owp.OutputString("Building the project in debug mode...");
      }
      sb.BuildProject("Debug", _selectedProject1.FullName, true);
    }

    public static string ExtractNamespaceTest(string text)
    {
      string namespaceText = text.Substring(text.IndexOf("namespace "));
      namespaceText = namespaceText.Substring("namespace ".Length);
      namespaceText = namespaceText.Substring(0, namespaceText.IndexOf("\n")).Trim();

      if (text.IndexOf("public class") == -1)
      {
        return $"{namespaceText}";
      }

      string className = text.Substring(text.IndexOf("public class"));
      className = className.Substring("public class".Length);
      className = className.Substring(0, className.IndexOf("\n")).Trim();

      if (text.LastIndexOf("[Test]") == -1)
      {
        return $"{namespaceText}.{className}";
      }

      string testName = text.Substring(text.LastIndexOf("[Test]"));
      testName = testName.Substring(testName.IndexOf("public void"));
      testName = testName.Substring("public void".Length);
      testName = testName.Substring(0, testName.IndexOf("\n")).Trim();
      testName = testName.Replace("()", "");

      return $"{namespaceText}.{className}.{testName}";
    }

    public static string ExtractNamespaceClass(string text)
    {
      string namespaceText = text.Substring(text.IndexOf("namespace "));
      namespaceText = namespaceText.Substring("namespace ".Length);
      namespaceText = namespaceText.Substring(0, namespaceText.IndexOf("\n")).Trim();

      string className = text.Substring(text.IndexOf("public class"));
      className = className.Substring("public class".Length);
      className = className.Substring(0, className.IndexOf("\n")).Trim();

      return $"{namespaceText}.{className}";
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
      string title = "DebugCommand";

      //     System.Threading.Thread thread1 = new System.Threading.Thread(WaitAttach);
      //     thread1.Start();


      Track.TrackFile();
      string fileName = "";
      EnvDTE.Project _selectedProject1 = null;
      Array _projects = s_dte.ActiveSolutionProjects as Array;
      if (_projects.Length != 0 && _projects != null)
      {
        EnvDTE.Project _selectedProject = _projects.GetValue(0) as EnvDTE.Project;
        _selectedProject1 = _selectedProject;
        //get the project path

        fileName = GetAssemblyPath(_selectedProject);

      }
      //string testWithNamespace = InputBox("Test with namespace", "");

      s_eventOccurred = false;
 //     RestartServer();


      var activePoint = ((EnvDTE.TextSelection)s_dte.ActiveDocument.Selection).ActivePoint;
      string textLine = activePoint.CreateEditPoint().GetLines(activePoint.Line, activePoint.Line + 1);

      string text2 = activePoint.CreateEditPoint().GetLines(1, activePoint.Line + 2);

      string testWithNamespace = ExtractNamespaceTest(text2);

      File.WriteAllText(@"C:\Program Files\OpenDriven\LastDebugTest.txt", $"{fileName}|{testWithNamespace}");

      DebugTests.PreDebug();

      Build(_selectedProject1);

      //bool isX86 = false;

      //if (File.Exists(@"C:\Program Files\OpenDriven\x86.txt"))
      //{
      //  if (File.ReadAllText(@"C:\Program Files\OpenDriven\x86.txt").ToLower() == "true")
      //  {
      //    isX86 = true;
      //  }
      //}
      //else
      //{
      //  string output = RunTests.Run(fileName, testWithNamespace);
      //  if (output.Contains("BadImageFormatException"))
      //  {
      //    isX86 = true;
      //  }
      //}


      DebugTests.Debug(fileName, testWithNamespace, s_dte);
 

      // Show a message box to prove we were here
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
