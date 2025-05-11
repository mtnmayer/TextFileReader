using System;
using LargeScaleDedup.Configuration;
using LargeScaleDedup.Core;
using LargeScaleDedup.Logging;

namespace LargeScaleDedup
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    DisplayUsage();
                    return;
                }

                string inputFile = args[0];

                string outputFile = GenerateOutputFileName(inputFile);
                long maxMemoryBytes = 8_000_000_000;

                Console.WriteLine($"Starting deduplication of: {inputFile}");
                Console.WriteLine($"Output will be saved to: {outputFile}");
                Console.WriteLine();

                var config = new Config(inputFile, outputFile, maxMemoryBytes);

                var deduplicator = new LargeFileDeduplicator(config);
                deduplicator.Deduplicate();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();
                Environment.Exit(1);
            }

            static void DisplayUsage()
            {
                Console.WriteLine("File Deduplication Tool");
                Console.WriteLine("======================");
                Console.WriteLine("Usage: FileDeduplication <inputFilePath>");
                Console.WriteLine();
                Console.WriteLine("The program will automatically:");
                Console.WriteLine(" - Create an output file with '_deduplicated' suffix");
                Console.WriteLine();
                Console.WriteLine("Example: FileDeduplication C:\\data\\large_file.txt");
                Console.WriteLine("         (creates C:\\data\\large_file_deduplicated.txt)");
            }

            static string GenerateOutputFileName(string inputFile)
            {
                string directory = Path.GetDirectoryName(inputFile);
                string fileName = Path.GetFileNameWithoutExtension(inputFile);
                string extension = Path.GetExtension(inputFile);

                string outputFileName = $"{fileName}_deduplicated{extension}";

                if (!string.IsNullOrEmpty(directory))
                {
                    return Path.Combine(directory, outputFileName);
                }

                return outputFileName;
            }           
        }
    }
}