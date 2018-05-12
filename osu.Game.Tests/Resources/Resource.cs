// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
