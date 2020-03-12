using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assignment2
{
    class MV
    {
        public sbyte x { get; set; }
        public sbyte y { get; set; }
        public MV() { }
        public MV(sbyte newX, sbyte newY)
        {
            x = newX;
            y = newY;
        }
    }
}
