using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OS_Simulator
{
    class Queue
    {

        private QueueType _Type; // determines behavior (one class to rule them all...)
        private double _MaxSize; // max size of simulator object
        private Collection<double> _PageSizes; // holds list of all current pages in KB
        private Collection<Dictionary<string, Process>> _ProcessList; // list of processes and their burstTime
        // list of each process / memory block and the metrics there for
        private Collection<Dictionary<string, double>> _TurnAroundChart; 
        private Collection<Dictionary<string, double>> _WaitChart;
        private Collection<Dictionary<string, double>> _FragmentationChart;

        
        public enum QueueType { Memory = 0, Paging = 1, Process = 2 };

        public Queue(int type, double size, double pages = 0)
        {
            _Type = (QueueType) type;
            _MaxSize = size;
            _PageSizes = new Collection<double>();
            _PageSizes.Add(pages);
            _ProcessList = new Collection<Dictionary<string, Process>>();
        }

        public void Generate_Data()
        {
            switch (_Type)
            {
                case QueueType.Memory:

                    break;

                case  QueueType.Paging:

                    break;

                case QueueType.Process:

                    break;
            }
        }

        public void Run_Simulation()
        {
            switch (_Type)
            {
                case QueueType.Memory:

                    break;

                case QueueType.Paging:

                    break;

                case QueueType.Process:

                    break;
            }
        }

        public void Get_Metrics(out Collection<Dictionary<string, Process>> processes, 
            out Collection<Dictionary<string, double>> turnAround, 
            out Collection<Dictionary<string, double>> wait,
            out Collection<Dictionary<string, double>> fragmentation)
        {
            processes = _ProcessList;
            turnAround = _TurnAroundChart;
            wait = _WaitChart;
            fragmentation = _FragmentationChart;
        }
    }
}
