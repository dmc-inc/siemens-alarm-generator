using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Dmc.Siemens.AlarmGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
        }

        private void IgnoreMouseEvent(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void ResetPopup(object sender, MouseButtonEventArgs e)
        {
            this.PopupBorder.Height = (this.Overlay.TryFindResource("BorderHeightAnimation") as DoubleAnimation)?.To ?? 80.0;
        }

    }
}
