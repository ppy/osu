// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    public partial class TestSceneSpinnerApplication : OsuTestScene
    {
        [Test]
        public void TestApplyNewSpinner()
        {
            DrawableSpinner dho = null;

            AddStep("create spinner", () => Child = dho = new DrawableSpinner(prepareObject(new Spinner
            {
                Position = new Vector2(256, 192),
                IndexInCurrentCombo = 0,
                Duration = 500,
            }))
            {
                Clock = new FramedClock(new StopwatchClock())
            });

            AddStep("rotate some", () => dho.RotationTracker.AddRotation(180));
            AddAssert("rotation is set", () => dho.Result.TotalRotation == 180);

            AddStep("apply new spinner", () => dho.Apply(prepareObject(new Spinner
            {
                Position = new Vector2(256, 192),
                ComboIndex = 1,
                Duration = 1000,
            })));

            AddAssert("rotation is reset", () => dho.Result.TotalRotation == 0);
        }

        private Spinner prepareObject(Spinner circle)
        {
            circle.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());
            return circle;
        }
    }
}
