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
                return Make_Milliseconds(_Start);
            }
        }

        public long FinishedTime
        {
            get
            {
                return Make_Milliseconds(_End);
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
            _Start = DateTime.Now;
            _Start.AddSeconds(ariveTime);
            
            _End = new DateTime(Make_Milliseconds(_Start));
            _Priority = priority;
        }

        public Process(double ariveTime, double burstTime)
        {
            _Start = DateTime.Now;
            _Start.AddSeconds(ariveTime);

            _End = new DateTime(Make_Milliseconds(_Start));
            _Priority = 0;
        }

        private long Make_Milliseconds(DateTime start)
        {
            double mil = start.ToUniversalTime().Subtract(new
                         DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))
                         .TotalMilliseconds;
            return (long)mil;
        }
        
        public void PreEmpt()
        {
            // get time ran
            _End = DateTime.Now;
            _RemainingTime = _End.Subtract(_Start).Milliseconds;
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
