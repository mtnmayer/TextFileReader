using System;

namespace LargeScaleDedup.Exceptions
{
    /// <summary>
    /// Custom exception for deduplication errors
    /// </summary>
    public class DeduplicationException : Exception
    {
        public DeduplicationException(string message) : base(message) { }
        public DeduplicationException(string message, Exception innerException) : base(message, innerException) { }
    }
}