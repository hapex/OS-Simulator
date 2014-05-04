using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OS_Simulator
{
    public class Stats
    {
        public string Name { get; set; }
        public string Series1 { get; set; }
        public string Series2 { get; set; }
        public string Grouping { get; set; }
        public double Stat1 { get; set; }
        public double Stat2 { get; set; }
        public Stats()
        {
            Stat1 = -1;
            Stat2 = -1;
            Series1 = null;
            Series2 = null;
        }
    }
}
