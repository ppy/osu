// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Game.Graphics.Cursor;
using osu.Game.Tournament.Screens.Drawings;

namespace osu.Game.Tournament.Tests.Screens
{
    public partial class TestSceneDrawingsScreen : TournamentScreenTestScene
    {
        [BackgroundDependencyLoader]
        private void load(Storage storage)
        {
            using (var stream = storage.CreateFileSafely("drawings.txt"))
            using (var writer = new StreamWriter(stream))
            {
                writer.WriteLine("KR : South Korea : KOR");
                writer.WriteLine("US : United States : USA");
                writer.WriteLine("PH : Philippines : PHL");
                writer.WriteLine("BR : Brazil : BRA");
                writer.WriteLine("JP : Japan : JPN");
            }

            Add(new OsuContextMenuContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = new DrawingsScreen()
            });
        }
    }
}
