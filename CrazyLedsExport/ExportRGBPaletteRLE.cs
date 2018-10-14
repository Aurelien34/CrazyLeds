using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrazyLedsExport
{
    public class ExportRGBPaletteRLE : ExportRGBBase
    {
        public override bool Export(CrazyColor[] ledMatrix, int width, int height, CrazyColor backColor, string imageName, out string outputHeaderFileContent, out int approximateSize)
        {
            // Try to build the palette
            Dictionary<CrazyColor, int> paletteTable = new Dictionary<CrazyColor, int>();
            foreach (CrazyColor color in ledMatrix)
            {
                if (!paletteTable.ContainsKey(color))
                {
                    paletteTable.Add(color, 1);
                }
            }

            // Do we have less than 16 colors?
            if (paletteTable.Count <= 16)
            {
                // Palette size is correct
                // Build the palette to be exported
                List<CrazyColor> palette = paletteTable.Keys.ToList();

                // Build bitmap data based on palette data
                List<byte> bitmapData = new List<byte>(ledMatrix.Length);
                foreach (CrazyColor color in ledMatrix)
                {
                    bitmapData.Add((byte)palette.IndexOf(color));
                }

                // Build RLE compressed data
                List<byte> output = new List<byte>(bitmapData.Count);
                for (int i = 0; i < bitmapData.Count; ++i)
                {
                    // List contiguous colors counts
                    byte referenceColor = bitmapData[i];
                    int j;
                    for (j = i; j - i < 16 && j < bitmapData.Count; ++j)
                    {
                        if (bitmapData[j] != referenceColor)
                            break;
                    }

                    // Encode the output
                    output.Add((byte)(referenceColor | (byte)(((j - i - 1) << 4))));

                    // Move to the next pixel
                    i = j - 1;
                }

                // Build the export class
                approximateSize = output.Count + palette.Count * 3;
                StringBuilder builder = new StringBuilder(approximateSize * 10);

                // Start with the palette
                builder.AppendFormat(
@"#include <avr/pgmspace.h>
#include ""../CrazyLedImagePaletteRLE.h""

const PROGMEM unsigned char LedImagePaletteData_{0} [] =
{{
    ", imageName);
                int counter = 0;
                foreach (CrazyColor color in palette)
                {
                    if (counter != 0)
                    {
                        builder.Append(",\n\t");
                    }
                    builder.AppendFormat("0x{0:X2}, 0x{1:X2}, 0x{2:X2}", color.R, color.G, color.B);
                    ++counter;
                }

                builder.Append("\n};\n\n");

                // Then add image information
                builder.AppendFormat(
@"const PROGMEM unsigned char LedImageData_{0} [] =
{{", imageName);

                counter = 0;
                foreach (byte b in output)
                {
                    if (counter != 0)
                    {
                        builder.Append(", ");
                    }
                    if (counter++ % 10 == 0)
                    {
                        builder.Append("\n\t");
                    }
                    builder.AppendFormat("0x{0:X2}", b);
                }

                builder.Append(@"
};

");
                builder.AppendFormat(
    @"CrazyLedImage * LoadCrazyLedImage_{0}() {{ return new CrazyLedImagePaletteRLE({1},{2}, CRGB(0x{4:X2}, 0x{5:X2}, 0x{6:X2}), LedImageData_{0}, {3}, LedImagePaletteData_{0}); }}",
                imageName, width, height, output.Count, backColor.R, backColor.G, backColor.B);

                outputHeaderFileContent = builder.ToString();

                return true;

            }
            else
            {
                // Too many colors to use this export mode
                outputHeaderFileContent = null;
                approximateSize = int.MaxValue;
                return false;
            }
        }
    }
}