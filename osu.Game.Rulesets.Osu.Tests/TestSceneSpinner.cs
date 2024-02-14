// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public partial class TestSceneSpinner : OsuSkinnableTestScene
    {
        private int depthIndex;

        private TestDrawableSpinner drawableSpinner;

        private readonly BindableDouble spinRate = new BindableDouble();

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddSliderStep("Spin rate", 0.5, 5, 1, val => spinRate.Value = val);
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("Reset rate", () => spinRate.Value = 1);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestVariousSpinners(bool autoplay)
        {
            string term = autoplay ? "Hit" : "Miss";
            AddStep($"{term} Big", () => SetContents(_ => testSingle(2, autoplay)));
            AddStep($"{term} Medium", () => SetContents(_ => testSingle(5, autoplay)));
            AddStep($"{term} Small", () => SetContents(_ => testSingle(7, autoplay)));
        }

        [Test]
        public void TestSpinnerNoBonus()
        {
            AddStep("Set high spin rate", () => spinRate.Value = 5);

            Spinner spinner;

            AddStep("add spinner", () => SetContents(_ =>
            {
                spinner = new Spinner
                {
                    StartTime = Time.Current,
                    EndTime = Time.Current + 750,
                    Samples = new List<HitSampleInfo>
                    {
                        new HitSampleInfo(HitSampleInfo.HIT_NORMAL)
                    }
                };

                spinner.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty { OverallDifficulty = 0 });

                return drawableSpinner = new TestDrawableSpinner(spinner, true, spinRate)
                {
                    Anchor = Anchor.Centre,
                    Depth = depthIndex++,
                    Scale = new Vector2(0.75f)
                };
            }));
        }

        [Test]
        public void TestSpinningSamplePitchShift()
        {
            AddStep("Add spinner", () => SetContents(_ => testSingle(5, true, 4000)));
            AddUntilStep("Pitch starts low", () => getSpinningSample().Frequency.Value < 0.8);
            AddUntilStep("Pitch increases", () => getSpinningSample().Frequency.Value > 0.8);

            PausableSkinnableSound getSpinningSample() =>
                drawableSpinner.ChildrenOfType<PausableSkinnableSound>().FirstOrDefault(s => s.Samples.Any(i => i.LookupNames.Any(l => l.Contains("spinnerspin"))));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestLongSpinner(bool autoplay)
        {
            AddStep("Very long spinner", () => SetContents(_ => testSingle(5, autoplay, 4000)));
            AddUntilStep("Wait for completion", () => drawableSpinner.Result.HasResult);
            AddUntilStep("Check correct progress", () => drawableSpinner.Progress == (autoplay ? 1 : 0));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestSuperShortSpinner(bool autoplay)
        {
            AddStep("Very short spinner", () => SetContents(_ => testSingle(5, autoplay, 200)));
            AddUntilStep("Wait for completion", () => drawableSpinner.Result.HasResult);
            AddUntilStep("Short spinner implicitly completes", () => drawableSpinner.Progress == 1);
        }

        [TestCase(0, 4, 6)]
        [TestCase(5, 7, 10)]
        [TestCase(10, 11, 8)]
        public void TestSpinnerSpinRequirements(int od, int normalTicks, int bonusTicks)
        {
            Spinner spinner = null;

            AddStep("add spinner", () => SetContents(_ =>
            {
                spinner = new Spinner
                {
                    StartTime = Time.Current,
                    EndTime = Time.Current + 3000,
                    Samples = new List<HitSampleInfo>
                    {
                        new HitSampleInfo(HitSampleInfo.HIT_NORMAL)
                    }
                };

                spinner.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty { OverallDifficulty = od });

                return drawableSpinner = new TestDrawableSpinner(spinner, true, spinRate)
                {
                    Anchor = Anchor.Centre,
                    Depth = depthIndex++,
                    Scale = new Vector2(0.75f)
                };
            }));

            AddAssert("number of normal ticks matches", () => spinner.SpinsRequired, () => Is.EqualTo(normalTicks));
            AddAssert("number of bonus ticks matches", () => spinner.MaximumBonusSpins, () => Is.EqualTo(bonusTicks));
        }

        private Drawable testSingle(float circleSize, bool auto = false, double length = 3000)
        {
            const double delay = 2000;

            var spinner = new Spinner
            {
                StartTime = Time.Current + delay,
                EndTime = Time.Current + delay + length,
                Samples = new List<HitSampleInfo>
                {
                    new HitSampleInfo(HitSampleInfo.HIT_NORMAL)
                }
            };

            spinner.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty { CircleSize = circleSize });

            drawableSpinner = new TestDrawableSpinner(spinner, auto, spinRate)
            {
                Anchor = Anchor.Centre,
                Depth = depthIndex++,
                Scale = new Vector2(0.75f)
            };

            foreach (var mod in SelectedMods.Value.OfType<IApplicableToDrawableHitObject>())
                mod.ApplyToDrawableHitObject(drawableSpinner);

            return drawableSpinner;
        }

        private partial class TestDrawableSpinner : DrawableSpinner
        {
            private readonly bool auto;
            private readonly BindableDouble spinRate;

            public TestDrawableSpinner(Spinner s, bool auto, BindableDouble spinRate)
                : base(s)
            {
                this.auto = auto;
                this.spinRate = spinRate;
            }

            protected override void Update()
            {
                base.Update();
                if (auto)
                    RotationTracker.AddRotation((float)Math.Min(180, Clock.ElapsedFrameTime * spinRate.Value));
            }
        }
    }
}
