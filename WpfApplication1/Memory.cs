using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApplication1;

namespace OS_Simulator
{
    public class Memory
    {
        #region Members & Properties
        // For convention we will allow either blocks of .5Kbytes, 1Kbytes, or 4K
        private double _BlockSize;
        private double _Start;
        private double _End;
        private int _Entries;
        private int _PageCount;
        private double _UsedSpace;
        private string _Name;
        private bool[] _Table;
        private int[] _Age;
        private List<string> _InternalFragReport;
        private List<string> _SecondChanceList;
        private bool _Replace;
        private int _ReplaceMethod;
        private long _PageFaults = 0;
        private long _FailedInserts = 0;
        private long _PageAccesses = 0;
        private Tuple<int, int> _TargetedDeletes; // Locations used for different pages
        private FileInfo[] _AllFiles;
        private List<int> _Points;
        private List<Stats> _StatusReport;

        /* replacement methods are
         * 0 - FIFO
         * 1 - LRU
         * 2 - LFU
         * 3 - Optomal
         * 4 - Second Chance
         * 5 - Clock
         */
        // getter's setter's

        public double Start
        {
            get
            {
                return _Start;
            }
            set
            {
                _Start = value;
            }
        }
        public double End
        {
            get
            {
                return _End;
            }
            set
            {
                _End = value;
            }
        }

        public int Entries
        {

            get
            {
                return _Entries;
            }
            set
            {
                _Entries = value;
            }

        }

        public int PageCount
        {
            get
            {
                return _PageCount;
            }
            set
            {
                _PageCount = value;
            }
        }

        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
            }
        }

        #endregion

        #region Constructors
        public Memory(double size, bool replace, int replaceMethod = 0)
        {
            _SecondChanceList = new List<string>();
            _InternalFragReport = new List<string>();
            if (size != .5 || size != 1 || size != 4)
            {
                _BlockSize = 1;
            }
            else
            {
                _BlockSize = size;
            }
            _Start = _End = _UsedSpace = _Entries = _PageCount = 0;
            _Replace = replace;
            _ReplaceMethod = replaceMethod;
            _Points = new List<int>();
        }

        public Memory(double size, double start, double end, int pages, int entries, bool replace, int replaceMethod = 0)
        {
            _SecondChanceList = new List<string>();
            _InternalFragReport = new List<string>();
            if (size != .5 || size != 2 || size != 1 || size != 4)
            {
                _BlockSize = 1;
            }
            else
            {
                _BlockSize = size;
            }
            _Start = start;
            _End = end;

            _UsedSpace = end - start;

            // error checking
            // if we're not a full size break for pages...
            if ((_End - _Start) % (1024 * size) != 0)
            {
                // bump end up to page size
                _End += ((1024 * size) - (_End % (1024 * size)));
            }

            _Entries = entries;
            _PageCount = pages;

            _Table = new bool[_PageCount];
            _Age = new int[_PageCount];
            
            // initialize the pages to empty;
            for (int i = 0; i < _PageCount; i += 1)
            {
                _Table[i] = false;
            }

            // check if replacement is happening or if this is simple
            _Replace = replace;
            _ReplaceMethod = replaceMethod;
            _Points = new List<int>();
        }

        #endregion

        #region Methods

        public double External_Fragmentation_Percent()
        {
            int used = 0;
            for (int i = 0 ; i < _Table.Length ; i +=1)
            {
                if (_Table[i])
                {
                    used +=1;
                }
            }
            return 100 * (1 - ((double)used / _Table.Length));
        }

        //inserts [default] using first available
        public void Insert(long bytesOfData, string name)
        {
            string insert;
            long blockCount;
            long bytesWasted;
            long blockSize;
            // calculate number of table entries needed

            _PageAccesses += 1;

            if (_InternalFragReport.Contains(name))
            {
                // update used time if 1-4
                if (_ReplaceMethod > 0 && _ReplaceMethod < 5)
                {
                    // get string except end and replace
                    int now = MakeIntTime();
                    string target = _InternalFragReport.FirstOrDefault(o => o.Contains(name));
                    if (target == null)
                    {
                        throw new Exception("WTF");
                    }
                    _InternalFragReport.Remove(target);
                    target = target.Substring(0, target.LastIndexOf(" "));

                    // if the replacement method requires counting
                    if (_ReplaceMethod == 2 || _ReplaceMethod == 4)
                    {
                        // edit the count by increasing by 1                        
                        int count = int.Parse(target.Substring(target.LastIndexOf(" ") + 1));
                        target = target.Substring(0, target.LastIndexOf(" "));
                        target += " " + count.ToString();
                    }

                    // now do the time stamp
                    target += now.ToString();
                    _InternalFragReport.Add(target);
                }

                return;
            }

            // if item needs to be loaded, count the fault

            _PageFaults += 1;

            if (_BlockSize == .5)
            {
                blockCount = ((int)Math.Ceiling((double)(bytesOfData) / 512));
                if (bytesOfData > 512)
                {
                    bytesWasted = bytesOfData % 512;
                }
                else
                {
                    bytesWasted = 512 - bytesOfData;
                }
                blockSize = 512;
            }

            else if (_BlockSize == 1)
            {
                blockCount = ((int)Math.Ceiling((double)(bytesOfData) / 1024));
                if (bytesOfData > 1024)
                {
                    bytesWasted = bytesOfData % 1024;
                }
                else
                {
                    bytesWasted = 1024 - bytesOfData;
                }
                blockSize = 1024;
            }

            else if (_BlockSize == 2)
            {
                blockCount = ((int)Math.Ceiling((double)(bytesOfData) / 2048));
                if (bytesOfData > 2048)
                {
                    bytesWasted = bytesOfData % 2048;
                }
                else
                {
                    bytesWasted = 2048 - bytesOfData;
                }
                blockSize = 2048;
            }

            else //(_BlockSize == 4)
            {
                blockCount = ((int)Math.Ceiling((double)(bytesOfData) / 4096));
                if (bytesOfData > 4096)
                {
                    bytesWasted = bytesOfData % 4096;
                }
                else
                {
                    bytesWasted = 4096 - bytesOfData;
                }
                blockSize = 4096;
            }


            int i = 0;
            // find open location if not already in memory
            while (_Table[i] && i < _PageCount - blockCount && Next_Blocks_Open(i, blockCount))
            {
                i += 1;
            }

            if (!_Replace)
            {
                // failure to insert
                if (i >= _PageCount - blockCount)
                {
                    _FailedInserts += 1;
                    return;
                }

                // insert lives at i
                insert = i.ToString() + " - " + (i + blockCount).ToString() + " "
                         + name + " " + bytesWasted.ToString() + " " + blockSize.ToString() + "1";
                // count of uses      ^
                //mark it used
                for (int j = i; j < i + blockCount; j += 1)
                {
                    _Table[i] = true;
                }

                _InternalFragReport.Add(insert);
            }
            // if using replacement check for replace method
            else
            {
                if (_ReplaceMethod == 0)
                {
                    // if replacement is needed
                    if (i >= _PageCount - blockCount)
                    {
                        // check on size as FIFO, if less than needed we will cascade through the next n pages
                        // to get space enought to build

                        int sizeFreed = 0;
                        while (sizeFreed < blockCount)
                        {
                            // find oldest inserts using last value as time of insert in unix time
                            string target = _InternalFragReport.OrderBy(o => int.Parse(o.Substring(o.LastIndexOf(" ")))).First();

                            // strip this block by getting indices
                            int start = int.Parse(target.Substring(0, target.IndexOf(" ")));
                            string temp = target.Substring(target.IndexOf(" - ") + 3);
                            int end = int.Parse(temp.Substring(0, temp.IndexOf(" ")));

                            sizeFreed = end - start;

                            // track removal points (as we need contiguous memory - no ptrs in c#)
                            _Points.Add(start);

                            Remove(start, end);

                            // now remove target from list as it's been nuked
                            _InternalFragReport.Remove(target);
                        }

                    }

                    // if more than one block to be moved
                    while (_Points.Count > 1)
                    {
                        // get data in this section
                        string moved = _InternalFragReport.First(o => o.Contains(_Points.First().ToString() + " - "));
                        
                        int localStart = int.Parse(moved.Substring(0,moved.IndexOf(" ")));

                        // this will be updated, so remove from report
                        _InternalFragReport.Remove(moved);

                        // get it's end
                        string temp = moved.Substring(moved.IndexOf(" - ") + 3);
                        int localEnd = int.Parse(temp.Substring(0, temp.IndexOf(" ")));

                        // toggle table to say used across the new area
                        for (int j = _Points.First(); j < _Points.First() + (localEnd - localStart) + 1; j += 1)
                        {
                            _Table[j] = true;
                        }
                        // free old area
                        for (int j = localStart ; j < localEnd ; j += 1)
                        {
                            _Table[j] = false;
                        }

                        // update string
                        temp = temp.Substring(temp.IndexOf(" "));
                        temp = _Points.ToString() + " " + localEnd + " " + temp;

                        // send back temp
                        _InternalFragReport.Add(temp);
                    }

                    int now = MakeIntTime();
                    // insert lives at i
                    insert = i.ToString() + " - " + (i + blockCount).ToString() + " "
                             + name + " " + bytesWasted.ToString() + " " + blockSize.ToString() + "0" + now.ToString();
                    // count of uses      ^ (pointless here)
                    //mark it used
                    for (int j = i; j < i + blockCount; j += 1)
                    {
                        _Table[i] = true;
                    }

                    _InternalFragReport.Add(insert);
                }

                // modified version of above as we log when we have used last, see top

                else if (_ReplaceMethod == 1)
                {
                    // if replacement is needed
                    if (i >= _PageCount - blockCount)
                    {
                        // check on size as FIFO, if less than needed we will cascade through the next n pages
                        // to get space enought to build

                        int sizeFreed = 0;
                        while (sizeFreed < blockCount)
                        {
                            // find smalleset used and move that to the front of the string
                            string target = _InternalFragReport.OrderBy(o => int.Parse(o.Substring(o.LastIndexOf(" ")))).First();

                            // strip this block by getting indices
                            int start = int.Parse(target.Substring(0, target.IndexOf(" ")));
                            string temp = target.Substring(target.IndexOf(" - ") + 3);
                            int end = int.Parse(temp.Substring(0, temp.IndexOf(" ")));

                            sizeFreed = end - start;

                            // track removal points (as we need contiguous memory - no ptrs in c#)
                            _Points.Add(start);

                            Remove(start, end);

                            // now remove target from list as it's been nuked
                            _InternalFragReport.Remove(target);
                        }

                    }

                    // if more than one block to be moved
                    while (_Points.Count > 1)
                    {
                        // get data in this section
                        string moved = _InternalFragReport.First(o => o.Contains(_Points.First().ToString() + " - "));

                        int localStart = int.Parse(moved.Substring(0, moved.IndexOf(" ")));

                        // this will be updated, so remove from report
                        _InternalFragReport.Remove(moved);

                        // get it's end
                        string temp = moved.Substring(moved.IndexOf(" - ") + 3);
                        int localEnd = int.Parse(temp.Substring(0, temp.IndexOf(" ")));

                        // toggle table to say used across the new area
                        for (int j = _Points.First(); j < _Points.First() + (localEnd - localStart) + 1; j += 1)
                        {
                            _Table[j] = true;
                        }
                        // free old area
                        for (int j = localStart; j < localEnd; j += 1)
                        {
                            _Table[j] = false;
                        }

                        // update string
                        temp = temp.Substring(temp.IndexOf(" "));
                        temp = _Points.ToString() + " " + localEnd + " " + temp;

                        // send back temp
                        _InternalFragReport.Add(temp);
                    }
                    int now = MakeIntTime();
                    // insert lives at i
                    insert = i.ToString() + " - " + (i + blockCount).ToString() + " "
                             + name + " " + bytesWasted.ToString() + " " + blockSize.ToString() + "1" + now.ToString();
                    // count of uses      ^ (pointless here)
                    //mark it used
                    for (int j = i; j < i + blockCount; j += 1)
                    {
                        _Table[i] = true;
                    }

                    _InternalFragReport.Add(insert);
                }

                // LFU
                else if (_ReplaceMethod == 2)
                {
                    // if replacement is needed
                    if (i >= _PageCount - blockCount)
                    {
                        // check on size as FIFO, if less than needed we will cascade through the next n pages
                        // to get space enought to build

                        int sizeFreed = 0;
                        while (sizeFreed < blockCount)
                        {
                            // find smalleset used and move that to the front of the string
                            string target = _InternalFragReport.OrderBy(o => Get_Used_Count(o)).First();

                            // strip this block by getting indices
                            int start = int.Parse(target.Substring(0, target.IndexOf(" ")));
                            string temp = target.Substring(target.IndexOf(" - ") + 3);
                            int end = int.Parse(temp.Substring(0, temp.IndexOf(" ")));

                            sizeFreed = end - start;

                            // track removal points (as we need contiguous memory - no ptrs in c#)
                            _Points.Add(start);

                            Remove(start, end);

                            // now remove target from list as it's been nuked
                            _InternalFragReport.Remove(target);
                        }

                    }

                    // if more than one block to be moved
                    while (_Points.Count > 1)
                    {
                        // get data in this section
                        string moved = _InternalFragReport.First(o => o.Contains(_Points.First().ToString() + " - "));

                        int localStart = int.Parse(moved.Substring(0, moved.IndexOf(" ")));

                        // this will be updated, so remove from report
                        _InternalFragReport.Remove(moved);

                        // get it's end
                        string temp = moved.Substring(moved.IndexOf(" - ") + 3);
                        int localEnd = int.Parse(temp.Substring(0, temp.IndexOf(" ")));

                        // toggle table to say used across the new area
                        for (int j = _Points.First(); j < _Points.First() + (localEnd - localStart) + 1; j += 1)
                        {
                            _Table[j] = true;
                        }
                        // free old area
                        for (int j = localStart; j < localEnd; j += 1)
                        {
                            _Table[j] = false;
                        }

                        // update string
                        temp = temp.Substring(temp.IndexOf(" "));
                        temp = _Points.ToString() + " " + localEnd + " " + temp;

                        // send back temp
                        _InternalFragReport.Add(temp);
                    }
                    int now = MakeIntTime();
                    // insert lives at i
                    insert = i.ToString() + " - " + (i + blockCount).ToString() + " "
                             + name + " " + bytesWasted.ToString() + " " + blockSize.ToString() + "1" + now.ToString();
                    // count of uses      ^ (pointless here)
                    //mark it used
                    for (int j = i; j < i + blockCount; j += 1)
                    {
                        _Table[i] = true;
                    }

                    _InternalFragReport.Add(insert);
                }

                // Optimal
                else if (_ReplaceMethod == 3)
                {
                    if (i >= _PageCount - blockCount)
                    {
                        // find last used by optomal
                        int sizeFreed = 0;
                        while (sizeFreed < blockCount)
                        {
                            // get the string to kill
                            string find = Get_Next_To_Kill();

                            string target = _InternalFragReport.First(o=>o.Contains(find));

                            // strip this block by getting indices
                            int start = int.Parse(target.Substring(0, target.IndexOf(" ")));
                            string temp = target.Substring(target.IndexOf(" - ") + 3);
                            int end = int.Parse(temp.Substring(0, temp.IndexOf(" ")));

                            sizeFreed = end - start;

                            // track removal points (as we need contiguous memory - no ptrs in c#)
                            _Points.Add(start);

                            Remove(start, end);

                            // now remove target from list as it's been nuked
                            _InternalFragReport.Remove(target);
                        }

                    }

                    // if more than one block to be moved
                    while (_Points.Count > 1)
                    {
                        // get data in this section
                        string moved = _InternalFragReport.First(o => o.Contains(_Points.First().ToString() + " - "));

                        int localStart = int.Parse(moved.Substring(0, moved.IndexOf(" ")));

                        // this will be updated, so remove from report
                        _InternalFragReport.Remove(moved);

                        // get it's end
                        string temp = moved.Substring(moved.IndexOf(" - ") + 3);
                        int localEnd = int.Parse(temp.Substring(0, temp.IndexOf(" ")));

                        // toggle table to say used across the new area
                        for (int j = _Points.First(); j < _Points.First() + (localEnd - localStart) + 1; j += 1)
                        {
                            _Table[j] = true;
                        }
                        // free old area
                        for (int j = localStart; j < localEnd; j += 1)
                        {
                            _Table[j] = false;
                        }

                        // update string
                        temp = temp.Substring(temp.IndexOf(" "));
                        temp = _Points.ToString() + " " + localEnd + " " + temp;

                        // send back temp
                        _InternalFragReport.Add(temp);
                    }
                    // insert lives at i
                    insert = i.ToString() + " - " + (i + blockCount).ToString() + " "
                             + name + " " + bytesWasted.ToString() + " " + blockSize.ToString() + "1";
                    // count of uses      ^
                    //mark it used
                    for (int j = i; j < i + blockCount; j += 1)
                    {
                        _Table[i] = true;
                    }

                    _InternalFragReport.Add(insert);
                }

                //Second Chance
                else if (_ReplaceMethod == 4)
                {
                    // if replacement is needed
                    if (i >= _PageCount - blockCount)
                    {

                        int sizeFreed = 0;
                        while (sizeFreed < blockCount)
                        {
                            // for each item in the list, check if in the twice checked list
                            foreach (string s in _InternalFragReport)
                            {
                                // delete it
                                if (_SecondChanceList.Contains(s))
                                {
                                    string target = s;
                                    // strip this block by getting indices
                                    int start = int.Parse(target.Substring(0, target.IndexOf(" ")));
                                    string temp = target.Substring(target.IndexOf(" - ") + 3);
                                    int end = int.Parse(temp.Substring(0, temp.IndexOf(" ")));

                                    sizeFreed = end - start;


                                    // track removal points (as we need contiguous memory - no ptrs in c#)
                                    _Points.Add(start);

                                    Remove(start, end);

                                    // now remove target from list as it's been nuked
                                    _InternalFragReport.Remove(target);

                                    // as done, break to recheck or insert and finish
                                    break;
                                }
                                // add it
                                else
                                {
                                    _SecondChanceList.Add(s);
                                }
                            }
                        }
                    }

                    // if more than one block to be moved
                    while (_Points.Count > 1)
                    {
                        // get data in this section
                        string moved = _InternalFragReport.First(o => o.Contains(_Points.First().ToString() + " - "));
                        
                        int localStart = int.Parse(moved.Substring(0,moved.IndexOf(" ")));

                        // this will be updated, so remove from report
                        _InternalFragReport.Remove(moved);

                        // get it's end
                        string temp = moved.Substring(moved.IndexOf(" - ") + 3);
                        int localEnd = int.Parse(temp.Substring(0, temp.IndexOf(" ")));

                        // toggle table to say used across the new area
                        for (int j = _Points.First(); j < _Points.First() + (localEnd - localStart) + 1; j += 1)
                        {
                            _Table[j] = true;
                        }
                        // free old area
                        for (int j = localStart ; j < localEnd ; j += 1)
                        {
                            _Table[j] = false;
                        }

                        // update string
                        temp = temp.Substring(temp.IndexOf(" "));
                        temp = _Points.ToString() + " " + localEnd + " " + temp;

                        // send back temp
                        _InternalFragReport.Add(temp);
                    }
                    int now = MakeIntTime();
                    // insert lives at i
                    insert = i.ToString() + " - " + (i + blockCount).ToString() + " "
                             + name + " " + bytesWasted.ToString() + " " + blockSize.ToString() + "1" + now.ToString();
                    // count of uses      ^ (pointless here)
                    //mark it used
                    for (int j = i; j < i + blockCount; j += 1)
                    {
                        _Table[i] = true;
                    }

                    _InternalFragReport.Add(insert);
                }

                // Clock
                else if (_ReplaceMethod == 5)
                {
                    // if replacement is needed
                    if (i >= _PageCount - blockCount)
                    {
                        // check on size as FIFO, if less than needed we will cascade through the next n pages
                        // to get space enought to build

                        int sizeFreed = 0;
                        while (sizeFreed < blockCount)
                        {
                            // find oldest insert using age table
                            int largest = _Age.Max();
                            int address = _Age.ToList().IndexOf(largest);
                            
                            string target = _InternalFragReport.OrderBy(o => int.Parse(o.Substring(o.IndexOf(" "))) == address).First();

                            // strip this block by getting indices
                            int start = int.Parse(target.Substring(0, target.IndexOf(" ")));
                            string temp = target.Substring(target.IndexOf(" - ") + 3);
                            int end = int.Parse(temp.Substring(0, temp.IndexOf(" ")));

                            sizeFreed = end - start;

                            // track removal points (as we need contiguous memory - no ptrs in c#)
                            _Points.Add(start);

                            Remove(start, end);

                            // now remove target from list as it's been nuked
                            _InternalFragReport.Remove(target);
                        }

                    }

                    // if more than one block to be moved
                    while (_Points.Count > 1)
                    {
                        // get data in this section
                        string moved = _InternalFragReport.First(o => o.Contains(_Points.First().ToString() + " - "));

                        int localStart = int.Parse(moved.Substring(0, moved.IndexOf(" ")));

                        // this will be updated, so remove from report
                        _InternalFragReport.Remove(moved);

                        // get it's end
                        string temp = moved.Substring(moved.IndexOf(" - ") + 3);
                        int localEnd = int.Parse(temp.Substring(0, temp.IndexOf(" ")));

                        // toggle table to say used across the new area
                        for (int j = _Points.First(); j < _Points.First() + (localEnd - localStart) + 1; j += 1)
                        {
                            _Table[j] = true;
                        }
                        // free old area
                        for (int j = localStart; j < localEnd; j += 1)
                        {
                            _Table[j] = false;
                        }

                        // update string
                        temp = temp.Substring(temp.IndexOf(" "));
                        temp = _Points.ToString() + " " + localEnd + " " + temp;

                        // send back temp
                        _InternalFragReport.Add(temp);
                    }
                    int now = MakeIntTime();
                    // insert lives at i
                    insert = i.ToString() + " - " + (i + blockCount).ToString() + " "
                             + name + " " + bytesWasted.ToString() + " " + blockSize.ToString() + "0" + now.ToString();
                    // count of uses      ^ (pointless here)
                    //mark it used
                    for (int j = i; j < i + blockCount; j += 1)
                    {
                        _Table[i] = true;
                        _Age[i] += 1;
                    }

                    //

                    _InternalFragReport.Add(insert);
                }
            }
        }

        private static int MakeIntTime()
        {
            return (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        private bool Next_Blocks_Open(long i, long count)
        {
            // cycle over table
            while (!_Table[i] && i < count)
            {
                i += 1;
            }
            // if we break early, this will return false or if we hit the 
            // end and all were empty including end, return true
            return !_Table[i];
        }

        public void Remove(string name)
        {
            // get the blocks to free
            string target = _InternalFragReport.FirstOrDefault(o => o.Contains(name) && o.Contains("Remov") == false);
            int start, end;
            if (target == null)
            {
                // log the attempted removal of a file not here
                target = "Attempted Removal of nonexistant " + name;
                _InternalFragReport.Add(target);
                return;
            }

            try
            {
                // parse out data
                start = int.Parse(target.Substring(0, target.IndexOf(" ")));
                // make tempstring after hyphen and spaces
                string temp = target.Substring(target.IndexOf(" - ") + 3);
                // get end point
                end = int.Parse(target.Substring(0, temp.IndexOf(" ")));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return;
            }

            // remove by killing the table record (soft delete)

            while (start < end)
            {
                _Table[start] = false;
                start += 1;
            }

            _InternalFragReport.Remove(target);

            // note the removal
            target = "Removed " + target;
            _InternalFragReport.Add(target);
        }
        private void Remove(int start, int end)
        {
            while (start < end)
            {
                _Table[start] = false;
                start += 1;
            }
        }

        public List<string> Get_Report()
        {
            return _InternalFragReport;
        }

        public int Get_Used_Count(string s)
        {
            int index = s.LastIndexOf(" ");
            return int.Parse(s.Substring(s.LastIndexOf(" ", 0, index), index));
        }

        // Omnipresence goes here (knowledge of what files will be done)
        public void Establish_List(FileInfo[] files)
        {
            var temp = new List<FileInfo>();

            Random rand = new Random();

            for (int i = 0; i < 1000 ; i +=1)
            {
                FileInfo fi = files[rand.Next(files.Length)];
                temp.Add(fi);
            }
                // make new
            _AllFiles = new List<FileInfo>(temp).ToArray();
        }

        private string Get_Next_To_Kill()
        {
            Dictionary<string, int> mapper = new Dictionary<string, int>();

            // use page accesses to determine what should be replaced
            for (long i = _PageAccesses; i < _AllFiles.Length ; i +=1)
            {
                // if not in the dictionary
                if (!mapper.ContainsKey(_AllFiles[i].Name))
                {

                    // and if it does not exist in the frag report, we get null back, so put in a 0 value for it
                    if (_InternalFragReport.FirstOrDefault(o=>o.Contains(_AllFiles[i].Name)) == null)
                    {
                        // return this because it is 0 and you don't have less than this
                        return _AllFiles[i].Name;
                    }
                    // else we enter it
                    int index = _InternalFragReport.IndexOf(_AllFiles[i].Name);
                    mapper.Add(_AllFiles[i].Name, index);

                }
            }
            // if we hit here we find the largest index in the mapper
            return mapper.OrderByDescending(o => o.Value).First().Key;
        }

        public double Get_Percent_Of_Page_Faults()
        {
            return ((double)_PageFaults / _PageAccesses);
        }

#endregion
    }
}
