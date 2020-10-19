// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using NUnit.Framework;
using osu.Framework.IO.Stores;

namespace osu.Game.Tests.Resources
{
    public static class TestResources
    {
        public static DllResourceStore GetStore() => new DllResourceStore(typeof(TestResources).Assembly);

        public static Stream OpenResource(string name) => GetStore().GetStream($"Resources/{name}");

        public static Stream GetTestBeatmapStream(bool virtualTrack = false) => OpenResource($"Archives/241526 Soleily - Renatus{(virtualTrack ? "_virtual" : "")}.osz");

        public static string GetTestBeatmapForImport(bool virtualTrack = false)
        {
            var tempPath = Path.GetTempFileName() + ".osz";

            using (var stream = GetTestBeatmapStream(virtualTrack))
            using (var newFile = File.Create(tempPath))
                stream.CopyTo(newFile);

            Assert.IsTrue(File.Exists(tempPath));
            return tempPath;
        }
    }
}
