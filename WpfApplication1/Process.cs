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

        // will be -20 t0 20 for priority, lowest is highest priority
        private int _Priority;
        private long _RemainingTime;
        private long _BurstTime;
        private long _Wait;
        public long Size;
        public string ProcessID;
        public int Priority
        {
            get
            {
                return _Priority;
            }
        }

        public long RemainingTime
        {
            get
            {
                return _RemainingTime;
            }
        }

        public long StartTime
        {
            get
            {
                return (Int32)(_Start.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            }
        }

        public long FinishedTime
        {
            get
            {
                return (Int32)(_End.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            }
        }

        // constructor
        public Process()
        {
            _Start.AddSeconds(0);
            _End.AddSeconds(0);
            _Priority = 0;
        }

        public static long Get_Current_Time()
        {
            return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }

        public Process(double ariveTime, double burstTime, int priority)
        {
            Random rand = new Random();
            _Start = DateTime.Now;
            _Start.AddSeconds(ariveTime);            
            _End = _Start.AddMilliseconds(rand.Next(101));
            _Priority = priority;
            _BurstTime = _RemainingTime = (long)_End.Subtract(_Start).TotalMilliseconds;
        }

        public Process(double ariveTime, double burstTime)
        {
            Random rand = new Random();
            _Start = DateTime.Now;
            _Start.AddSeconds(ariveTime);
            _End = _Start.AddMilliseconds(rand.Next(101));
            _Priority = 0;
            _BurstTime = _RemainingTime = (long)_End.Subtract(_Start).TotalMilliseconds;
        }
        
        public void PreEmpt()
        {
            // get time ran            
            _RemainingTime = _End.Subtract(_Start).Milliseconds;
            _End = DateTime.Now;
        }

        public void Start()
        {
            // take note of wait time
            if (_End < DateTime.Now)
            {
                _Wait += (long)DateTime.Now.Subtract(_End).TotalMilliseconds;
            }
            
            _Start = DateTime.Now;
        }

        public bool Completed()
        {
            return _RemainingTime <= 0;
        }

        public long Get_Wait_Time()
        {
            return _Wait;
        }
    }
}
