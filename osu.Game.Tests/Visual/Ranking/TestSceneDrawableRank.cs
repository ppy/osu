// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.Leaderboards;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Tests.Visual.Ranking
{
    public partial class TestSceneDrawableRank : OsuTestScene
    {
        [Test]
        public void TestAllRanks()
        {
            AddStep("create content", () => Child = new FillFlowContainer<DrawableRank>
            {
                AutoSizeAxes = Axes.X,
                RelativeSizeAxes = Axes.Y,
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Direction = FillDirection.Vertical,
                Padding = new MarginPadding(20),
                Spacing = new Vector2(10),
                ChildrenEnumerable = Enum.GetValues<ScoreRank>().OrderBy(v => v).Select(rank => new DrawableRank(rank)
                {
                    RelativeSizeAxes = Axes.None,
                    Size = new Vector2(50, 25),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                })
            });
        }
    }
}
