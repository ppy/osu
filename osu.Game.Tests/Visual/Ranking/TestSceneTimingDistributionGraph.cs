// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Osu.Statistics;
using osu.Game.Rulesets.Scoring;
using osuTK;

namespace osu.Game.Tests.Visual.Ranking
{
    public class TestSceneTimingDistributionGraph : OsuTestScene
    {
        public TestSceneTimingDistributionGraph()
        {
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4Extensions.FromHex("#333")
                },
                new TimingDistributionGraph(CreateDistributedHitEvents())
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(400, 130)
                }
            };
        }

        public static List<HitEvent> CreateDistributedHitEvents()
        {
            var hitEvents = new List<HitEvent>();

            for (int i = 0; i < 50; i++)
            {
                int count = (int)(Math.Pow(25 - Math.Abs(i - 25), 2));

                for (int j = 0; j < count; j++)
                    hitEvents.Add(new HitEvent(i - 25, HitResult.Perfect, new HitCircle(), new HitCircle(), null));
            }

            return hitEvents;
        }
    }
}
