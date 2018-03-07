using System;
using System.Net;
using Microsoft.ProjectServer.Client;
using Microsoft.SharePoint.Client;
using Csom.Client.Common;

namespace Csom.Client.Context
{
    public static class ProjectContextFactory
    {
        /// <summary>
        /// Build an instance of a csom ProjectContext
        /// </summary>
        /// <param name="options">options needed to build the project context</param>
        /// <returns></returns>
        public static ProjectContext Build(ProjectContextOptions options)
        {
            GuardParameters(options);

            ProjectContext context = null;

            try
            {
                context = new ProjectContext(options.PwaWebSiteUrl);
                var networkCredentials = new NetworkCredential(options.PwaUser, options.PwaPassword);
                var orgIDCredential = new SharePointOnlineCredentials(networkCredentials.UserName, networkCredentials.SecurePassword);
                context.Credentials = orgIDCredential;                
            }
            catch(Exception ex)
            {
                // TODO: LOG ERROR!

                throw new CsomClientException($"Unexcepted error building project context. " +
                    $"Project context pwa webSite url is {options.PwaWebSiteUrl}. " +
                    $"User tried with is {options.PwaUser}", ex);
            }

            return context;
        }

        private static void GuardParameters(ProjectContextOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (string.IsNullOrEmpty((options.PwaWebSiteUrl.Trim())))
            {
                throw new ArgumentOutOfRangeException(nameof(options.PwaWebSiteUrl));
            }

            if (string.IsNullOrEmpty((options.PwaUser.Trim())))
            {
                throw new ArgumentOutOfRangeException(nameof(options.PwaUser));
            }

            if (string.IsNullOrEmpty((options.PwaWebSiteUrl.Trim())))
            {
                throw new ArgumentOutOfRangeException(nameof(options.PwaWebSiteUrl));
            }
        }
    }
}
