global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using System;
global using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using OpenDriven.Commands;
using System.ComponentModel.Design;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace OpenDriven
{
  //https://stackoverflow.com/questions/54044350/vs2017-vsix-just-created-asyncpackage-is-not-being-instanced
  //for auto load settings

  [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
  [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
  [ProvideMenuResource("Menus.ctmenu", 1)]
  [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string, PackageAutoLoadFlags.BackgroundLoad)]
  [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionHasMultipleProjects_string, PackageAutoLoadFlags.BackgroundLoad)]
  [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionHasSingleProject_string, PackageAutoLoadFlags.BackgroundLoad)]
  [Guid(PackageGuids.OpenDrivenString)]
  public sealed class OpenDrivenPackage : ToolkitPackage
  {
    public OpenDrivenPackage()
    {
    }

    protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
    {
      DebugTestsCommand.s_dte = (EnvDTE.DTE)this.GetService(typeof(EnvDTE.DTE));
      RunTestsCommand.s_dte = (EnvDTE.DTE)this.GetService(typeof(EnvDTE.DTE));

      await this.RegisterCommandsAsync();
        await OpenDriven.Commands.RunTestsCommand.InitializeAsync(this);
        await OpenDriven.Commands.DebugTestsCommand.InitializeAsync(this);
        await OpenDriven.Commands.ToolbarOpenReportPassedCommand.InitializeAsync(this);
        await OpenDriven.Commands.ToolbarOpenReportFailedCommand.InitializeAsync(this);


      bool doPass = false;
      if (File.Exists(@"C:\Program Files\OpenDriven\LastRunTestResult.txt"))
      {
        if (File.ReadAllText(@"C:\Program Files\OpenDriven\LastRunTestResult.txt") == "PASS")
        {
          doPass = true;
        }
      
     
      }

      bool doFail = false;
      if (File.Exists(@"C:\Program Files\OpenDriven\LastRunTestResult.txt"))
      {
        if (File.ReadAllText(@"C:\Program Files\OpenDriven\LastRunTestResult.txt") == "FAIL")
        {
          doFail = true;
        }
      }


      const string guidOpenDrivenPackageCmdSet = "c5bccf32-96d1-4e8a-93b2-a9c56ea803d9";
      int cmdID = 4129;


      OleMenuCommandService mcs = (OleMenuCommandService)this.GetService(typeof(IMenuCommandService));
      var newCmdID = new CommandID(new Guid(guidOpenDrivenPackageCmdSet), cmdID);
      MenuCommand mc = mcs.FindCommand(newCmdID);
      if (mc != null)
      {
        //mc.CommandChanged += Mc_CommandChanged;
        //        mc.Enabled = enableCmd;
        //        mc.Visible = enableCmd;
        mc.Visible = doPass;
      }

      cmdID = 4177;

      mcs = (OleMenuCommandService)this.GetService(typeof(IMenuCommandService));
      newCmdID = new CommandID(new Guid(guidOpenDrivenPackageCmdSet), cmdID);
      mc = mcs.FindCommand(newCmdID);
      if (mc != null)
      {
        //mc.CommandChanged += Mc_CommandChanged;
        //        mc.Enabled = enableCmd;
        //        mc.Visible = enableCmd;
        mc.Visible = doFail;
      }
        await OpenDriven.Commands.ToolbarRunLastCommand.InitializeAsync(this);
        await OpenDriven.Commands.ToolbarDebugLastCommand.InitializeAsync(this);
        await OpenDriven.Commands.RunFileTestsCommand.InitializeAsync(this);
        await OpenDriven.Commands.DebugFileTestsCommand.InitializeAsync(this);
      


    }
  }
}