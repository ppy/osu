// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Containers;
using osu.Framework.Screens.Testing;
using osu.Framework.Graphics;
using osu.Game.Screens.Select.Leaderboards;
using OpenTK;

namespace osu.Desktop.VisualTests
{
    public class TestCaseLeaderboard : TestCase
    {
        public override string Name => @"Leaderboard";
        public override string Description => @"From song select";

        public override void Reset()
        {
            base.Reset();

            Add(new Leaderboard
            {
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Size = new Vector2(550f, 450f),
            });
        }
    }
}
