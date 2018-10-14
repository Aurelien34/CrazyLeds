using CrazyLedsExport;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrazyLeds
{
    public class Global
    {
        public Global()
        {
            CustomColorsList = new List<CrazyColor>();
        }

        public CrazyDisplayType DisplayType { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }

        public CrazyColor ForeGroundColor { get; set; }
        public CrazyColor BackGroundColor { get; set; }

        public List<CrazyColor> CustomColorsList { get; set; }

        public List<CrazyColor> LedMatrix { get; set; }
    }
}
