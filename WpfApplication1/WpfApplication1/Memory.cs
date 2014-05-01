﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OS_Simulator
{
    public class Memory
    {
        // For convention we will allow either blocks of .5Kbytes, 1Kbytes, or 4K
        private double _BlockSize;
        private double _Start;
        private double _End;
        private int _Entries;
        private int _PageCount;
        private double _UsedSpace;
        private string _Name;
        private bool[] _Table;
        private List<string> _InternalFragReport;
        private bool _Replace;
        private int _ReplaceMethod;

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

        public Memory(double size, bool replace, int replaceMethod = 0)
        {
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
        }

        public Memory(double size, double start, double end, int pages, int entries, bool replace, int replaceMethod = 0)
        {
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
            if ( (_End - _Start) % (1024 * size) != 0 )
            {
                // bump end up to page size
                _End += ((1024*size) - (_End % (1024 * size)));
            }
            
            _Entries = entries;
            _PageCount = pages;

            _Table = new bool[_PageCount];
            // initialize the pages to empty;
            for (int i = 0; i < _PageCount; i += 1 )
            {
                _Table[i] = false;
            }

            // check if replacement is happening or if this is simple
            _Replace = replace;
            _ReplaceMethod = replaceMethod;
        }

        public double External_Fragmentation_Percent()
        {
            return 100 * (_UsedSpace / (_End - Start));
        }

        //inserts [default] using first available
        public void Insert(long bytesOfData, string name)
        {
            string insert;
            long blockCount;
            long bytesWasted;
            long blockSize;
            // calculate number of table entries needed

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
                // find open location
                while (_Table[i] && i < _PageCount - blockCount && Next_Blocks_Open(i, blockCount))
                {
                    i += 1;
                }

                if (!_Replace)
                {

                    if (i >= _PageCount - blockCount)
                    {
                        insert = "Removing new instance of " + name + ".  Ran out of memory inserting into table.";
                        _InternalFragReport.Add(insert);
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
                            // make scratch locations list
                            List<string> tempLocationsList = new List<string>(_InternalFragReport);

                            // check on size as FIFO, if less than needed we will cascade through the next n pages
                            // to get space enought to build


                            // find oldest inserts using last value as time of insert in unix time
                            string target = tempLocationsList.OrderBy(o => int.Parse(o.Substring(o.LastIndexOf(" ")))).First();
                            
                            // strip this block by getting indices
                            int start = int.Parse(target.Substring(0, target.IndexOf(" ")));
                            string temp = target.Substring(target.IndexOf(" - ") + 3);
                            int end = int.Parse(temp.Substring(0, temp.IndexOf(" ")));

                            Remove(start, end);

                        }
                        int now = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                        // insert lives at i
                        insert = i.ToString() + " - " + (i + blockCount).ToString() + " "
                                 + name + " " + bytesWasted.ToString() + " " + blockSize.ToString() + now.ToString();
                        // count of uses      ^
                        //mark it used
                        for (int j = i; j < i + blockCount; j += 1)
                        {
                            _Table[i] = true;
                        }

                        _InternalFragReport.Add(insert);
                    }
                    else if (_ReplaceMethod == 1)
                    {
                        if (i >= _PageCount - blockCount)
                        {
                        // find last used by time
                        }
                        // insert lives at i
                        int now = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                        insert = i.ToString() + " - " + (i + blockCount).ToString() + " "
                                 + name + " " + bytesWasted.ToString() + " " + blockSize.ToString() + now.ToString();
                        // count of uses      ^
                        //mark it used
                        for (int j = i; j < i + blockCount; j += 1)
                        {
                            _Table[i] = true;
                        }

                        _InternalFragReport.Add(insert);
                    }
                    else if (_ReplaceMethod == 2)
                    {
                        if (i >= _PageCount - blockCount)
                        {
                        // find least used by count
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
                    else if (_ReplaceMethod == 3)
                    {
                        if (i >= _PageCount - blockCount)
                        {
                        // find last used by optomal
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
                    else if (_ReplaceMethod == 4)
                    {
                        if (i >= _PageCount - blockCount)
                        {
                        // find last used by second chance
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
                    else if (_ReplaceMethod == 5)
                    {
                        if (i >= _PageCount - blockCount)
                        {
                            // find last used by clock
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
                }
        }

        private bool Next_Blocks_Open(long i, long count)
        {
            // cycle over table
            while(!_Table[i] && i < count)
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
    }
}
