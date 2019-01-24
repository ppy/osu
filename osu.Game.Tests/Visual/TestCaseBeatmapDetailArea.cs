// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Screens.Select;
using osuTK;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    [System.ComponentModel.Description("PlaySongSelect leaderboard/details area")]
    public class TestCaseBeatmapDetailArea : OsuTestCase
    {
        public TestCaseBeatmapDetailArea()
        {
            Add(new BeatmapDetailArea
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(550f, 450f),
            });
        }
    }
}
