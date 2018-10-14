using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrazyLedsExport
{
    public class ExportBWRaw : ExportBWBase
    {
        public override bool Export(CrazyColor[] ledMatrix, int width, int height, CrazyColor backColor, string imageName, out string outputHeaderFileContent, out int approximateSize)
        {
            approximateSize = (width * height) / 8; // One bit per pixel

            StringBuilder builder = new StringBuilder(1024);

            builder.AppendFormat(
@"#include <avr/pgmspace.h>
#include ""../CrazyLedBWImageRAW.h""

#ifdef DEFINE_IMAGE_RESOURCES
const PROGMEM unsigned char LedImageData_{0} [] =
{{
    ", imageName);
            for (int j = 0; j < height / 8; ++j)
            {
                for (int i = 0; i < width; ++i)
                {
                    byte accumulator = 0;
                    for (int bitIndex = 0; bitIndex < 8; ++bitIndex)
                    {
                        if (!ledMatrix[i + ((j * 8) + bitIndex) * width].Equals(CrazyColor.Black))
                        {
                            accumulator |= (byte)(1 << bitIndex);
                        }
                    }

                    if (i != 0 || j != 0)
                    {
                        builder.Append(", ");
                    }

                    if ((i + j * width) % 16 == 0)
                    {
                        builder.Append("\n\t");
                    }

                    builder.AppendFormat("0x{0:X2}", accumulator);
                }
            }

            builder.AppendFormat(
@"
}};

", imageName);
            builder.AppendFormat(
@"CrazyLedBWImage * LoadCrazyLedBWImage_{0}() {{ return new CrazyLedBWImageRAW({1}, {2}, LedImageData_{0}); }}

#else

CrazyLedBWImage * LoadCrazyLedBWImage_{0}();

#endif

",
            imageName, width, height);
            outputHeaderFileContent = builder.ToString();
            return true;
        }
    }
}
