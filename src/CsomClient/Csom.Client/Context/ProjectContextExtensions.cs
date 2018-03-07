using Microsoft.ProjectServer.Client;

namespace Csom.Client.Context
{
    internal static class ProjectContextExtensions
    {
        public static JobState WaitWithRetries(this ProjectContext projectContext, QueueJob queuedJob, int maxRetries, int waitingTimeInSeconds)
        {
            JobState result = JobState.Unknown;
            int i = 0;

            while(i < maxRetries)
            {
                var jobState = projectContext.WaitForQueue(queuedJob, waitingTimeInSeconds);

                if (jobState == JobState.Success)
                {
                    result = jobState;
                    break;
                }
                if (jobState == JobState.Canceled || jobState == JobState.Failed || jobState == JobState.FailedNotBlocking)
                {
                    result = jobState;
                    break;
                }

                i++;
            }

            return result;
        }
    }
}
