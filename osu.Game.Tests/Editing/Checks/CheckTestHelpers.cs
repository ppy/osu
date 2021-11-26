// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.IO;

namespace osu.Game.Tests.Editing.Checks
{
    public static class CheckTestHelpers
    {
        public static BeatmapSetFileInfo CreateMockFile(string extension) =>
            new BeatmapSetFileInfo
            {
                Filename = $"abc123.{extension}",
                FileInfo = new FileInfo { Hash = "abcdef" }
            };
    }
}
