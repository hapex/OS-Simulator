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
            
            // Add chart Area to the graph
            chart.ChartAreas.Add("area");
            chart.ChartAreas["area"].AxisY.Interval = 1;
            
            // Create a legend
            chart.Legends.Add(new Legend("legend"));
            // Dock legend to chart area
            chart.Legends["legend"].DockedToChartArea = "area";

            // add the first series to the graph
            chart.Series.Add(list[0].Series1);
            chart.Series[list[0].Series1].ChartType = SeriesChartType.Column;

            // If the 2nd series exists, add it to the graph
            if (list[0].Series2 != null)
            {
                chart.Series.Add(list[0].Series2);
                chart.Series[list[0].Series2].ChartType = SeriesChartType.Column;
            }

            // Add the data source to graph
            chart.DataSource = list;

            // Hook up the 1st series to the graph
            chart.Series[list[0].Series1].XValueMember = "Grouping";
            chart.Series[list[0].Series1].YValueMembers = "Stat1";
            chart.Series[list[0].Series1].Legend = "legend";

            // If the 2nd Series exists, hook it up to the graph
            if (list[0].Series2 != null)
            {
                chart.Series[list[0].Series2].XValueMember = "Grouping";
                chart.Series[list[0].Series2].YValueMembers = "Stat2";
                chart.Series[list[0].Series2].Legend = "legend";
            }


        }

        private void Window_Loaded(object sedner, RoutedEventArgs e)
        {
            System.Windows.Forms.Integration.WindowsFormsHost host =
                new System.Windows.Forms.Integration.WindowsFormsHost();

        }
    }

    
}



