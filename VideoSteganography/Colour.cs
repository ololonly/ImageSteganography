using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoSteganography
{
    public class Colour : IComparable
    {
        public int Intesivity { get; set; }
        public int Index { get; set; }

        public Colour(int index, int intesivity)
        {
            this.Intesivity = intesivity;
            this.Index = index;
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;
            var otherColour = obj as Colour;
            if (otherColour != null) return this.Intesivity.CompareTo(otherColour.Intesivity);
            else throw new ArgumentException("Object is not comparable type");
        }
    }
}
