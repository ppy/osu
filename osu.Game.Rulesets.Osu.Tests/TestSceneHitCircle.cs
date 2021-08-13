﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Scoring;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public class TestSceneHitCircle : OsuSkinnableTestScene
    {
        private int depthIndex;

        [Test]
        public void TestHits()
        {
            AddStep("Hit Big Single", () => SetContents(_ => testSingle(2, true)));
            AddStep("Hit Medium Single", () => SetContents(_ => testSingle(5, true)));
            AddStep("Hit Small Single", () => SetContents(_ => testSingle(7, true)));
            AddStep("Hit Big Stream", () => SetContents(_ => testStream(2, true)));
            AddStep("Hit Medium Stream", () => SetContents(_ => testStream(5, true)));
            AddStep("Hit Small Stream", () => SetContents(_ => testStream(7, true)));
        }

        [Test]
        public void TestHittingEarly()
        {
            AddStep("Hit stream early", () => SetContents(_ => testStream(5, true, -150)));
        }

        [Test]
        public void TestMisses()
        {
            AddStep("Miss Big Single", () => SetContents(_ => testSingle(2)));
            AddStep("Miss Medium Single", () => SetContents(_ => testSingle(5)));
            AddStep("Miss Small Single", () => SetContents(_ => testSingle(7)));
            AddStep("Miss Big Stream", () => SetContents(_ => testStream(2)));
            AddStep("Miss Medium Stream", () => SetContents(_ => testStream(5)));
            AddStep("Miss Small Stream", () => SetContents(_ => testStream(7)));
        }

        [Test]
        public void TestHittingLate()
        {
            AddStep("Hit stream late", () => SetContents(_ => testStream(5, true, 150)));
        }

        private Drawable testSingle(float circleSize, bool auto = false, double timeOffset = 0, Vector2? positionOffset = null)
        {
            var drawable = createSingle(circleSize, auto, timeOffset, positionOffset);

            var playfield = new TestOsuPlayfield();
            playfield.Add(drawable);
            return playfield;
        }

        private Drawable testStream(float circleSize, bool auto = false, double hitOffset = 0)
        {
            var playfield = new TestOsuPlayfield();

            Vector2 pos = new Vector2(-250, 0);

            for (int i = 0; i <= 1000; i += 100)
            {
                playfield.Add(createSingle(circleSize, auto, i, pos, hitOffset));
                pos.X += 50;
            }

            return playfield;
        }

        private TestDrawableHitCircle createSingle(float circleSize, bool auto, double timeOffset, Vector2? positionOffset, double hitOffset = 0)
        {
            positionOffset ??= Vector2.Zero;

            var circle = new HitCircle
            {
                StartTime = Time.Current + 1000 + timeOffset,
                Position = OsuPlayfield.BASE_SIZE / 4 + positionOffset.Value,
            };

            circle.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty { CircleSize = circleSize });

            var drawable = CreateDrawableHitCircle(circle, auto, hitOffset);

            foreach (var mod in SelectedMods.Value.OfType<IApplicableToDrawableHitObject>())
                mod.ApplyToDrawableHitObject(drawable);
            return drawable;
        }

        protected virtual TestDrawableHitCircle CreateDrawableHitCircle(HitCircle circle, bool auto, double hitOffset = 0) => new TestDrawableHitCircle(circle, auto, hitOffset)
        {
            Depth = depthIndex++
        };

        protected class TestDrawableHitCircle : DrawableHitCircle
        {
            private readonly bool auto;
            private readonly double hitOffset;

            public TestDrawableHitCircle(HitCircle h, bool auto, double hitOffset)
                : base(h)
            {
                this.auto = auto;
                this.hitOffset = hitOffset;
            }

            public void TriggerJudgement() => Schedule(() => UpdateResult(true));

            protected override void CheckForResult(bool userTriggered, double timeOffset)
            {
                if (auto && !userTriggered && timeOffset > hitOffset && CheckHittable?.Invoke(this, Time.Current) != false)
                {
                    // force success
                    ApplyResult(r => r.Type = HitResult.Great);
                }
                else
                    base.CheckForResult(userTriggered, timeOffset);
            }
        }

        protected class TestOsuPlayfield : OsuPlayfield
        {
            public TestOsuPlayfield()
            {
                RelativeSizeAxes = Axes.Both;
            }
        }
    }
}
