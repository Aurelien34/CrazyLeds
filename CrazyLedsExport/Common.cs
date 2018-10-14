using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrazyLedsExport
{
    public enum CrazyDisplayType
    {
        RGBMatrix = 0,
        BWDisplay = 1
    }

    public class CrazyColor
    {
        public CrazyColor()
        {

        }

        public CrazyColor(byte _R, byte _G, byte _B) : this()
        {
            R = _R;
            G = _G;
            B = _B;
        }

        public CrazyColor(Color color) : this(color.R, color.G, color.B)
        {
        }

        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }

        public Color ToColor()
        {
            return Color.FromArgb(R, G, B);
        }

        public override string ToString()
        {
            return String.Format("{0},{1},{2}", R, G, B);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(CrazyColor))
            {
                return false;
            }
            else
            {
                return R == ((CrazyColor)obj).R
                    && G == ((CrazyColor)obj).G
                    && B == ((CrazyColor)obj).B;
            }
        }

        public override int GetHashCode()
        {
            return ((int)R) + ((int)G) >> 8 + ((int)B) >> 16;
        }

        public static CrazyColor Black { get { return new CrazyColor(0, 0, 0); } }
    }
}
