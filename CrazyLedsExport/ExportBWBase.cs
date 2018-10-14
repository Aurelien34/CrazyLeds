using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrazyLedsExport
{
    public abstract class ExportBWBase
    {
        public abstract bool Export(CrazyColor[] ledMatrix, int width, int height, CrazyColor backColor, string imageName, out string outputHeaderFileContent, out int approximateSize);
    }
}
