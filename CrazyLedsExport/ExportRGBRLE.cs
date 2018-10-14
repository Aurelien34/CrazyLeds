using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrazyLedsExport
{
    public class ExportRGBRLE : ExportRGBBase
    {
        public override bool Export(CrazyColor[] ledMatrix, int width, int height, CrazyColor backColor, string imageName, out string outputHeaderFileContent, out int approximateSize)
        {
            List<KeyValuePair<CrazyColor, int>> colorOccurrences = new List<KeyValuePair<CrazyColor, int>>();
            CrazyColor currentColor = null;
            foreach (CrazyColor pixelColor in ledMatrix)
            {
                if (currentColor == null || !pixelColor.Equals(currentColor))
                {
                    currentColor = pixelColor;
                    colorOccurrences.Add(new KeyValuePair<CrazyColor, int>(currentColor, 1));
                }
                else
                {
                    colorOccurrences[colorOccurrences.Count - 1] =
                        new KeyValuePair<CrazyColor, int>(
                            currentColor,
                            colorOccurrences[colorOccurrences.Count - 1].Value + 1);
                }
            }

            List<byte> output = new List<byte>();
            for (int i = 0; i < colorOccurrences.Count; i++)
            {
                if (colorOccurrences[i].Value == 1)
                {
                    int j;
                    for (j = i + 1; j < colorOccurrences.Count && colorOccurrences[j].Value == 1 && j-i <= 127; j++)
                    {
                    }
                    output.Add((byte)(j - i));
                    for (int k = i; k < j; k++)
                    {
                        output.Add(colorOccurrences[k].Key.R);
                        output.Add(colorOccurrences[k].Key.G);
                        output.Add(colorOccurrences[k].Key.B);
                    }
                    i = j - 1;
                }
                else
                {
                    int j = colorOccurrences[i].Value;
                    while (j > 0)
                    {
                        byte smallj = (byte)(j % 128);
                        output.Add((byte)(smallj + 128));
                        output.Add(colorOccurrences[i].Key.R);
                        output.Add(colorOccurrences[i].Key.G);
                        output.Add(colorOccurrences[i].Key.B);
                        j -= smallj;
                    }
                }
                
            }

            approximateSize = output.Count();

            StringBuilder builder = new StringBuilder(approximateSize * 10);

            builder.AppendFormat(
@"#include <avr/pgmspace.h>
#include ""../CrazyLedImageRLE.h""

const PROGMEM unsigned char LedImageData_{0} [] =
{{
    ", imageName);
            int counter = 0;
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

            builder.AppendFormat(
@"
}};

", imageName);
            builder.AppendFormat(
@"CrazyLedImage * LoadCrazyLedImage_{0}() {{ return new CrazyLedImageRLE({1},{2}, CRGB(0x{4:X2}, 0x{5:X2}, 0x{6:X2}), LedImageData_{0}, {3}); }}
",
            imageName, width, height, output.Count, backColor.R, backColor.G, backColor.B);

            outputHeaderFileContent = builder.ToString();
            return true;
        }
    }
}