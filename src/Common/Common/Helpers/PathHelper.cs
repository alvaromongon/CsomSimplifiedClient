using System;
using System.IO;
using System.Reflection;

namespace Common.Helpers
{
    public static class PathHelper
    {
        public static string ExecutingAssemblyDirectory
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly();
                return AssemblyDirectory(assembly);
            }
        }

        public static string AssemblyDirectory(Assembly assembly)
        {
            string codeBase = assembly.CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }
    }
}
