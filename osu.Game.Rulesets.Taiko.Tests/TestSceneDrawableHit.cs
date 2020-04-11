// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;

namespace osu.Game.Rulesets.Taiko.Tests
{
    [TestFixture]
    public class TestSceneDrawableHit : TaikoSkinnableTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => base.RequiredTypes.Concat(new[]
        {
            typeof(DrawableHit),
            typeof(DrawableCentreHit),
            typeof(DrawableRimHit),
        }).ToList();

        [BackgroundDependencyLoader]
        private void load()
        {
            AddStep("Centre hit", () => SetContents(() => new DrawableCentreHit(createHitAtCurrentTime())
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            }));
            AddStep("Rim hit", () => SetContents(() => new DrawableRimHit(createHitAtCurrentTime())
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            }));
        }

        private Hit createHitAtCurrentTime()
        {
            var hit = new Hit
            {
                StartTime = Time.Current + 3000,
            };

            hit.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            return hit;
        }
    }
}
