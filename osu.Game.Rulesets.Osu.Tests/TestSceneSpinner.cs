// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public class TestSceneSpinner : OsuSkinnableTestScene
    {
        private int depthIndex;

        private TestDrawableSpinner drawableSpinner;

        [TestCase(false)]
        [TestCase(true)]
        public void TestVariousSpinners(bool autoplay)
        {
            AddStep("Miss Big", () => SetContents(() => testSingle(2)));
            AddStep("Miss Medium", () => SetContents(() => testSingle(5)));
            AddStep("Miss Small", () => SetContents(() => testSingle(7)));
            AddStep("Hit Big", () => SetContents(() => testSingle(2, autoplay)));
            AddStep("Hit Medium", () => SetContents(() => testSingle(5, autoplay)));
            AddStep("Hit Small", () => SetContents(() => testSingle(7, autoplay)));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestLongSpinner(bool autoplay)
        {
            AddStep("Very short spinner", () => SetContents(() => testSingle(5, autoplay, 2000)));
            AddUntilStep("Wait for completion", () => drawableSpinner.Result.HasResult);
            AddUntilStep("Check correct progress", () => drawableSpinner.Progress == (autoplay ? 1 : 0));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestSuperShortSpinner(bool autoplay)
        {
            AddStep("Very short spinner", () => SetContents(() => testSingle(5, autoplay, 200)));
            AddUntilStep("Wait for completion", () => drawableSpinner.Result.HasResult);
            AddUntilStep("Short spinner implicitly completes", () => drawableSpinner.Progress == 1);
        }

        private Drawable testSingle(float circleSize, bool auto = false, double length = 3000)
        {
            var spinner = new Spinner { StartTime = Time.Current + 2000, EndTime = Time.Current + +2000 + length };

            spinner.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty { CircleSize = circleSize });

            drawableSpinner = new TestDrawableSpinner(spinner, auto)
            {
                Anchor = Anchor.Centre,
                Depth = depthIndex++
            };

            foreach (var mod in SelectedMods.Value.OfType<IApplicableToDrawableHitObjects>())
                mod.ApplyToDrawableHitObjects(new[] { drawableSpinner });

            return drawableSpinner;
        }

        private class TestDrawableSpinner : DrawableSpinner
        {
            private readonly bool auto;

            public TestDrawableSpinner(Spinner s, bool auto)
                : base(s)
            {
                this.auto = auto;
            }

            protected override void Update()
            {
                base.Update();
                if (auto)
                    RotationTracker.AddRotation((float)(Clock.ElapsedFrameTime * 3));
            }
        }
    }
}
