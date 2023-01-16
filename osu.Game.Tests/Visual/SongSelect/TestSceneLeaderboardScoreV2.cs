// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.Leaderboards;
using osuTK;

namespace osu.Game.Tests.Visual.SongSelect
{
    public partial class TestSceneLeaderboardScoreV2 : OsuTestScene
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            Child = new FillFlowContainer
            {
                Width = 900,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Spacing = new Vector2(0, 10),
                AutoSizeAxes = Axes.Y,
                Children = new Drawable[]
                {
                    new LeaderBoardScoreV2(),
                    new LeaderBoardScoreV2(true)
                }
            };
        }
    }
}
