using System;
using System.Collections.Generic;
using System.Drawing;
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
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace OS_Simulator
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1(List<Stats> list)
        {
            // needs rework for dialog after simulation....
            InitializeComponent();

            // Create Chart object
            Chart chart = this.FindName("Graph") as Chart;
            
            
            chart.ChartAreas.Add("area");
            chart.ChartAreas["area"].AxisY.Interval = 1;
            
            chart.Series.Add(list[0].Series1);
            chart.Series[list[0].Series1].ChartType = SeriesChartType.Column;
            if (list[0].Series2 != null)
            {
                chart.Series.Add(list[0].Series2);
                chart.Series[list[0].Series2].ChartType = SeriesChartType.Column;
            }

            chart.DataSource = list;
            chart.Series[list[0].Series1].XValueMember = "Grouping";
            chart.Series[list[0].Series1].YValueMembers = "Stat1";
            if (list[0].Series2 != null)
            {
                chart.Series[list[0].Series2].XValueMember = "Grouping";
                chart.Series[list[0].Series2].YValueMembers = "Stat2";
            }

            //chart.DataBind();
            //chart.Series["series"].XValueMember = "X";
            //chart.Series["series"].YValueMembers = "Y";

        }

        private void Window_Loaded(object sedner, RoutedEventArgs e)
        {
            System.Windows.Forms.Integration.WindowsFormsHost host =
                new System.Windows.Forms.Integration.WindowsFormsHost();

        }
    }

    
}



