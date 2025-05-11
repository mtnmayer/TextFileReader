using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

class ExternalDeduplication
{
    static void Main(string[] args)
    {
        string inputFile = @"C:\Users\mtnma\Downloads\input_50GB\input_50GB.txt";
        string outputFile = "output.txt";
        long maxMemoryBytes = 8_000_000_000;

        DeduplicateLargeFile(inputFile, outputFile, maxMemoryBytes);
    }

    static void DeduplicateLargeFile(string inputFile, string outputFile, long maxMemoryBytes)
    {
        List<string> chunkFiles = CreateDeduplicatedChunks(inputFile, maxMemoryBytes);

        if (chunkFiles.Count == 1)
        {
            File.Copy(chunkFiles[0], outputFile, true);
        }
        else
        {
            MergeDeduplicatedChunks(chunkFiles, outputFile, maxMemoryBytes);
        }

        foreach (string chunkFile in chunkFiles)
        {
            File.Delete(chunkFile);
        }
    }

    static List<string> CreateDeduplicatedChunks(string inputFile, long maxMemoryBytes)
    {
        List<string> chunkFiles = new List<string>();
        int chunkNumber = 0;
        string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        using (StreamReader reader = new StreamReader(inputFile))
        {
            HashSet<string> uniqueLines = new HashSet<string>(StringComparer.Ordinal);
            long currentSize = 0;
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                if (uniqueLines.Add(line))
                {
                    currentSize += Encoding.UTF8.GetByteCount(line);
                }
      
                if (currentSize >= maxMemoryBytes)
                {
                    string chunkFile = WriteChunk(uniqueLines, tempDir, chunkNumber);
                    chunkFiles.Add(chunkFile);
                    chunkNumber++;
                    uniqueLines.Clear();
                    currentSize = 0;
                }
            }

            if (uniqueLines.Count > 0)
            {
                string chunkFile = WriteChunk(uniqueLines, tempDir, chunkNumber);
                chunkFiles.Add(chunkFile);
            }
        }

        return chunkFiles;
    }

    static string WriteChunk(HashSet<string> uniqueLines, string tempDir, int chunkNumber)
    {
        string chunkFile = Path.Combine(tempDir, $"chunk_{chunkNumber}.txt");
        using (StreamWriter writer = new StreamWriter(chunkFile))
        {
            foreach (string line in uniqueLines)
            {
                writer.WriteLine(line);
            }
        }

        return chunkFile;
    }

    static void MergeDeduplicatedChunks(List<string> chunkFiles, string outputFile, long maxMemoryBytes)
    {              
            MergeUsingProgressiveApproach(chunkFiles, outputFile);
    }

    static void MergeUsingProgressiveApproach(List<string> chunkFiles, string outputFile)
    {
        File.Copy(chunkFiles[0], outputFile, true);
        for (int i = 1; i < chunkFiles.Count; i++)
        {
            string tempResultFile = Path.Combine(Path.GetTempPath(), "temp_result.txt");
            HashSet<string> currentChunkLines = new HashSet<string>(File.ReadLines(chunkFiles[i]), StringComparer.Ordinal);

            using (StreamReader resultReader = new StreamReader(outputFile))
            using (StreamWriter tempWriter = new StreamWriter(tempResultFile))
            {
                string line;
                while ((line = resultReader.ReadLine()) != null)
                {
                    tempWriter.WriteLine(line);
                    currentChunkLines.Remove(line);
                }

                foreach (string newLine in currentChunkLines)
                {
                    tempWriter.WriteLine(newLine);
                }
            }

            File.Delete(outputFile);
            File.Move(tempResultFile, outputFile);
        }
    }
}