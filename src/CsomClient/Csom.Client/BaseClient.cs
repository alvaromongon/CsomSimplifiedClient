using System;
using Microsoft.ProjectServer.Client;
using Csom.Client.Common;
using Csom.Client.Context;

namespace Csom.Client
{
    public abstract class BaseClient
    {
        private readonly CsomClientOptions _options;
        protected readonly ProjectContext _projectContext;        

        public BaseClient(ProjectContext projectContext, CsomClientOptions options)
        {
            GuardParameters(projectContext, options);

            _projectContext = projectContext;
            _options = options;
        }

        protected void WaitForQueuedJobToComplete(QueueJob queuedJob, string actionName)
        {
            //var jobState = _projectContext.WaitForQueue(queuedJob, _options.WaitingTimeInSeconds);
            var jobState = _projectContext.WaitWithRetries(queuedJob, _options.MaxRetries, _options.WaitingTimeInSeconds);

            if (jobState != JobState.Success)
            {
                // TODO: LOG ERROR!

                throw new CsomClientException($"Timeout {actionName}. " +
                    $"waiting time in seconds is {_options.WaitingTimeInSeconds} seconds. " +
                    $"Project context url is {_projectContext.Url}.");
            }
        }

        private static void GuardParameters(ProjectContext projectContext, CsomClientOptions projectServiceOptions)
        {
            if (projectContext == null)
            {
                throw new ArgumentNullException(nameof(projectContext));
            }

            if (projectServiceOptions == null)
            {
                throw new ArgumentNullException(nameof(projectServiceOptions));
            }

            if (projectServiceOptions.WaitingTimeInSeconds < 5)
            {
                throw new ArgumentOutOfRangeException(nameof(projectServiceOptions.WaitingTimeInSeconds));
            }
        }
    }
}
