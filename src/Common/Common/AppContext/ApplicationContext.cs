using Common.Helpers;

namespace Common.AppContext
{
    public class ApplicationContext : IApplicationContext
    {
        private string _executionPath;

        /// <summary>
        /// Gets the execution path.
        /// </summary>
        /// <value>
        /// The execution path.
        /// </value>
        public string ExecutionPath
        {
            get
            {
                if (string.IsNullOrEmpty(_executionPath))
                {
                    _executionPath = PathHelper.ExecutingAssemblyDirectory;
                }

                return _executionPath;
            }
        }
    }

}
