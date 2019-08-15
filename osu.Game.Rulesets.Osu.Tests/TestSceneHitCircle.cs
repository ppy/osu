// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osuTK;
using System.Collections.Generic;
using System;
using osu.Game.Rulesets.Mods;
using System.Linq;
using NUnit.Framework;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public class TestSceneHitCircle : SkinnableTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(DrawableHitCircle)
        };

        private int depthIndex;

        public TestSceneHitCircle()
        {
            AddStep("Miss Big Single", () => SetContents(() => testSingle(2)));
            AddStep("Miss Medium Single", () => SetContents(() => testSingle(5)));
            AddStep("Miss Small Single", () => SetContents(() => testSingle(7)));
            AddStep("Hit Big Single", () => SetContents(() => testSingle(2, true)));
            AddStep("Hit Medium Single", () => SetContents(() => testSingle(5, true)));
            AddStep("Hit Small Single", () => SetContents(() => testSingle(7, true)));
            AddStep("Miss Big Stream", () => SetContents(() => testStream(2)));
            AddStep("Miss Medium Stream", () => SetContents(() => testStream(5)));
            AddStep("Miss Small Stream", () => SetContents(() => testStream(7)));
            AddStep("Hit Big Stream", () => SetContents(() => testStream(2, true)));
            AddStep("Hit Medium Stream", () => SetContents(() => testStream(5, true)));
            AddStep("Hit Small Stream", () => SetContents(() => testStream(7, true)));
        }

        private Drawable testSingle(float circleSize, bool auto = false, double timeOffset = 0, Vector2? positionOffset = null)
        {
            positionOffset = positionOffset ?? Vector2.Zero;

            var circle = new HitCircle
            {
                StartTime = Time.Current + 1000 + timeOffset,
                Position = positionOffset.Value,
            };

            circle.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty { CircleSize = circleSize });

            var drawable = CreateDrawableHitCircle(circle, auto);

            foreach (var mod in Mods.Value.OfType<IApplicableToDrawableHitObjects>())
                mod.ApplyToDrawableHitObjects(new[] { drawable });

            return drawable;
        }

        protected virtual TestDrawableHitCircle CreateDrawableHitCircle(HitCircle circle, bool auto) => new TestDrawableHitCircle(circle, auto)
        {
            Anchor = Anchor.Centre,
            Depth = depthIndex++
        };

        private Drawable testStream(float circleSize, bool auto = false)
        {
            var container = new Container { RelativeSizeAxes = Axes.Both };

            Vector2 pos = new Vector2(-250, 0);

            for (int i = 0; i <= 1000; i += 100)
            {
                container.Add(testSingle(circleSize, auto, i, pos));
                pos.X += 50;
            }

            return container;
        }

        protected class TestDrawableHitCircle : DrawableHitCircle
        {
            private readonly bool auto;

            public TestDrawableHitCircle(HitCircle h, bool auto)
                : base(h)
            {
                this.auto = auto;
            }

            public void TriggerJudgement() => UpdateResult(true);

            protected override void CheckForResult(bool userTriggered, double timeOffset)
            {
                if (auto && !userTriggered && timeOffset > 0)
                {
                    // force success
                    ApplyResult(r => r.Type = HitResult.Great);
                }
                else
                    base.CheckForResult(userTriggered, timeOffset);
            }
        }
    }
}
