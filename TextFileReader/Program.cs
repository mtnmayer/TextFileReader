using System;
using System.IO;
using System.Text;
using System.Linq;

namespace TextFileReader
{
    class Program
    {
        private static string ReverseHebrewText(string text)
        {
            return new string(text.Reverse().ToArray());
        }

        private static bool ContainsHebrew(string text)
        {
            return text.Any(c => c >= 0x0590 && c <= 0x05FF);
        }

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            Console.WriteLine("Text File Line-by-Line Reader");
            Console.WriteLine("----------------------------");

            Console.Write("Enter the input file path: ");
            string inputFilePath = Console.ReadLine();

            Console.Write("Enter the output file path: ");
            string outputFilePath = Console.ReadLine();

            try
            {
                if (!File.Exists(inputFilePath))
                {
                    Console.WriteLine("Error: Input file not found!");
                    return;
                }

                Console.WriteLine("\nProcessing file...");
                Console.WriteLine("------------------");

                using (StreamReader reader = new StreamReader(inputFilePath, Encoding.UTF8))
                using (StreamWriter writer = new StreamWriter(outputFilePath, false, Encoding.UTF8)) 
                {
                    string line;
                    int lineNumber = 1;

                    while ((line = reader.ReadLine()) != null)
                    {
                        string displayLine = line;

                        if (ContainsHebrew(line))
                        {
                            displayLine = ReverseHebrewText(line);
                        }

                        // Write to console
                        Console.WriteLine($"{lineNumber}: {displayLine}");

                        // Write to output file
                        writer.WriteLine(displayLine);

                        lineNumber++;
                    }
                }

                Console.WriteLine("\nFile processing completed.");
                Console.WriteLine($"Output written to: {outputFilePath}");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"File error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error: {ex.Message}");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
