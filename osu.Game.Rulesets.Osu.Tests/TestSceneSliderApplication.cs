// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestSceneSliderApplication : OsuTestScene
    {
        [Test]
        public void TestApplyNewSlider()
        {
            DrawableSlider dho = null;

            AddStep("create slider", () => Child = dho = new DrawableSlider(prepareObject(new Slider
            {
                Position = new Vector2(256, 192),
                IndexInCurrentCombo = 0,
                StartTime = Time.Current,
                Path = new SliderPath(PathType.Linear, new[]
                {
                    Vector2.Zero,
                    new Vector2(150, 100),
                    new Vector2(300, 0),
                })
            })));

            AddWaitStep("wait for progression", 1);

            AddStep("apply new slider", () => dho.Apply(prepareObject(new Slider
            {
                Position = new Vector2(256, 192),
                ComboIndex = 1,
                StartTime = dho.HitObject.StartTime,
                Path = new SliderPath(PathType.Bezier, new[]
                {
                    Vector2.Zero,
                    new Vector2(150, 100),
                    new Vector2(300, 0),
                }),
                RepeatCount = 1
            }), null));
        }

        private Slider prepareObject(Slider slider)
        {
            slider.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());
            return slider;
        }
    }
}
