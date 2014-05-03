using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using WpfApplication1;

namespace OS_Simulator
{
    class Queue
    {

        private QueueType _Type; // determines behavior (one class to rule them all...)
        private double _MaxSize; // max size of simulator object
        private Collection<double> _PageSizes; // holds list of all current pages in KB
        private Collection<Process> _ProcessList; // list of processes and their burstTime
        // list of each process / memory block and the metrics there for
        private Collection<Memory> _MemoryList; // list of memory blocks
        private Collection<string> _Page1, _Page2, _Page3, _Page4;
        private Dictionary<string, double> _TurnAroundChart; 
        private Dictionary<string, double> _WaitChart;
        private Dictionary<string, double> _FragmentationChart;
        private Collection<string> _Log;
        private FileInfo[] _Files;
        private List<Stats> _StatusReport;
        
        public enum QueueType { Memory = 0, Paging = 1, Process = 2 };

        public Queue(int type, double size, double pages = 0)
        {
            _StatusReport = new List<Stats>();
            _Type = (QueueType) type;
            _MaxSize = size;
            _PageSizes = new Collection<double>();
            _PageSizes.Add(pages);
            _ProcessList = new Collection<Process>();
            _Log = new Collection<string>();
            _MemoryList = new Collection<Memory>();
        }

        public void Generate_Data()
        {
            if (_Type == QueueType.Process)
            {
                // use system data to make random processes
                System.Diagnostics.Process[] currentProcesses = System.Diagnostics.Process.GetProcesses();

                foreach (System.Diagnostics.Process p in currentProcesses)
                {
                    Process insert = new Process(Make_Start_Time(), Make_Burst_Time(), Make_Priority());
                    insert.ProcessID = p.ProcessName;
                    insert.Size = p.PeakPagedMemorySize64;
                    _ProcessList.Add(insert);
                }

                // now that all processes are in list, reorder againsts arival time
                _ProcessList = new Collection<Process>(_ProcessList.OrderBy(o => o.StartTime).ToList());
            }

            else if (_Type == QueueType.Paging)
            {
                // here we need generated information regarding blocks of data, so for random fun, lets use frequent things
                string localFolder = Environment.GetFolderPath(Environment.SpecialFolder.Recent);
                DirectoryInfo recentFolder = new DirectoryInfo(localFolder);
                _Files = recentFolder.GetFiles();
            }
            else if (_Type == QueueType.Memory)
            {

            }
        }

        public double Make_Start_Time()
        {
            Random rand = new Random();
            // set random to start atleast 5 seconds in the future
            return rand.NextDouble() * 20 + 5;
        }

        public double Make_Burst_Time()
        {
            Random rand = new Random();
            // set random to complete at least .5 seconds from now
            return rand.NextDouble() * 20 + .5;
        }

        public int Make_Priority()
        {
            Random rand = new Random();
            return rand.Next(41) - 20;
        }

        public void Get_Metrics(out Collection<Process> processes, 
            out Dictionary<string, double> turnAround, 
            out Dictionary<string, double> wait,
            out Dictionary<string, double> fragmentation)
        {
            processes = _ProcessList;
            turnAround = _TurnAroundChart;
            wait = _WaitChart;
            fragmentation = _FragmentationChart;
        }

        public void Run_Process_Sim_RR()
        {
            Random rand = new Random();

            // random quantum to rotate in milliseconds
            int quantum = rand.Next(10);

            // start in arrival list and we rotate throught first to end

            // while there is a process that hasn't completed yet
            while (_ProcessList.Any(o=>o.Completed() == false))
            {

                Process p = _ProcessList.Where(o => o.Completed() == false).First();
                // let the process run for quantum or until the task is complete

                DateTime die = DateTime.Now.AddMilliseconds(quantum);
                while(DateTime.Now.Subtract(die).TotalMilliseconds > 0 && p.Completed() == false)
                {
                    // wait one millisecond
                    Thread.Sleep(1);
                }

                // if we didn't get done
                if (p.Completed() == false)
                {
                    p.PreEmpt();
                }

                // else we push to the end regardless

                // remove p from the list and put back at the end
                _ProcessList.Remove(p);
                _ProcessList.Insert(_ProcessList.Count, p);
                             
            }

            // when complete notify gui
            Notify_Complete();
        }

        public void Run_Process_Sim_Priority()
        {
            // get first process here
            Process p = _ProcessList.Where(o => o.Completed() == false).First();

            // while there is a process that hasn't completed yet
            while (_ProcessList.Any(o => o.Completed() == false))
            {
                // let the process run until interupt by higher priority process or until the task is complete
                while (_ProcessList.Where(o => o.StartTime < Process.Get_Current_Time())
                                   .All(n=>n.Priority > p.Priority) && p.Completed() == false)
                {
                    // wait one millisecond
                    Thread.Sleep(1);
                }

                // if we didn't get done
                if (p.Completed() == false)
                {
                    p.PreEmpt();
                    // replace p with the highest priority process at present
                    p = _ProcessList.Where(o => o.StartTime < Process.Get_Current_Time()).OrderBy(n => n.Priority).First();
                }

                else
                {
                    // replace p with next in line
                    p = _ProcessList.FirstOrDefault(o => o.Completed() == false);
                }
                // don't adjust queues here
            }

            // when complete notify gui
            Notify_Complete();
        }

        public void Run_Process_Sim_Shortest()
        {
            // get first process here
            Process p = _ProcessList.Where(o => o.Completed() == false).First();

            // while there is a process that hasn't completed yet
            while (_ProcessList.Any(o => o.Completed() == false))
            {
                // let the process run until interupt by process with lower remaining time that also exists now or until the task is complete
                while (_ProcessList.Where(o => o.StartTime < Process.Get_Current_Time())
                                   .All(n => n.RemainingTime > p.RemainingTime) && p.Completed() == false)
                {
                    // wait one millisecond
                    Thread.Sleep(1);
                }

                // if we didn't get done
                if (p.Completed() == false)
                {
                    p.PreEmpt();
                    // replace p with the highest priority process at present
                    p = _ProcessList.Where(o => o.StartTime < Process.Get_Current_Time()).OrderBy(n => n.Priority).First();
                }
                else
                {
                    // replace p with next in line
                    p = _ProcessList.FirstOrDefault(o => o.Completed() == false);
                }
                // don't adjust queues here
            }

            // when complete notify gui
            Notify_Complete();
        }

        public void Run_Mem_Sim_TLB()
        {

        }

        public void Run_Mem_Simple_Paging()
        {
            // run each block sim on these as : 512byte, 1024, 2048, 4096
            Memory half, one, two, four;
            // calculate size of files so we know how big to make these
            long byteTotal, fileCount;
            fileCount = byteTotal = 0;
            foreach(FileInfo fi in _Files)
            {
                byteTotal += fi.Length;
                fileCount += 1;
            }

            Stats insert = new Stats();

            // make each at four times the total size to test internal fragmentations
            
            // start with the halfs
            half = new Memory(.5, 0, 4 * byteTotal, (int) Math.Floor((double)(4 * byteTotal / 512)), 0, false);

            // for the next 10 seconds, generate random inserts and random removals of data to this set
            Stopwatch sw = new Stopwatch();
            Random rand = new Random();
            sw.Start();
            while (sw.ElapsedMilliseconds < 10000)
            {
                half.Insert(_Files[rand.Next((int)fileCount)].Length, _Files[rand.Next((int)fileCount)].Name);
                half.Remove(_Files[rand.Next((int)fileCount)].Name);
            }

            insert.Name = "Memory - Standard Paging";
            insert.Grouping = "512";
            insert.ParameterList = new Tuple<double,double>(half.Get_Percent_Of_Page_Faults(),half.External_Fragmentation_Percent());
            _StatusReport.Add(insert);
            insert = new Stats();
            sw.Reset();
            // rinse and repeat

            one = new Memory(1, 0, 4 * byteTotal, (int)Math.Floor((double)(4 * byteTotal / 1024)), 0, false);
            sw.Start();
            while (sw.ElapsedMilliseconds < 10000)
            {
                one.Insert(_Files[rand.Next((int)fileCount)].Length, _Files[rand.Next((int)fileCount)].Name);
                one.Remove(_Files[rand.Next((int)fileCount)].Name);
            }

            insert.Name = "Memory - Standard Paging";
            insert.Grouping = "1024";
            insert.ParameterList = new Tuple<double, double>(one.Get_Percent_Of_Page_Faults(), one.External_Fragmentation_Percent());
            _StatusReport.Add(insert);
            insert = new Stats();
            sw.Reset();
            // rinse and repeat

            two = new Memory(2, 0, 4 * byteTotal, (int)Math.Floor((double)(4 * byteTotal / 2048)), 0, false);
            sw.Start();
            while (sw.ElapsedMilliseconds < 10000)
            {
                two.Insert(_Files[rand.Next((int)fileCount)].Length, _Files[rand.Next((int)fileCount)].Name);
                two.Remove(_Files[rand.Next((int)fileCount)].Name);
            }

            insert.Name = "Memory - Standard Paging";
            insert.Grouping = "2048";
            insert.ParameterList = new Tuple<double, double>(two.Get_Percent_Of_Page_Faults(), two.External_Fragmentation_Percent());
            _StatusReport.Add(insert);
            insert = new Stats();
            sw.Reset();
            // rinse and repeat

            four = new Memory(4, 0, 4 * byteTotal, (int)Math.Floor((double)(4 * byteTotal / 4096)), 0, false);
            sw.Start();
            while (sw.ElapsedMilliseconds < 10000)
            {
                four.Insert(_Files[rand.Next((int)fileCount)].Length, _Files[rand.Next((int)fileCount)].Name);
                four.Remove(_Files[rand.Next((int)fileCount)].Name);
            }

            insert.Name = "Memory - Standard Paging";
            insert.Grouping = "4096";
            insert.ParameterList = new Tuple<double, double>(four.Get_Percent_Of_Page_Faults(), four.External_Fragmentation_Percent());
            _StatusReport.Add(insert);
            insert = new Stats();
            sw.Reset();
            // when complete notify gui
            Notify_Complete();
        }

        public void Run_Paging_Rotation_FIFO()
        {
            // run each block sim on these as : 512byte, 1024, 2048, 4096
            Memory half, one, two, four;
            // calculate size of files so we know how big to make these
            long byteTotal, fileCount;
            fileCount = byteTotal = 0;
            foreach (FileInfo fi in _Files)
            {
                byteTotal += fi.Length;
                fileCount += 1;
            }

            Stats insert = new Stats();

            // make each at four times the total size to test internal fragmentations

            // start with the halfs
            half = new Memory(.5, 0, 4 * byteTotal, (int)Math.Floor((double)(4 * byteTotal / 512)), 0, true);

            // for the next 10 seconds, generate random inserts and random removals of data to this set
            Stopwatch sw = new Stopwatch();
            Random rand = new Random();
            sw.Start();
            while (sw.ElapsedMilliseconds < 10000)
            {
                half.Insert(_Files[rand.Next((int)fileCount)].Length, _Files[rand.Next((int)fileCount)].Name);
                half.Remove(_Files[rand.Next((int)fileCount)].Name);
            }

            insert.Name = "Memory - Standard Paging";
            insert.Grouping = "512";
            insert.ParameterList = new Tuple<double, double>(half.Get_Percent_Of_Page_Faults(), half.External_Fragmentation_Percent());
            _StatusReport.Add(insert);
            insert = new Stats();
            sw.Reset();
            // rinse and repeat

            one = new Memory(1, 0, 4 * byteTotal, (int)Math.Floor((double)(4 * byteTotal / 1024)), 0, true);
            sw.Start();
            while (sw.ElapsedMilliseconds < 10000)
            {
                one.Insert(_Files[rand.Next((int)fileCount)].Length, _Files[rand.Next((int)fileCount)].Name);
                one.Remove(_Files[rand.Next((int)fileCount)].Name);
            }

            insert.Name = "Memory - Standard Paging";
            insert.Grouping = "1024";
            insert.ParameterList = new Tuple<double, double>(one.Get_Percent_Of_Page_Faults(), one.External_Fragmentation_Percent());
            _StatusReport.Add(insert);
            insert = new Stats();
            sw.Reset();
            // rinse and repeat

            two = new Memory(2, 0, 4 * byteTotal, (int)Math.Floor((double)(4 * byteTotal / 2048)), 0, true);
            sw.Start();
            while (sw.ElapsedMilliseconds < 10000)
            {
                two.Insert(_Files[rand.Next((int)fileCount)].Length, _Files[rand.Next((int)fileCount)].Name);
                two.Remove(_Files[rand.Next((int)fileCount)].Name);
            }

            insert.Name = "Memory - Standard Paging";
            insert.Grouping = "2048";
            insert.ParameterList = new Tuple<double, double>(two.Get_Percent_Of_Page_Faults(), two.External_Fragmentation_Percent());
            _StatusReport.Add(insert);
            insert = new Stats();
            sw.Reset();
            // rinse and repeat

            four = new Memory(4, 0, 4 * byteTotal, (int)Math.Floor((double)(4 * byteTotal / 4096)), 0, true);
            sw.Start();
            while (sw.ElapsedMilliseconds < 10000)
            {
                four.Insert(_Files[rand.Next((int)fileCount)].Length, _Files[rand.Next((int)fileCount)].Name);
                four.Remove(_Files[rand.Next((int)fileCount)].Name);
            }

            insert.Name = "Memory - Standard Paging";
            insert.Grouping = "4096";
            insert.ParameterList = new Tuple<double, double>(four.Get_Percent_Of_Page_Faults(), four.External_Fragmentation_Percent());
            _StatusReport.Add(insert);
            insert = new Stats();

            // when complete notify gui
            Notify_Complete();
        }

        public void Run_Paging_Rotation_LRU()
        {
            // run each block sim on these as : 512byte, 1024, 2048, 4096
            Memory half, one, two, four;
            // calculate size of files so we know how big to make these
            long byteTotal, fileCount;
            fileCount = byteTotal = 0;
            foreach (FileInfo fi in _Files)
            {
                byteTotal += fi.Length;
                fileCount += 1;
            }

            Stats insert = new Stats();

            // make each at four times the total size to test internal fragmentations

            // start with the halfs
            half = new Memory(.5, 0, 4 * byteTotal, (int)Math.Floor((double)(4 * byteTotal / 512)), 0, true, 1);

            // for the next 10 seconds, generate random inserts and random removals of data to this set
            Stopwatch sw = new Stopwatch();
            Random rand = new Random();
            sw.Start();
            while (sw.ElapsedMilliseconds < 10000)
            {
                half.Insert(_Files[rand.Next((int)fileCount)].Length, _Files[rand.Next((int)fileCount)].Name);
                half.Remove(_Files[rand.Next((int)fileCount)].Name);
            }

            insert.Name = "Memory - Standard Paging";
            insert.Grouping = "512";
            insert.ParameterList = new Tuple<double, double>(half.Get_Percent_Of_Page_Faults(), half.External_Fragmentation_Percent());
            _StatusReport.Add(insert);
            insert = new Stats();
            sw.Reset();
            // rinse and repeat

            one = new Memory(1, 0, 4 * byteTotal, (int)Math.Floor((double)(4 * byteTotal / 1024)), 0, true, 1);
            sw.Start();
            while (sw.ElapsedMilliseconds < 10000)
            {
                one.Insert(_Files[rand.Next((int)fileCount)].Length, _Files[rand.Next((int)fileCount)].Name);
                one.Remove(_Files[rand.Next((int)fileCount)].Name);
            }

            insert.Name = "Memory - Standard Paging";
            insert.Grouping = "1024";
            insert.ParameterList = new Tuple<double, double>(one.Get_Percent_Of_Page_Faults(), one.External_Fragmentation_Percent());
            _StatusReport.Add(insert);
            insert = new Stats();
            sw.Reset();
            // rinse and repeat

            two = new Memory(2, 0, 4 * byteTotal, (int)Math.Floor((double)(4 * byteTotal / 2048)), 0, true, 1);
            sw.Start();
            while (sw.ElapsedMilliseconds < 10000)
            {
                two.Insert(_Files[rand.Next((int)fileCount)].Length, _Files[rand.Next((int)fileCount)].Name);
                two.Remove(_Files[rand.Next((int)fileCount)].Name);
            }

            insert.Name = "Memory - Standard Paging";
            insert.Grouping = "2048";
            insert.ParameterList = new Tuple<double, double>(two.Get_Percent_Of_Page_Faults(), two.External_Fragmentation_Percent());
            _StatusReport.Add(insert);
            insert = new Stats();
            sw.Reset();
            // rinse and repeat

            four = new Memory(4, 0, 4 * byteTotal, (int)Math.Floor((double)(4 * byteTotal / 4096)), 0, true, 1);
            sw.Start();
            while (sw.ElapsedMilliseconds < 10000)
            {
                four.Insert(_Files[rand.Next((int)fileCount)].Length, _Files[rand.Next((int)fileCount)].Name);
                four.Remove(_Files[rand.Next((int)fileCount)].Name);
            }

            insert.Name = "Memory - Standard Paging";
            insert.Grouping = "4096";
            insert.ParameterList = new Tuple<double, double>(four.Get_Percent_Of_Page_Faults(), four.External_Fragmentation_Percent());
            _StatusReport.Add(insert);
            insert = new Stats();

            // when complete notify gui
            Notify_Complete();
        }

        public void Run_Paging_Rotation_LFU()
        {
            // run each block sim on these as : 512byte, 1024, 2048, 4096
            Memory half, one, two, four;
            // calculate size of files so we know how big to make these
            long byteTotal, fileCount;
            fileCount = byteTotal = 0;
            foreach (FileInfo fi in _Files)
            {
                byteTotal += fi.Length;
                fileCount += 1;
            }

            Stats insert = new Stats();

            // make each at four times the total size to test internal fragmentations

            // start with the halfs
            half = new Memory(.5, 0, 4 * byteTotal, (int)Math.Floor((double)(4 * byteTotal / 512)), 0, true, 2);

            // for the next 10 seconds, generate random inserts and random removals of data to this set
            Stopwatch sw = new Stopwatch();
            Random rand = new Random();
            sw.Start();
            while (sw.ElapsedMilliseconds < 10000)
            {
                half.Insert(_Files[rand.Next((int)fileCount)].Length, _Files[rand.Next((int)fileCount)].Name);
                half.Remove(_Files[rand.Next((int)fileCount)].Name);
            }

            insert.Name = "Memory - Standard Paging";
            insert.Grouping = "512";
            insert.ParameterList = new Tuple<double, double>(half.Get_Percent_Of_Page_Faults(), half.External_Fragmentation_Percent());
            _StatusReport.Add(insert);
            insert = new Stats();
            sw.Reset();
            // rinse and repeat

            one = new Memory(1, 0, 4 * byteTotal, (int)Math.Floor((double)(4 * byteTotal / 1024)), 0, true, 2);
            sw.Start();
            while (sw.ElapsedMilliseconds < 10000)
            {
                one.Insert(_Files[rand.Next((int)fileCount)].Length, _Files[rand.Next((int)fileCount)].Name);
                one.Remove(_Files[rand.Next((int)fileCount)].Name);
            }

            insert.Name = "Memory - Standard Paging";
            insert.Grouping = "1024";
            insert.ParameterList = new Tuple<double, double>(one.Get_Percent_Of_Page_Faults(), one.External_Fragmentation_Percent());
            _StatusReport.Add(insert);
            insert = new Stats();
            sw.Reset();
            // rinse and repeat

            two = new Memory(2, 0, 4 * byteTotal, (int)Math.Floor((double)(4 * byteTotal / 2048)), 0, true, 2);
            sw.Start();
            while (sw.ElapsedMilliseconds < 10000)
            {
                two.Insert(_Files[rand.Next((int)fileCount)].Length, _Files[rand.Next((int)fileCount)].Name);
                two.Remove(_Files[rand.Next((int)fileCount)].Name);
            }

            insert.Name = "Memory - Standard Paging";
            insert.Grouping = "2048";
            insert.ParameterList = new Tuple<double, double>(two.Get_Percent_Of_Page_Faults(), two.External_Fragmentation_Percent());
            _StatusReport.Add(insert);
            insert = new Stats();
            sw.Reset();
            // rinse and repeat

            four = new Memory(4, 0, 4 * byteTotal, (int)Math.Floor((double)(4 * byteTotal / 4096)), 0, true, 2);
            sw.Start();
            while (sw.ElapsedMilliseconds < 10000)
            {
                four.Insert(_Files[rand.Next((int)fileCount)].Length, _Files[rand.Next((int)fileCount)].Name);
                four.Remove(_Files[rand.Next((int)fileCount)].Name);
            }

            insert.Name = "Memory - Standard Paging";
            insert.Grouping = "4096";
            insert.ParameterList = new Tuple<double, double>(four.Get_Percent_Of_Page_Faults(), four.External_Fragmentation_Percent());
            _StatusReport.Add(insert);
            insert = new Stats();

            // when complete notify gui
            Notify_Complete();
        }

        public void Run_Paging_Rotation_Optomal()
        {
            // run each block sim on these as : 512byte, 1024, 2048, 4096
            Memory half, one, two, four;
            // calculate size of files so we know how big to make these
            long byteTotal, fileCount;
            fileCount = byteTotal = 0;
            foreach (FileInfo fi in _Files)
            {
                byteTotal += fi.Length;
                fileCount += 1;
            }

            Stats insert = new Stats();

            // make each at four times the total size to test internal fragmentations

            // start with the halfs
            half = new Memory(.5, 0, 4 * byteTotal, (int)Math.Floor((double)(4 * byteTotal / 512)), 0, true, 3);

            // for the next 10 seconds, generate random inserts and random removals of data to this set
            Stopwatch sw = new Stopwatch();
            Random rand = new Random();
            sw.Start();
            while (sw.ElapsedMilliseconds < 10000)
            {
                half.Insert(_Files[rand.Next((int)fileCount)].Length, _Files[rand.Next((int)fileCount)].Name);
                half.Remove(_Files[rand.Next((int)fileCount)].Name);
            }

            insert.Name = "Memory - Standard Paging";
            insert.Grouping = "512";
            insert.ParameterList = new Tuple<double, double>(half.Get_Percent_Of_Page_Faults(), half.External_Fragmentation_Percent());
            _StatusReport.Add(insert);
            insert = new Stats();
            sw.Reset();
            // rinse and repeat

            one = new Memory(1, 0, 4 * byteTotal, (int)Math.Floor((double)(4 * byteTotal / 1024)), 0, true, 3);
            sw.Start();
            while (sw.ElapsedMilliseconds < 10000)
            {
                one.Insert(_Files[rand.Next((int)fileCount)].Length, _Files[rand.Next((int)fileCount)].Name);
                one.Remove(_Files[rand.Next((int)fileCount)].Name);
            }

            insert.Name = "Memory - Standard Paging";
            insert.Grouping = "1024";
            insert.ParameterList = new Tuple<double, double>(one.Get_Percent_Of_Page_Faults(), one.External_Fragmentation_Percent());
            _StatusReport.Add(insert);
            insert = new Stats();
            sw.Reset();
            // rinse and repeat

            two = new Memory(2, 0, 4 * byteTotal, (int)Math.Floor((double)(4 * byteTotal / 2048)), 0, true, 3);
            sw.Start();
            while (sw.ElapsedMilliseconds < 10000)
            {
                two.Insert(_Files[rand.Next((int)fileCount)].Length, _Files[rand.Next((int)fileCount)].Name);
                two.Remove(_Files[rand.Next((int)fileCount)].Name);
            }

            insert.Name = "Memory - Standard Paging";
            insert.Grouping = "2048";
            insert.ParameterList = new Tuple<double, double>(two.Get_Percent_Of_Page_Faults(), two.External_Fragmentation_Percent());
            _StatusReport.Add(insert);
            insert = new Stats();
            sw.Reset();
            // rinse and repeat

            four = new Memory(4, 0, 4 * byteTotal, (int)Math.Floor((double)(4 * byteTotal / 4096)), 0, true, 3);
            sw.Start();
            while (sw.ElapsedMilliseconds < 10000)
            {
                four.Insert(_Files[rand.Next((int)fileCount)].Length, _Files[rand.Next((int)fileCount)].Name);
                four.Remove(_Files[rand.Next((int)fileCount)].Name);
            }

            insert.Name = "Memory - Standard Paging";
            insert.Grouping = "4096";
            insert.ParameterList = new Tuple<double, double>(four.Get_Percent_Of_Page_Faults(), four.External_Fragmentation_Percent());
            _StatusReport.Add(insert);
            insert = new Stats();

            // when complete notify gui
            Notify_Complete();
        }

        public void Run_Paging_Rotation_Second()
        {
            // run each block sim on these as : 512byte, 1024, 2048, 4096
            Memory half, one, two, four;
            // calculate size of files so we know how big to make these
            long byteTotal, fileCount;
            fileCount = byteTotal = 0;
            foreach (FileInfo fi in _Files)
            {
                byteTotal += fi.Length;
                fileCount += 1;
            }

            Stats insert = new Stats();

            // make each at four times the total size to test internal fragmentations

            // start with the halfs
            half = new Memory(.5, 0, 4 * byteTotal, (int)Math.Floor((double)(4 * byteTotal / 512)), 0, true, 4);

            // for the next 10 seconds, generate random inserts and random removals of data to this set
            Stopwatch sw = new Stopwatch();
            Random rand = new Random();
            sw.Start();
            while (sw.ElapsedMilliseconds < 10000)
            {
                half.Insert(_Files[rand.Next((int)fileCount)].Length, _Files[rand.Next((int)fileCount)].Name);
                half.Remove(_Files[rand.Next((int)fileCount)].Name);
            }

            insert.Name = "Memory - Standard Paging";
            insert.Grouping = "512";
            insert.ParameterList = new Tuple<double, double>(half.Get_Percent_Of_Page_Faults(), half.External_Fragmentation_Percent());
            _StatusReport.Add(insert);
            insert = new Stats();
            sw.Reset();
            // rinse and repeat

            one = new Memory(1, 0, 4 * byteTotal, (int)Math.Floor((double)(4 * byteTotal / 1024)), 0, true, 4);
            sw.Start();
            while (sw.ElapsedMilliseconds < 10000)
            {
                one.Insert(_Files[rand.Next((int)fileCount)].Length, _Files[rand.Next((int)fileCount)].Name);
                one.Remove(_Files[rand.Next((int)fileCount)].Name);
            }

            insert.Name = "Memory - Standard Paging";
            insert.Grouping = "1024";
            insert.ParameterList = new Tuple<double, double>(one.Get_Percent_Of_Page_Faults(), one.External_Fragmentation_Percent());
            _StatusReport.Add(insert);
            insert = new Stats();
            sw.Reset();
            // rinse and repeat

            two = new Memory(2, 0, 4 * byteTotal, (int)Math.Floor((double)(4 * byteTotal / 2048)), 0, true, 4);
            sw.Start();
            while (sw.ElapsedMilliseconds < 10000)
            {
                two.Insert(_Files[rand.Next((int)fileCount)].Length, _Files[rand.Next((int)fileCount)].Name);
                two.Remove(_Files[rand.Next((int)fileCount)].Name);
            }

            insert.Name = "Memory - Standard Paging";
            insert.Grouping = "2048";
            insert.ParameterList = new Tuple<double, double>(two.Get_Percent_Of_Page_Faults(), two.External_Fragmentation_Percent());
            _StatusReport.Add(insert);
            insert = new Stats();
            sw.Reset();
            // rinse and repeat

            four = new Memory(4, 0, 4 * byteTotal, (int)Math.Floor((double)(4 * byteTotal / 4096)), 0, true, 4);
            sw.Start();
            while (sw.ElapsedMilliseconds < 10000)
            {
                four.Insert(_Files[rand.Next((int)fileCount)].Length, _Files[rand.Next((int)fileCount)].Name);
                four.Remove(_Files[rand.Next((int)fileCount)].Name);
            }

            insert.Name = "Memory - Standard Paging";
            insert.Grouping = "4096";
            insert.ParameterList = new Tuple<double, double>(four.Get_Percent_Of_Page_Faults(), four.External_Fragmentation_Percent());
            _StatusReport.Add(insert);
            insert = new Stats();

            // when complete notify gui
            Notify_Complete();
        }

        public void Run_Paging_Rotation_Clock()
        {
            // run each block sim on these as : 512byte, 1024, 2048, 4096
            Memory half, one, two, four;
            // calculate size of files so we know how big to make these
            long byteTotal, fileCount;
            fileCount = byteTotal = 0;
            foreach (FileInfo fi in _Files)
            {
                byteTotal += fi.Length;
                fileCount += 1;
            }

            Stats insert = new Stats();

            // make each at four times the total size to test internal fragmentations

            // start with the halfs
            half = new Memory(.5, 0, 4 * byteTotal, (int)Math.Floor((double)(4 * byteTotal / 512)), 0, true, 5);

            // for the next 10 seconds, generate random inserts and random removals of data to this set
            Stopwatch sw = new Stopwatch();
            Random rand = new Random();
            sw.Start();
            while (sw.ElapsedMilliseconds < 10000)
            {
                half.Insert(_Files[rand.Next((int)fileCount)].Length, _Files[rand.Next((int)fileCount)].Name);
                half.Remove(_Files[rand.Next((int)fileCount)].Name);
            }

            insert.Name = "Memory - Standard Paging";
            insert.Grouping = "512";
            insert.ParameterList = new Tuple<double, double>(half.Get_Percent_Of_Page_Faults(), half.External_Fragmentation_Percent());
            _StatusReport.Add(insert);
            insert = new Stats();
            sw.Reset();
            // rinse and repeat

            one = new Memory(1, 0, 4 * byteTotal, (int)Math.Floor((double)(4 * byteTotal / 1024)), 0, true, 5);
            sw.Start();
            while (sw.ElapsedMilliseconds < 10000)
            {
                one.Insert(_Files[rand.Next((int)fileCount)].Length, _Files[rand.Next((int)fileCount)].Name);
                one.Remove(_Files[rand.Next((int)fileCount)].Name);
            }

            insert.Name = "Memory - Standard Paging";
            insert.Grouping = "1024";
            insert.ParameterList = new Tuple<double, double>(one.Get_Percent_Of_Page_Faults(), one.External_Fragmentation_Percent());
            _StatusReport.Add(insert);
            insert = new Stats();
            sw.Reset();
            // rinse and repeat

            two = new Memory(2, 0, 4 * byteTotal, (int)Math.Floor((double)(4 * byteTotal / 2048)), 0, true, 5);
            sw.Start();
            while (sw.ElapsedMilliseconds < 10000)
            {
                two.Insert(_Files[rand.Next((int)fileCount)].Length, _Files[rand.Next((int)fileCount)].Name);
                two.Remove(_Files[rand.Next((int)fileCount)].Name);
            }

            insert.Name = "Memory - Standard Paging";
            insert.Grouping = "2048";
            insert.ParameterList = new Tuple<double, double>(two.Get_Percent_Of_Page_Faults(), two.External_Fragmentation_Percent());
            _StatusReport.Add(insert);
            insert = new Stats();
            sw.Reset();
            // rinse and repeat

            four = new Memory(4, 0, 4 * byteTotal, (int)Math.Floor((double)(4 * byteTotal / 4096)), 0, true, 5);
            sw.Start();
            while (sw.ElapsedMilliseconds < 10000)
            {
                four.Insert(_Files[rand.Next((int)fileCount)].Length, _Files[rand.Next((int)fileCount)].Name);
                four.Remove(_Files[rand.Next((int)fileCount)].Name);
            }

            insert.Name = "Memory - Standard Paging";
            insert.Grouping = "4096";
            insert.ParameterList = new Tuple<double, double>(four.Get_Percent_Of_Page_Faults(), four.External_Fragmentation_Percent());
            _StatusReport.Add(insert);
            insert = new Stats();

            // when complete notify gui
            Notify_Complete();
        }


        private void Notify_Complete()
        {

        }


        public void Get_Pages(out Collection<String> page1, out Collection<String> page2, out Collection<String> page3, out Collection<String> page4)
        {
            page1 = _Page1;
            page2 = _Page2;
            page3 = _Page3;
            page4 = _Page4;
        }
        public List<Stats> Get_Stats()
        {
            return _StatusReport;
        }
    }
}
