// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    public partial class TestSceneHitCircleLateFade : OsuTestScene
    {
        [Test]
        public void TestFadeOutIntoMiss()
        {
            float? alphaAtMiss = null;

            AddStep("Create hit circle", () =>
            {
                alphaAtMiss = null;

                DrawableHitCircle drawableHitCircle = new DrawableHitCircle(new HitCircle
                {
                    StartTime = Time.Current + 500,
                    Position = new Vector2(250)
                });

                drawableHitCircle.HitObject.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());

                drawableHitCircle.OnNewResult += (_, _) =>
                {
                    alphaAtMiss = drawableHitCircle.Alpha;
                };

                Child = drawableHitCircle;
            });

            AddUntilStep("Wait until circle is missed", () => alphaAtMiss.IsNotNull());
            AddAssert("Transparent when missed", () => alphaAtMiss == 0);
        }
    }
}
