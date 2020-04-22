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
using osu.Game.Rulesets.Taiko.Skinning;

namespace osu.Game.Rulesets.Taiko.Tests.Skinning
{
    [TestFixture]
    public class TestSceneDrawableHit : TaikoSkinnableTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => base.RequiredTypes.Concat(new[]
        {
            typeof(DrawableHit),
            typeof(DrawableCentreHit),
            typeof(DrawableRimHit),
            typeof(LegacyHit),
            typeof(LegacyCirclePiece),
        }).ToList();

        [BackgroundDependencyLoader]
        private void load()
        {
            AddStep("Centre hit", () => SetContents(() => new DrawableCentreHit(createHitAtCurrentTime())
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            }));

            AddStep("Centre hit (strong)", () => SetContents(() => new DrawableCentreHit(createHitAtCurrentTime(true))
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            }));

            AddStep("Rim hit", () => SetContents(() => new DrawableRimHit(createHitAtCurrentTime())
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            }));

            AddStep("Rim hit (strong)", () => SetContents(() => new DrawableRimHit(createHitAtCurrentTime(true))
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            }));
        }

        private Hit createHitAtCurrentTime(bool strong = false)
        {
            var hit = new Hit
            {
                IsStrong = strong,
                StartTime = Time.Current + 3000,
            };

            hit.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

            return hit;
        }
    }
}
