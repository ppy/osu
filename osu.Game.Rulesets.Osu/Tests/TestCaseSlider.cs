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

namespace osu.Game.Rulesets.Osu.Tests
{
    [Ignore("getting CI working")]
    public class TestCaseSlider : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[] { typeof(DrawableSlider) };

        private readonly Container content;
        protected override Container<Drawable> Content => content;

        private double speedMultiplier = 2;
        private double sliderMultiplier = 2;
        private int depthIndex;

        public TestCaseSlider()
        {
            base.Content.Add(content = new OsuInputManager(new RulesetInfo { ID = 0 }));

            AddStep("Single", () => addSingle());
            AddStep("Repeated (1)", () => addRepeated(1));
            AddStep("Repeated (2)", () => addRepeated(2));
            AddStep("Repeated (3)", () => addRepeated(3));
            AddStep("Repeated (4)", () => addRepeated(4));
            AddStep("Stream", addStream);

            AddSliderStep("SpeedMultiplier", 0.01, 10, 2, s => speedMultiplier = s);
            AddSliderStep("SliderMultiplier", 0.01, 10, 2, s => sliderMultiplier = s);
        }

        private void addSingle(double timeOffset = 0, Vector2? positionOffset = null)
        {
            positionOffset = positionOffset ?? Vector2.Zero;

            var slider = new Slider
            {
                StartTime = Time.Current + 1000 + timeOffset,
                Position = new Vector2(-200, 0) + positionOffset.Value,
                ControlPoints = new List<Vector2>
                {
                    new Vector2(-200, 0) + positionOffset.Value,
                    new Vector2(400, 0) + positionOffset.Value,
                },
                Distance = 400,
            };

            var cpi = new ControlPointInfo();
            cpi.DifficultyPoints.Add(new DifficultyControlPoint { SpeedMultiplier = speedMultiplier });

            var difficulty = new BeatmapDifficulty
            {
                SliderMultiplier = (float)sliderMultiplier,
                CircleSize = 0
            };

            slider.ApplyDefaults(cpi, difficulty);
            Add(new DrawableSlider(slider)
            {
                Anchor = Anchor.Centre,
                Depth = depthIndex++
            });
        }

        private void addRepeated(int repeats)
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
                ControlPoints = new List<Vector2>
                {
                    new Vector2(-200, 0),
                    new Vector2(400, 0),
                },
                Distance = 400,
                RepeatCount = repeats,
                RepeatSamples = repeatSamples
            };

            var cpi = new ControlPointInfo();
            cpi.DifficultyPoints.Add(new DifficultyControlPoint { SpeedMultiplier = speedMultiplier });

            var difficulty = new BeatmapDifficulty
            {
                SliderMultiplier = (float)sliderMultiplier,
                CircleSize = 0
            };

            slider.ApplyDefaults(cpi, difficulty);
            Add(new DrawableSlider(slider)
            {
                Anchor = Anchor.Centre,
                Depth = depthIndex++
            });
        }

        private void addStream()
        {
            Vector2 pos = Vector2.Zero;

            for (int i = 0; i <= 1000; i += 100)
            {
                addSingle(i, pos);
                pos += new Vector2(10);
            }
        }
    }
}
