namespace OpenDriven
{
  [Command(PackageIds.ConfigureCommand)]
  internal sealed class ConfigureCommand : BaseCommand<ConfigureCommand>
  {
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
      await VS.MessageBox.ShowWarningAsync("OpenDriven", "Thank you for using OpenDriven");
    }
  }
}
