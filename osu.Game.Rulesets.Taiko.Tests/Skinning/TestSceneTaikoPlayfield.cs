// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Taiko.Skinning;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Taiko.Tests.Skinning
{
    public class TestSceneTaikoPlayfield : TaikoSkinnableTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => base.RequiredTypes.Concat(new[]
        {
            typeof(HitTarget),
            typeof(LegacyHitTarget),
        }).ToList();

        [Cached(typeof(IScrollingInfo))]
        private ScrollingTestContainer.TestScrollingInfo info = new ScrollingTestContainer.TestScrollingInfo
        {
            Direction = { Value = ScrollingDirection.Left },
            TimeRange = { Value = 5000 },
        };

        public TestSceneTaikoPlayfield()
        {
            AddStep("Load playfield", () => SetContents(() => new TaikoPlayfield(new ControlPointInfo())
            {
                Height = 0.4f,
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
            }));
        }
    }
}
