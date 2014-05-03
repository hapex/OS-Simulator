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
        public List<double> ParameterList;
        public Stats()
        {
            ParameterList = new List<double>();
        }
    }
}
