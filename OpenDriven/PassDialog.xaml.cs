using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OpenDriven
{
  /// <summary>
  /// Interaction logic for FailDialog.xaml
  /// </summary>
  public partial class PassDialog : Window
  {
    public PassDialog()
    {
      InitializeComponent();
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
      //     OpenDriven.Commands.ToolbarOpenReportPassedCommand.OpenTestReport();
      //     Close();
    }

    private void button_Click(object sender, RoutedEventArgs e)
    {
      Close();
    }

    private void label2_MouseDown(object sender, MouseButtonEventArgs e)
    {
      //      OpenDriven.Commands.ToolbarOpenReportPassedCommand.OpenTestReport();
      //      Close();
    }

    private void Label_MouseDown(object sender, MouseButtonEventArgs e)
    {
      OpenDriven.Commands.ToolbarOpenReportPassedCommand.OpenTestReport();
      Close();
    }

    private void Label_MouseEnter(object sender, MouseEventArgs e)
    {
      Cursor = Cursors.Hand;
    }

    private void Label_MouseLeave(object sender, MouseEventArgs e)
    {
      Cursor = Cursors.Arrow;
    }
  }
}
