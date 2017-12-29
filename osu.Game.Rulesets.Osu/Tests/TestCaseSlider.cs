// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Tests.Visual;
using OpenTK;
using osu.Game.Rulesets.Osu.Mods;
using OpenTK.Graphics;

namespace osu.Game.Rulesets.Osu.Tests
{
    [Ignore("getting CI working")]
    public class TestCaseSlider : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(Slider),
            typeof(HitCircle),
            typeof(OsuModHidden),
            typeof(DrawableSlider),
            typeof(DrawableHitCircle),
            typeof(DrawableSliderTick),
            typeof(DrawableRepeatPoint)
        };

        private readonly Container content;
        protected override Container<Drawable> Content => content;

        private bool hidden;
        private int repeats;
        private int depthIndex;
        private int circleSize;
        private float circleScale = 1;
        private double speedMultiplier = 2;
        private double sliderMultiplier = 2;

        public TestCaseSlider()
        {
            base.Content.Add(content = new OsuInputManager(new RulesetInfo { ID = 0 }));

            AddStep("Single", () => testSingle());
            AddStep("Stream", testStream);
            AddStep("Repeated", () => testRepeated(repeats));
            AddToggleStep("Hidden", v => hidden = v);
            AddSliderStep("Repeats", 1, 10, 1, s => repeats = s);
            AddSliderStep("CircleSize", 0, 10, 0, s => circleSize = s);
            AddSliderStep("CircleScale", 0.5f, 2, 1, s => circleScale = s);
            AddSliderStep("SpeedMultiplier", 0.1, 10, 2, s => speedMultiplier = s);
            AddSliderStep("SliderMultiplier", 0.1, 10, 2, s => sliderMultiplier = s);
        }

        private void testSingle(double timeOffset = 0, Vector2? positionOffset = null)
        {
            positionOffset = positionOffset ?? Vector2.Zero;

            var slider = new Slider
            {
                StartTime = Time.Current + 1000 + timeOffset,
                Position = new Vector2(-200, 0) + positionOffset.Value,
                ComboColour = Color4.LightSeaGreen,
                ControlPoints = new List<Vector2>
                {
                    new Vector2(-200, 0) + positionOffset.Value,
                    new Vector2(400, 0) + positionOffset.Value,
                },
                Distance = 400
            };

            addSlider(slider);
        }

        private void testRepeated(int repeats)
        {
            // The first run through the slider is considered a repeat
            repeats++;

            var repeatSamples = new List<List<SampleInfo>>();
            for (int i = 0; i < repeats; i++)
                repeatSamples.Add(new List<SampleInfo>());

            var slider = new Slider
            {
                StartTime = Time.Current + 1000,
                Position = new Vector2(-200, 0),
                ComboColour = Color4.LightSeaGreen,
                ControlPoints = new List<Vector2>
                {
                    new Vector2(-200, 0),
                    new Vector2(400, 0),
                },
                Distance = 400,
                RepeatCount = repeats,
                RepeatSamples = repeatSamples
            };

            addSlider(slider);
        }

        private void testStream()
        {
            Vector2 pos = Vector2.Zero;

            for (int i = 0; i <= 1000; i += 100)
            {
                testSingle(i, pos);
                pos += new Vector2(10);
            }
        }

        private void addSlider(Slider slider)
        {
            var cpi = new ControlPointInfo();
            cpi.DifficultyPoints.Add(new DifficultyControlPoint { SpeedMultiplier = speedMultiplier });

            var difficulty = new BeatmapDifficulty
            {
                SliderMultiplier = (float)sliderMultiplier,
                CircleSize = circleSize
            };

            slider.ApplyDefaults(cpi, difficulty);

            var drawable = new DrawableSlider(slider)
            {
                Anchor = Anchor.Centre,
                Scale = new Vector2(circleScale),
                Depth = depthIndex++
            };

            if (hidden)
                new OsuModHidden().ApplyToDrawableHitObjects(new [] { drawable });

            Add(drawable);
        }
    }

}
