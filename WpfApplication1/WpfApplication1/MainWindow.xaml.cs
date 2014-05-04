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
            steve.Run_Mem_Simple_Paging();
        }

        private void OnProcessSimClicked(object sender, RoutedEventArgs e)
        {
            List<Stats> list = new List<Stats>();


            //// Debuging code
            //Stats obj = new Stats();

            //obj.Name = "Test1";
            //obj.Grouping = "A";
            //obj.Series1 = "Alpha";
            //obj.Series2 = "Beta";
            //obj.Stat1 = 1;
            //obj.Stat2 = 6;

            //list.Add(obj);

            //Stats obj2 = new Stats();
            //obj2.Name = "Test1";
            //obj2.Grouping = "B";
            //obj2.Series1 = "Alpha";
            //obj2.Series2 = "Beta";
            //obj2.Stat1 = 8;
            //obj2.Stat2 = 16;

            //list.Add(obj2);

            //Stats obj3 = new Stats();
            //obj3.Name = "Test1";
            //obj3.Grouping = "C";
            //obj3.Series1 = "Alpha";
            //obj3.Series2 = "Beta";
            //obj3.Stat1 = 3;
            //obj3.Stat2 = 4;

            //list.Add(obj3);

            var window1 = new Window1(list);
            window1.Show();
        }

        private void OnMemorySimClicked(object sender, RoutedEventArgs e)
        {
            List<Stats> list = new List<Stats>();

            // call Memory run here

            var window1 = new Window1(list);
            window1.Show();
        }

        private void OnPagingSimClicked(object sender, RoutedEventArgs e)
        {
            List<Stats> list = new List<Stats>();

            // call Paging run here

            var window1 = new Window1(list);
            window1.Show();
        }
        private void OnBestClicked(object sender, RoutedEventArgs e)
        {
            List<Stats> list = new List<Stats>();

            // call run here

            var window1 = new Window1(list);
            window1.Show();
        }

        private void OnExitClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
