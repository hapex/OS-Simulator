using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OS_Simulator
{
    public class Process
    {
        // members
        private DateTime _Start;
        private DateTime _End;
        private double _AriveTime;
        private double _BurstTime;
        // will be -20 t0 20 for priority, lowest is highest priority
        private int _Priority;
        // used for preemption
        private double _RemainingTime;
        private double _RunTime;

        // constructor
        public Process()
        {
            _AriveTime = 0;
            _BurstTime = 0;
            _Priority = 0;
        }

        public Process(double ariveTime, double burstTime, int priority)
        {
            _AriveTime = ariveTime;
            _BurstTime = burstTime;
            _Priority = priority;
        }

        public Process(double ariveTime, double burstTime)
        {
            _AriveTime = ariveTime;
            _BurstTime = burstTime;
            _Priority = 0;
        }
        
        public void PreEmpt()
        {
            // get time ran
            _End = DateTime.Now;
            double mSec = _End.Subtract(_Start).Milliseconds;

            _RemainingTime = _BurstTime - mSec;
        }

        public void Start()
        {
            _Start = DateTime.Now;
        }

        public bool Completed()
        {
            return _RemainingTime <= 0;
        }
    }
}
