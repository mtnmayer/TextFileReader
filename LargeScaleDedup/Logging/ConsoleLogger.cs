using System;
using LargeScaleDedup.Logging;

namespace LargeScaleDedup.Logging
{
    public class ConsoleLogger : ILogger
    {
        public void Log(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[INFO] {DateTime.Now}: {message}");
            Console.ResetColor();
        }

        public void LogError(string errorMessage)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ERROR] {DateTime.Now}: {errorMessage}");
            Console.ResetColor();
        }
    }
}