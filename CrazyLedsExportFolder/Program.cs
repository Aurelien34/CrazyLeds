using CrazyLeds;
using CrazyLedsExport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CrazyLedsExportFolder
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 2)
            {
                string sourcePath = args[0];
                string targetPath = args[1];
                if (Directory.Exists(sourcePath))
                {
                    if (Directory.Exists(targetPath))
                    {
                        // Parameters are OK
                        // List input files
                        foreach (string sourceFileName in Directory.GetFiles(sourcePath, "*.led"))
                        {
                            // Compute target file name
                            string targetFileName = Path.Combine(targetPath, Path.GetFileNameWithoutExtension(sourceFileName) + ".h");

                            // Compare file dates and generate output file only if the output file is not up to date
                            if (!File.Exists(targetFileName) || File.GetLastWriteTime(sourceFileName) > File.GetLastWriteTime(targetFileName))
                            {
                                // Deserialize led file
                                XmlSerializer serializer = new XmlSerializer(typeof(Global));
                                Global data = null;
                                using (Stream stream = File.OpenRead(sourceFileName))
                                {
                                    data = (Global)serializer.Deserialize(stream);
                                }

                                String exportedData;
                                // Determine the type of display
                                if (data.DisplayType == CrazyDisplayType.RGBMatrix)
                                {
                                    // Generate header file
                                    exportedData = ExportManager.ExportLedImageToHeaderFile(
                                        data.LedMatrix.ToArray(),
                                        data.Width,
                                        data.Height,
                                        data.BackGroundColor,
                                        Path.GetFileNameWithoutExtension(sourceFileName));
                                }
                                else
                                {
                                    // Generate header file
                                    exportedData = ExportManager.ExportBWImageToHeaderFile(
                                        data.LedMatrix.ToArray(),
                                        data.Width,
                                        data.Height,
                                        data.BackGroundColor,
                                        Path.GetFileNameWithoutExtension(sourceFileName));
                                }
                                // Store header file
                                File.WriteAllText(targetFileName, exportedData);
                            }
                        }
                    }
                    else
                    {
                        Console.Error.WriteLine("Target path " + targetPath + " does not exist");
                    }
                }
                else
                {
                    Console.Error.WriteLine("Source path " + sourcePath + " does not exist");
                }
            }
            else
            {
                Console.Error.WriteLine("Expected source and target folders as parameters");
            }
        }
    }
}
