global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using System;
global using Task = System.Threading.Tasks.Task;
using OpenDriven.Commands;
using System.Runtime.InteropServices;
using System.Threading;

namespace OpenDriven
{
  [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
  [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
  [ProvideMenuResource("Menus.ctmenu", 1)]
  [Guid(PackageGuids.OpenDrivenString)]
  public sealed class OpenDrivenPackage : ToolkitPackage
  {
    public OpenDrivenPackage()
    {
    }

    protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
    {
      DebugCommand.s_dte = (EnvDTE.DTE)this.GetService(typeof(EnvDTE.DTE));
      Command1.s_dte = (EnvDTE.DTE)this.GetService(typeof(EnvDTE.DTE));

      await this.RegisterCommandsAsync();
        await OpenDriven.Commands.Command1.InitializeAsync(this);
        await OpenDriven.Commands.DebugCommand.InitializeAsync(this);
    }
  }
}