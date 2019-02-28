// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using NUnit.Framework;
using osu.Framework.IO.Stores;

namespace osu.Game.Tests.Resources
{
    public static class TestResources
    {
        public static Stream OpenResource(string name) => new DllResourceStore("osu.Game.Tests.dll").GetStream($"Resources/{name}");

        public static Stream GetTestBeatmapStream() => new DllResourceStore("osu.Game.Resources.dll").GetStream("Beatmaps/241526 Soleily - Renatus.osz");

        public static string GetTestBeatmapForImport()
        {
            string temp = Path.GetTempFileName() + ".osz";

            using (Stream stream = GetTestBeatmapStream())
            using (FileStream newFile = File.Create(temp))
                stream.CopyTo(newFile);

            Assert.IsTrue(File.Exists(temp));
            return temp;
        }
    }
}
