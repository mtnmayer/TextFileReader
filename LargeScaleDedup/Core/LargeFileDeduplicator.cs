using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LargeScaleDedup.Configuration;
using LargeScaleDedup.Exceptions;
using LargeScaleDedup.Logging;

namespace LargeScaleDedup.Core
{
    public class LargeFileDeduplicator
    {
        private readonly Config _config;
        private readonly ILogger _logger;

        public LargeFileDeduplicator(Config config, ILogger logger = null)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? new ConsoleLogger();

            Directory.CreateDirectory(_config.TempDirectory);
        }

        public void Deduplicate()
        {
            try
            {
                _logger.Log($"Starting deduplication of {_config.InputFilePath}");

                List<string> chunkFiles = CreateDeduplicatedChunks();
                _logger.Log($"Created {chunkFiles.Count} deduplicated chunks");

                if (chunkFiles.Count == 1)
                {
                    File.Copy(chunkFiles[0], _config.OutputFilePath, true);
                    _logger.Log("Only one chunk needed - direct copy complete");
                }
                else
                {
                    MergeDeduplicatedChunks(chunkFiles);
                    _logger.Log("Merged all chunks successfully");
                }

                if (_config.DeleteTempFiles)
                {
                    CleanupTempFiles(chunkFiles);
                }

                _logger.Log($"Deduplication complete. Output saved to {_config.OutputFilePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Deduplication failed: {ex.Message}");
                throw;
            }
        }

        private List<string> CreateDeduplicatedChunks()
        {
            List<string> chunkFiles = new List<string>();
            int chunkNumber = 0;

            try
            {
                using (StreamReader reader = new StreamReader(_config.InputFilePath))
                {
                    HashSet<string> uniqueLines = new HashSet<string>(StringComparer.Ordinal);
                    long currentMemoryUsage = 0;
                    string line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (uniqueLines.Add(line))
                        {
                            currentMemoryUsage += Encoding.UTF8.GetByteCount(line);                                              
                        }

                        if (currentMemoryUsage >= _config.MaxMemoryBytes)
                        {
                            string chunkFile = WriteChunkToDisk(uniqueLines, chunkNumber++);
                            chunkFiles.Add(chunkFile);

                            uniqueLines.Clear();
                            currentMemoryUsage = 0;

                            _logger.Log($"Wrote chunk {chunkNumber - 1} with {uniqueLines.Count} unique lines");
                        }
                    }

                    // Write any remaining lines
                    if (uniqueLines.Count > 0)
                    {
                        string chunkFile = WriteChunkToDisk(uniqueLines, chunkNumber);
                        chunkFiles.Add(chunkFile);
                        _logger.Log($"Wrote final chunk {chunkNumber} with {uniqueLines.Count} unique lines");
                    }
                }

                return chunkFiles;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating chunks: {ex.Message}");
                throw new DeduplicationException("Failed to create deduplicated chunks", ex);
            }
        }

        private string WriteChunkToDisk(HashSet<string> uniqueLines, int chunkNumber)
        {
            string chunkFilePath = Path.Combine(_config.TempDirectory, $"chunk_{chunkNumber}.txt");

            try
            {
                using (StreamWriter writer = new StreamWriter(chunkFilePath))
                {
                    foreach (string line in uniqueLines)
                    {
                        writer.WriteLine(line);
                    }
                }

                return chunkFilePath;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error writing chunk {chunkNumber}: {ex.Message}");
                throw new DeduplicationException($"Failed to write chunk file {chunkFilePath}", ex);
            }
        }

        private void MergeDeduplicatedChunks(List<string> chunkFiles)
        {
            try
            {
                File.Copy(chunkFiles[0], _config.OutputFilePath, true);
                _logger.Log($"Started merge with chunk 0 as base");

                for (int i = 1; i < chunkFiles.Count; i++)
                {
                    string tempResultFile = Path.Combine(_config.TempDirectory, "temp_result.txt");

                    HashSet<string> currentChunkLines = new HashSet<string>(File.ReadLines(chunkFiles[i]),StringComparer.Ordinal);

                    _logger.Log($"Merging chunk {i} with {currentChunkLines.Count} lines");

                    using (StreamReader resultReader = new StreamReader(_config.OutputFilePath))
                    using (StreamWriter tempWriter = new StreamWriter(tempResultFile))
                    {
                        string line;
                        int existingLinesCount = 0;

                        while ((line = resultReader.ReadLine()) != null)
                        {
                            tempWriter.WriteLine(line);
                            currentChunkLines.Remove(line);
                            existingLinesCount++;
                        }

                        foreach (string newLine in currentChunkLines)
                        {
                            tempWriter.WriteLine(newLine);
                        }

                        _logger.Log($"Kept {existingLinesCount} existing lines, added {currentChunkLines.Count} new unique lines");
                    }

                    File.Delete(_config.OutputFilePath);
                    File.Move(tempResultFile, _config.OutputFilePath);

                    _logger.Log($"Completed merge of chunk {i}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error merging chunks: {ex.Message}");
                throw new DeduplicationException("Failed to merge deduplicated chunks", ex);
            }
        }

        private void CleanupTempFiles(List<string> chunkFiles)
        {
            try
            {
                foreach (string chunkFile in chunkFiles)
                {
                    if (File.Exists(chunkFile))
                    {
                        File.Delete(chunkFile);
                    }
                }

                string tempResultFile = Path.Combine(_config.TempDirectory, "temp_result.txt");
                if (File.Exists(tempResultFile))
                {
                    File.Delete(tempResultFile);
                }

                if (Directory.Exists(_config.TempDirectory))
                {
                    Directory.Delete(_config.TempDirectory, true);
                }

                _logger.Log("Cleaned up all temporary files");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Warning: Failed to clean up some temporary files: {ex.Message}");
            }
        }
    }
}