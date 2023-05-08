// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Osu.Utils;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public class OsuHitObjectGenerationUtilsTest
    {
        private static Slider createTestSlider()
        {
            var slider = new Slider
            {
                Position = new Vector2(128, 128),
                Path = new SliderPath
                {
                    ControlPoints =
                    {
                        new PathControlPoint(new Vector2(), PathType.Linear),
                        new PathControlPoint(new Vector2(-64, -128), PathType.Linear), // absolute position: (64, 0)
                        new PathControlPoint(new Vector2(-128, 0), PathType.Linear) // absolute position: (0, 128)
                    }
                },
                RepeatCount = 1
            };
            slider.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());
            return slider;
        }

        [Test]
        public void TestReflectSliderHorizontallyAlongPlayfield()
        {
            var slider = createTestSlider();

            OsuHitObjectGenerationUtils.ReflectHorizontallyAlongPlayfield(slider);

            Assert.That(slider.Position, Is.EqualTo(new Vector2(OsuPlayfield.BASE_SIZE.X - 128, 128)));
            Assert.That(slider.NestedHitObjects.OfType<SliderRepeat>().Single().Position, Is.EqualTo(new Vector2(OsuPlayfield.BASE_SIZE.X - 0, 128)));
            Assert.That(slider.Path.ControlPoints.Select(point => point.Position), Is.EquivalentTo(new[]
            {
                new Vector2(),
                new Vector2(64, -128),
                new Vector2(128, 0)
            }));
        }

        [Test]
        public void TestReflectSliderVerticallyAlongPlayfield()
        {
            var slider = createTestSlider();

            OsuHitObjectGenerationUtils.ReflectVerticallyAlongPlayfield(slider);

            Assert.That(slider.Position, Is.EqualTo(new Vector2(128, OsuPlayfield.BASE_SIZE.Y - 128)));
            Assert.That(slider.NestedHitObjects.OfType<SliderRepeat>().Single().Position, Is.EqualTo(new Vector2(0, OsuPlayfield.BASE_SIZE.Y - 128)));
            Assert.That(slider.Path.ControlPoints.Select(point => point.Position), Is.EquivalentTo(new[]
            {
                new Vector2(),
                new Vector2(-64, 128),
                new Vector2(-128, 0)
            }));
        }

        [Test]
        public void TestFlipSliderInPlaceHorizontally()
        {
            var slider = createTestSlider();

            OsuHitObjectGenerationUtils.FlipSliderInPlaceHorizontally(slider);

            Assert.That(slider.Position, Is.EqualTo(new Vector2(128, 128)));
            Assert.That(slider.NestedHitObjects.OfType<SliderRepeat>().Single().Position, Is.EqualTo(new Vector2(256, 128)));
            Assert.That(slider.Path.ControlPoints.Select(point => point.Position), Is.EquivalentTo(new[]
            {
                new Vector2(),
                new Vector2(64, -128),
                new Vector2(128, 0)
            }));
        }
    }
}
