// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Overlays.Profile.Header.Components;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public partial class TestSceneLevelBadge : OsuTestScene
    {
        public TestSceneLevelBadge()
        {
            var levels = new List<UserStatistics.LevelInfo>();

            for (int i = 0; i < 11; i++)
            {
                levels.Add(new UserStatistics.LevelInfo
                {
                    Current = i * 10
                });
            }

            levels.Add(new UserStatistics.LevelInfo { Current = 101 });
            levels.Add(new UserStatistics.LevelInfo { Current = 105 });
            levels.Add(new UserStatistics.LevelInfo { Current = 110 });
            levels.Add(new UserStatistics.LevelInfo { Current = 115 });
            levels.Add(new UserStatistics.LevelInfo { Current = 120 });

            Children = new Drawable[]
            {
                new FillFlowContainer<LevelBadge>
                {
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Full,
                    Spacing = new Vector2(5),
                    ChildrenEnumerable = levels.Select(level => new LevelBadge
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(60),
                        LevelInfo = { Value = level }
                    })
                }
            };
        }
    }
}
