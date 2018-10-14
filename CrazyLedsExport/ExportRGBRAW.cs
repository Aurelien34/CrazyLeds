using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrazyLedsExport
{
    public class ExportRGBRAW : ExportRGBBase
    {
        public override bool Export(CrazyColor[] ledMatrix, int width, int height, CrazyColor backColor, string imageName, out string outputHeaderFileContent, out int approximateSize)
        {
            approximateSize = ledMatrix.Count<CrazyColor>() * 3;
            StringBuilder builder = new StringBuilder(approximateSize * 10);

            builder.AppendFormat(
@"#include <avr/pgmspace.h>
#include ""../CrazyLedImageRAW.h""

const PROGMEM unsigned char LedImageData_{0} [] =
{{
    ", imageName);
            int i = 0;
            foreach(CrazyColor pixel in ledMatrix)
            {
                if (i++ != 0)
                {
                    builder.Append(",\n\t");
                }
                builder.AppendFormat("0x{0:X2}, 0x{1:X2}, 0x{2:X2}", pixel.R, pixel.G, pixel.B);
            }

            builder.AppendFormat(
@"
}};

", imageName);
            builder.AppendFormat(
@"CrazyLedImage * LoadCrazyLedImage_{1}() {{ return new CrazyLedImageRAW({2},{3}, CRGB(0x{4:X2}, 0x{5:X2}, 0x{6:X2}), LedImageData_{1}, {0}); }}
",
            ledMatrix.Length * 3, imageName, width, height, backColor.R, backColor.G, backColor.B);
            outputHeaderFileContent = builder.ToString();
            return true;
        }
    }
}