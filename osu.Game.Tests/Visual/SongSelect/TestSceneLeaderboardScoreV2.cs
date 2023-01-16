// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.Leaderboards;

namespace osu.Game.Tests.Visual.SongSelect
{
    public partial class TestSceneLeaderboardScoreV2 : OsuTestScene
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            Child = new Container
            {
                Width = 900,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Y,
                Child = new LeaderBoardScoreV2()
            };
        }
    }
}
