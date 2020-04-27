// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Skinning;
using osu.Game.Rulesets.Taiko.UI;

namespace osu.Game.Rulesets.Taiko.Tests.Skinning
{
    [TestFixture]
    public class TestSceneHitExplosion : TaikoSkinnableTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => base.RequiredTypes.Concat(new[]
        {
            typeof(HitExplosion),
            typeof(LegacyHitExplosion),
            typeof(DefaultHitExplosion),
        }).ToList();

        [BackgroundDependencyLoader]
        private void load()
        {
            AddStep("Great", () => SetContents(() => getContentFor(HitResult.Great)));
            AddStep("Good", () => SetContents(() => getContentFor(HitResult.Good)));
            AddStep("Miss", () => SetContents(() => getContentFor(HitResult.Miss)));
        }

        private Drawable getContentFor(HitResult type)
        {
            DrawableTaikoHitObject hit;

            return new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    hit = createHit(type),
                    new HitExplosion(hit)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    }
                }
            };
        }

        private DrawableTaikoHitObject createHit(HitResult type) => new TestDrawableHit(new Hit { StartTime = Time.Current }, type);

        private class TestDrawableHit : DrawableTaikoHitObject
        {
            private readonly HitResult type;

            public TestDrawableHit(Hit hit, HitResult type)
                : base(hit)
            {
                this.type = type;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Result.Type = type;
            }

            public override bool OnPressed(TaikoAction action) => false;
        }
    }
}
