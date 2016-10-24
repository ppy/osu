using System;
using System.IO;
using System.Reflection;

namespace osu.Game.Tests.Resources
{
    public static class Resource
    {
        public static Stream OpenResource(string name)
        {
            var localPath = Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(Assembly.GetExecutingAssembly().CodeBase).Path));

            return Assembly.GetExecutingAssembly().GetManifestResourceStream($@"osu.Game.Tests.Resources.{name}") ??
                Assembly.LoadFrom(Path.Combine(localPath, @"osu.Game.Resources.dll")).GetManifestResourceStream($@"osu.Game.Resources.{name}");
        }
    }
}