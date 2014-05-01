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

namespace OS_Simulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Queue steve = new Queue(1, 8192);
            steve.Generate_Data();
            steve.Run_Mem_Sim_Paging();
        }

        private void OnProcessSimClicked(object sender, RoutedEventArgs e)
        {

        }

        private void OnMemorySimClicked(object sender, RoutedEventArgs e)
        {

        }

        private void OnPagingSimClicked(object sender, RoutedEventArgs e)
        {

        }
        private void OnBestClicked(object sender, RoutedEventArgs e)
        {

        }
    }
}
