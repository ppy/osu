// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Screens.Select;
using OpenTK;

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
