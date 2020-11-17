// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
    public class TestSceneHitCircleApplication : OsuTestScene
    {
        [Test]
        public void TestApplyNewCircle()
        {
            DrawableHitCircle dho = null;

            AddStep("create circle", () => Child = dho = new DrawableHitCircle(prepareObject(new HitCircle
            {
                Position = new Vector2(256, 192),
                IndexInCurrentCombo = 0
            }))
            {
                Clock = new FramedClock(new StopwatchClock())
            });

            AddStep("apply new circle", () => dho.Apply(prepareObject(new HitCircle
            {
                Position = new Vector2(128, 128),
                ComboIndex = 1,
            }), null));
        }

        private HitCircle prepareObject(HitCircle circle)
        {
            circle.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());
            return circle;
        }
    }
}
