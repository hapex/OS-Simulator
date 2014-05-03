using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApplication1
{
    public class Stats
    {
        public string Name;
        public string Grouping;
        public Tuple<double,double> ParameterList;
        public Stats()
        {
            ParameterList = new Tuple<double, double>();
        }
    }
}
