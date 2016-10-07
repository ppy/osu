using System;
using System.IO;
using System.Reflection;

namespace osu.Game.Tests.Resources
{
    public static class Resource
    {
        public static string GetPath(string path)
        {
            var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return Path.Combine(assemblyDir, "Resources", path);
        }
    }
}