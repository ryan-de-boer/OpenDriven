namespace OpenDriven
{
  [Command(PackageIds.MyCommand)]
  internal sealed class MyCommand : BaseCommand<MyCommand>
  {
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
    {
      await VS.MessageBox.ShowWarningAsync("OpenDriven", "Thank you for using OpenDriven");
    }
  }
}
