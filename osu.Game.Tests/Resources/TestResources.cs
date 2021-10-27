// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using NUnit.Framework;
using osu.Framework.IO.Stores;
using osu.Framework.Testing;

namespace osu.Game.Tests.Resources
{
    public static class TestResources
    {
        public const double QUICK_BEATMAP_LENGTH = 10000;

        private static readonly TemporaryNativeStorage temp_storage = new TemporaryNativeStorage("TestResources");

        public static DllResourceStore GetStore() => new DllResourceStore(typeof(TestResources).Assembly);

        public static Stream OpenResource(string name) => GetStore().GetStream($"Resources/{name}");

        public static Stream GetTestBeatmapStream(bool virtualTrack = false) => OpenResource($"Archives/241526 Soleily - Renatus{(virtualTrack ? "_virtual" : "")}.osz");

        /// <summary>
        /// Retrieve a path to a copy of a shortened (~10 second) beatmap archive with a virtual track.
        /// </summary>
        /// <remarks>
        /// This is intended for use in tests which need to run to completion as soon as possible and don't need to test a full length beatmap.</remarks>
        /// <returns>A path to a copy of a beatmap archive (osz). Should be deleted after use.</returns>
        public static string GetQuickTestBeatmapForImport()
        {
            string tempPath = getTempFilename();
            using (var stream = OpenResource("Archives/241526 Soleily - Renatus_virtual_quick.osz"))
            using (var newFile = File.Create(tempPath))
                stream.CopyTo(newFile);

            Assert.IsTrue(File.Exists(tempPath));
            return tempPath;
        }

        /// <summary>
        /// Retrieve a path to a copy of a full-fledged beatmap archive.
        /// </summary>
        /// <param name="virtualTrack">Whether the audio track should be virtual.</param>
        /// <returns>A path to a copy of a beatmap archive (osz). Should be deleted after use.</returns>
        public static string GetTestBeatmapForImport(bool virtualTrack = false)
        {
            string tempPath = getTempFilename();

            using (var stream = GetTestBeatmapStream(virtualTrack))
            using (var newFile = File.Create(tempPath))
                stream.CopyTo(newFile);

            Assert.IsTrue(File.Exists(tempPath));
            return tempPath;
        }

        private static string getTempFilename() => temp_storage.GetFullPath(Guid.NewGuid() + ".osz");
    }
}
