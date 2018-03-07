namespace Csom.Client.Common
{
    public class CsomClientOptions
    {
        /// <summary>
        /// Max number of retries after wating for the waiting time in seconds
        /// </summary>
        public int MaxRetries { get; set; }

        /// <summary>
        /// Waiting time in seconds for a queue operation to complete
        /// NOTE: Values under 5 seems not to work properly
        /// </summary>
        public int WaitingTimeInSeconds { get; set; }
    }
}
