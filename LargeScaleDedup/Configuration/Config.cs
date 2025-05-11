using System;
using System.IO;

namespace LargeScaleDedup.Configuration
{
    public class Config
    {
        public string InputFilePath { get; set; }
        public string OutputFilePath { get; set; }
        public long MaxMemoryBytes { get; set; }
        public string TempDirectory { get; set; }
        public bool DeleteTempFiles { get; set; } = true;

        public Config(string inputPath, string outputPath, long maxMemoryBytes)
        {
            InputFilePath = inputPath ?? throw new ArgumentNullException(nameof(inputPath));
            OutputFilePath = outputPath ?? throw new ArgumentNullException(nameof(outputPath));
            MaxMemoryBytes = maxMemoryBytes > 0 ? maxMemoryBytes : throw new ArgumentException("Memory limit must be positive", nameof(maxMemoryBytes));
            TempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        }
    }
}