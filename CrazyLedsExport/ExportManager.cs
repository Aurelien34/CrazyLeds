using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CrazyLedsExport
{
    public static class ExportManager
    {
        public static string ExportLedImageToHeaderFile(CrazyColor[] ledMatrix, int width, int height, CrazyColor backColor, string imageName)
        {
            String outputHeaderFileContent = null;
            int approximateROMSize = int.MaxValue;

            foreach (Type exportEngineType in Assembly.GetExecutingAssembly().GetTypes().Where(t => t.BaseType == typeof(ExportRGBBase)))
            {
                ExportRGBBase exportEngineInstance = (ExportRGBBase)Activator.CreateInstance(exportEngineType);
                string generatedHeader;
                int generatedROMSize;
                if (exportEngineInstance.Export(ledMatrix, width, height, backColor, imageName, out generatedHeader, out generatedROMSize)
                    && generatedROMSize < approximateROMSize)
                {
                    outputHeaderFileContent = generatedHeader;
                    approximateROMSize = generatedROMSize;
                }
            }
            return outputHeaderFileContent;
        }

        public static string ExportBWImageToHeaderFile(CrazyColor[] ledMatrix, int width, int height, CrazyColor backColor, string imageName)
        {
            String outputHeaderFileContent = null;
            int approximateROMSize = int.MaxValue;

            foreach (Type exportEngineType in Assembly.GetExecutingAssembly().GetTypes().Where(t => t.BaseType == typeof(ExportBWBase)))
            {
                ExportBWBase exportEngineInstance = (ExportBWBase)Activator.CreateInstance(exportEngineType);
                string generatedHeader;
                int generatedROMSize;
                if (exportEngineInstance.Export(ledMatrix, width, height, backColor, imageName, out generatedHeader, out generatedROMSize)
                    && generatedROMSize < approximateROMSize)
                {
                    outputHeaderFileContent = generatedHeader;
                    approximateROMSize = generatedROMSize;
                }
            }
            return outputHeaderFileContent;
        }
    }
}
