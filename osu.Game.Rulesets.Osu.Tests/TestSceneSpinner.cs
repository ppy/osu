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

        public TestSceneSpinner()
        {
            //            base.Content.Add(content = new OsuInputManager(new RulesetInfo { ID = 0 }));

            AddStep("Miss Big", () => SetContents(() => testSingle(2)));
            AddStep("Miss Medium", () => SetContents(() => testSingle(5)));
            AddStep("Miss Small", () => SetContents(() => testSingle(7)));
            AddStep("Hit Big", () => SetContents(() => testSingle(2, true)));
            AddStep("Hit Medium", () => SetContents(() => testSingle(5, true)));
            AddStep("Hit Small", () => SetContents(() => testSingle(7, true)));
        }

        private Drawable testSingle(float circleSize, bool auto = false)
        {
            var spinner = new Spinner { StartTime = Time.Current + 2000, EndTime = Time.Current + 5000 };

            spinner.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty { CircleSize = circleSize });

            var drawable = new TestDrawableSpinner(spinner, auto)
            {
                Anchor = Anchor.Centre,
                Depth = depthIndex++
            };

            foreach (var mod in SelectedMods.Value.OfType<IApplicableToDrawableHitObjects>())
                mod.ApplyToDrawableHitObjects(new[] { drawable });

            return drawable;
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
