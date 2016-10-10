using System;
using System.IO;
using System.Reflection;

namespace osu.Game.Tests.Resources
{
    public static class Resource
    {
        public static Stream OpenResource(string name)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(
                $@"osu.Game.Tests.Resources.{name}");
        }
    }
}