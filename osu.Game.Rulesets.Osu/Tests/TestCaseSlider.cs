// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
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
using OpenTK.Graphics;
using osu.Game.Rulesets.Mods;
using System.Linq;
using osu.Game.Rulesets.Osu.Objects.Drawables.Pieces;

namespace osu.Game.Rulesets.Osu.Tests
{
    [Ignore("getting CI working")]
    public class TestCaseSlider : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(SliderBall),
            typeof(SliderBody),
            typeof(DrawableSlider),
            typeof(DrawableRepeatPoint),
            typeof(DrawableOsuHitObject)
        };

        private readonly Container content;
        protected override Container<Drawable> Content => content;

        private int depthIndex;
        protected readonly List<Mod> Mods = new List<Mod>();

        public TestCaseSlider()
        {
            base.Content.Add(content = new OsuInputManager(new RulesetInfo { ID = 0 }));

            AddStep("Big Single", () => testSimpleBig());
            AddStep("Medium Single", () => testSimpleMedium());
            AddStep("Small Single", () => testSimpleSmall());
            AddStep("Big 1 Repeat", () => testSimpleBig(1));
            AddStep("Medium 1 Repeat", () => testSimpleMedium(1));
            AddStep("Small 1 Repeat", () => testSimpleSmall(1));
            AddStep("Big 2 Repeats", () => testSimpleBig(2));
            AddStep("Medium 2 Repeats", () => testSimpleMedium(2));
            AddStep("Small 2 Repeats", () => testSimpleSmall(2));

            AddStep("Slow Slider", testSlowSpeed); // slow long sliders take ages already so no repeat steps
            AddStep("Slow Short Slider", () => testShortSlowSpeed());
            AddStep("Slow Short Slider 1 Repeats", () => testShortSlowSpeed(1));
            AddStep("Slow Short Slider 2 Repeats", () => testShortSlowSpeed(2));

            AddStep("Fast Slider", () => testHighSpeed());
            AddStep("Fast Slider 1 Repeat", () => testHighSpeed(1));
            AddStep("Fast Slider 2 Repeats", () => testHighSpeed(2));
            AddStep("Fast Short Slider", () => testShortHighSpeed());
            AddStep("Fast Short Slider 1 Repeat", () => testShortHighSpeed(1));
            AddStep("Fast Short Slider 2 Repeats", () => testShortHighSpeed(2));
            AddStep("Fast Short Slider 6 Repeats", () => testShortHighSpeed(6));

            AddStep("Perfect Curve", testCurve);
            // TODO more curve types?
        }

        private void testSimpleBig(int repeats = 0) => createSlider(2, repeats: repeats);

        private void testSimpleMedium(int repeats = 0) => createSlider(5, repeats: repeats);

        private void testSimpleSmall(int repeats = 0) => createSlider(7, repeats: repeats);

        private void testSlowSpeed() => createSlider(speedMultiplier: 0.5);

        private void testShortSlowSpeed(int repeats = 0) => createSlider(distance: 100, repeats: repeats, speedMultiplier: 0.5);

        private void testHighSpeed(int repeats = 0) => createSlider(repeats: repeats, speedMultiplier: 15);

        private void testShortHighSpeed(int repeats = 0) => createSlider(distance: 100, repeats: repeats, speedMultiplier: 15);

        private void createSlider(float circleSize = 2, float distance = 400, int repeats = 0, double speedMultiplier = 2)
        {
            var repeatSamples = new List<List<SampleInfo>>();
            for (int i = 0; i < repeats; i++)
                repeatSamples.Add(new List<SampleInfo>());

            var slider = new Slider
            {
                StartTime = Time.Current + 1000,
                Position = new Vector2(-(distance / 2), 0),
                ComboColour = Color4.LightSeaGreen,
                ControlPoints = new List<Vector2>
                {
                    new Vector2(-(distance / 2), 0),
                    new Vector2(distance / 2, 0),
                },
                Distance = distance,
                RepeatCount = repeats,
                RepeatSamples = repeatSamples
            };

            addSlider(slider, circleSize, speedMultiplier);
        }

        private void testCurve()
        {
            var slider = new Slider
            {
                StartTime = Time.Current + 1000,
                Position = new Vector2(-200, 0),
                ComboColour = Color4.LightSeaGreen,
                ControlPoints = new List<Vector2>
                {
                    new Vector2(-200, 0),
                    new Vector2(0, 200),
                    new Vector2(200, 0)
                },
                Distance = 600
            };

            addSlider(slider, 2, 3);
        }

        private void addSlider(Slider slider, float circleSize, double speedMultiplier)
        {
            var cpi = new ControlPointInfo();
            cpi.DifficultyPoints.Add(new DifficultyControlPoint { SpeedMultiplier = speedMultiplier });

            slider.ApplyDefaults(cpi, new BeatmapDifficulty { CircleSize = circleSize });

            var drawable = new DrawableSlider(slider)
            {
                Anchor = Anchor.Centre,
                Depth = depthIndex++
            };

            foreach (var mod in Mods.OfType<IApplicableToDrawableHitObjects>())
                mod.ApplyToDrawableHitObjects(new[] { drawable });

            Add(drawable);
        }
    }
}
