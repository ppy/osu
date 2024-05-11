// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Models;

namespace osu.Game.Tests.Editing.Checks
{
    public static class CheckTestHelpers
    {
        public static RealmNamedFileUsage CreateMockFile(string extension) =>
            new RealmNamedFileUsage(new RealmFile { Hash = "abcdef" }, $"abc123.{extension}");
    }
}
