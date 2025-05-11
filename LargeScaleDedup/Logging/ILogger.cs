namespace LargeScaleDedup.Logging
{
    public interface ILogger
    {
        void Log(string message);
        void LogError(string errorMessage);
    }
}