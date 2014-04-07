using System;
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


        public Memory(double size)
        {
            if (size != .5 || size != 1 || size != 4)
            {
                _BlockSize = 1;
            }
            else
            {
                _BlockSize = size;
            }
            _Start = _End = _UsedSpace = _Entries = _PageCount = 0;
        }

        public Memory(double size, double start, double end, int pages, int entries)
        {
            if (size != .5 || size != 1 || size != 4)
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
        }

        public double InternalFragmentationPercent()
        {
            return 100 * (_UsedSpace / (_End - Start));
        }

    }
}
