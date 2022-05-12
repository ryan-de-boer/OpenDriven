using OpenDriven.Commands;
using System.Linq;

namespace OpenDriven
{
  public class Track
  {
    //https://github.com/tunnelvisionlabs/FindInSolutionExplorer/blob/master/Tvl.VisualStudio.FindInSolutionExplorer/FindInSolutionExplorerPackage.cs
    public static void TrackFile()
    {
      EnvDTE80.DTE2 dte = DebugTestsCommand.s_dte2;
      EnvDTE.Property track = dte.get_Properties("Environment", "ProjectsAndSolution").Item("TrackFileSelectionInExplorer");
      if (track.Value is bool && !((bool)track.Value))
      {
        track.Value = true;
        track.Value = false;
      }

      // Find the Solution Explorer object
      EnvDTE80.Windows2 windows = dte.Windows as EnvDTE80.Windows2;
      EnvDTE80.Window2 solutionExplorer = FindWindow(windows, EnvDTE.vsWindowType.vsWindowTypeSolutionExplorer);
      if (solutionExplorer != null)
        solutionExplorer.Activate();
    }

    public static EnvDTE80.Window2 FindWindow(EnvDTE80.Windows2 windows, EnvDTE.vsWindowType vsWindowType)
    {
      return windows.Cast<EnvDTE80.Window2>().FirstOrDefault(w => w.Type == vsWindowType);
    }
  }
}
