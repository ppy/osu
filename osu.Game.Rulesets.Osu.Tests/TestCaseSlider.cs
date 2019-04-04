// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Graphics;
using osu.Game.Rulesets.Mods;
using System.Linq;
using NUnit.Framework;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects.Drawables.Pieces;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public class TestCaseSlider : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(SliderBall),
            typeof(SliderBody),
            typeof(SliderTick),
            typeof(DrawableSlider),
            typeof(DrawableSliderTick),
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

            AddStep("Perfect Curve", () => testPerfect());
            AddStep("Perfect Curve 1 Repeat", () => testPerfect(1));
            AddStep("Perfect Curve 2 Repeats", () => testPerfect(2));

            AddStep("Linear Slider", () => testLinear());
            AddStep("Linear Slider 1 Repeat", () => testLinear(1));
            AddStep("Linear Slider 2 Repeats", () => testLinear(2));

            AddStep("Bezier Slider", () => testBezier());
            AddStep("Bezier Slider 1 Repeat", () => testBezier(1));
            AddStep("Bezier Slider 2 Repeats", () => testBezier(2));

            AddStep("Linear Overlapping", () => testLinearOverlapping());
            AddStep("Linear Overlapping 1 Repeat", () => testLinearOverlapping(1));
            AddStep("Linear Overlapping 2 Repeats", () => testLinearOverlapping(2));

            AddStep("Catmull Slider", () => testCatmull());
            AddStep("Catmull Slider 1 Repeat", () => testCatmull(1));
            AddStep("Catmull Slider 2 Repeats", () => testCatmull(2));

            AddStep("Big Single, Large StackOffset", () => testSimpleBigLargeStackOffset());
            AddStep("Big 1 Repeat, Large StackOffset", () => testSimpleBigLargeStackOffset(1));

            AddStep("Distance Overflow", () => testDistanceOverflow());
            AddStep("Distance Overflow 1 Repeat", () => testDistanceOverflow(1));
        }

        private void testSimpleBig(int repeats = 0) => createSlider(2, repeats: repeats);

        private void testSimpleBigLargeStackOffset(int repeats = 0) => createSlider(2, repeats: repeats, stackHeight: 10);

        private void testDistanceOverflow(int repeats = 0)
        {
            var slider = new Slider
            {
                StartTime = Time.Current + 1000,
                Position = new Vector2(239, 176),
                Path = new SliderPath(PathType.PerfectCurve, new[]
                {
                    Vector2.Zero,
                    new Vector2(154, 28),
                    new Vector2(52, -34)
                }, 700),
                RepeatCount = repeats,
                NodeSamples = createEmptySamples(repeats),
                StackHeight = 10
            };

            addSlider(slider, 2, 2);
        }

        private void testSimpleMedium(int repeats = 0) => createSlider(5, repeats: repeats);

        private void testSimpleSmall(int repeats = 0) => createSlider(7, repeats: repeats);

        private void testSlowSpeed() => createSlider(speedMultiplier: 0.5);

        private void testShortSlowSpeed(int repeats = 0) => createSlider(distance: 100, repeats: repeats, speedMultiplier: 0.5);

        private void testHighSpeed(int repeats = 0) => createSlider(repeats: repeats, speedMultiplier: 15);

        private void testShortHighSpeed(int repeats = 0) => createSlider(distance: 100, repeats: repeats, speedMultiplier: 15);

        private void createSlider(float circleSize = 2, float distance = 400, int repeats = 0, double speedMultiplier = 2, int stackHeight = 0)
        {
            var slider = new Slider
            {
                StartTime = Time.Current + 1000,
                Position = new Vector2(-(distance / 2), 0),
                Path = new SliderPath(PathType.PerfectCurve, new[]
                {
                    Vector2.Zero,
                    new Vector2(distance, 0),
                }, distance),
                RepeatCount = repeats,
                NodeSamples = createEmptySamples(repeats),
                StackHeight = stackHeight
            };

            addSlider(slider, circleSize, speedMultiplier);
        }

        private void testPerfect(int repeats = 0)
        {
            var slider = new Slider
            {
                StartTime = Time.Current + 1000,
                Position = new Vector2(-200, 0),
                Path = new SliderPath(PathType.PerfectCurve, new[]
                {
                    Vector2.Zero,
                    new Vector2(200, 200),
                    new Vector2(400, 0)
                }, 600),
                RepeatCount = repeats,
                NodeSamples = createEmptySamples(repeats)
            };

            addSlider(slider, 2, 3);
        }

        private void testLinear(int repeats = 0) => createLinear(repeats);

        private void createLinear(int repeats)
        {
            var slider = new Slider
            {
                StartTime = Time.Current + 1000,
                Position = new Vector2(-200, 0),
                Path = new SliderPath(PathType.Linear, new[]
                {
                    Vector2.Zero,
                    new Vector2(150, 75),
                    new Vector2(200, 0),
                    new Vector2(300, -200),
                    new Vector2(400, 0),
                    new Vector2(430, 0)
                }),
                RepeatCount = repeats,
                NodeSamples = createEmptySamples(repeats)
            };

            addSlider(slider, 2, 3);
        }

        private void testBezier(int repeats = 0) => createBezier(repeats);

        private void createBezier(int repeats)
        {
            var slider = new Slider
            {
                StartTime = Time.Current + 1000,
                Position = new Vector2(-200, 0),
                Path = new SliderPath(PathType.Bezier, new[]
                {
                    Vector2.Zero,
                    new Vector2(150, 75),
                    new Vector2(200, 100),
                    new Vector2(300, -200),
                    new Vector2(430, 0)
                }),
                RepeatCount = repeats,
                NodeSamples = createEmptySamples(repeats)
            };

            addSlider(slider, 2, 3);
        }

        private void testLinearOverlapping(int repeats = 0) => createOverlapping(repeats);

        private void createOverlapping(int repeats)
        {
            var slider = new Slider
            {
                StartTime = Time.Current + 1000,
                Position = new Vector2(0, 0),
                Path = new SliderPath(PathType.Linear, new[]
                {
                    Vector2.Zero,
                    new Vector2(-200, 0),
                    new Vector2(0, 0),
                    new Vector2(0, -200),
                    new Vector2(-200, -200),
                    new Vector2(0, -200)
                }),
                RepeatCount = repeats,
                NodeSamples = createEmptySamples(repeats)
            };

            addSlider(slider, 2, 3);
        }

        private void testCatmull(int repeats = 0) => createCatmull(repeats);

        private void createCatmull(int repeats = 0)
        {
            var repeatSamples = new List<List<SampleInfo>>();
            for (int i = 0; i < repeats; i++)
                repeatSamples.Add(new List<SampleInfo>());

            var slider = new Slider
            {
                StartTime = Time.Current + 1000,
                Position = new Vector2(-100, 0),
                Path = new SliderPath(PathType.Catmull, new[]
                {
                    Vector2.Zero,
                    new Vector2(50, -50),
                    new Vector2(150, 50),
                    new Vector2(200, 0)
                }),
                RepeatCount = repeats,
                NodeSamples = repeatSamples
            };

            addSlider(slider, 3, 1);
        }

        private List<List<SampleInfo>> createEmptySamples(int repeats)
        {
            var repeatSamples = new List<List<SampleInfo>>();
            for (int i = 0; i < repeats; i++)
                repeatSamples.Add(new List<SampleInfo>());
            return repeatSamples;
        }

        private void addSlider(Slider slider, float circleSize, double speedMultiplier)
        {
            var cpi = new ControlPointInfo();
            cpi.DifficultyPoints.Add(new DifficultyControlPoint { SpeedMultiplier = speedMultiplier });

            slider.ApplyDefaults(cpi, new BeatmapDifficulty { CircleSize = circleSize, SliderTickRate = 3 });

            var drawable = new DrawableSlider(slider)
            {
                Anchor = Anchor.Centre,
                Depth = depthIndex++
            };

            foreach (var mod in Mods.OfType<IApplicableToDrawableHitObjects>())
                mod.ApplyToDrawableHitObjects(new[] { drawable });

            drawable.OnNewResult += onNewResult;

            Add(drawable);
        }

        private float judgementOffsetDirection = 1;

        private void onNewResult(DrawableHitObject judgedObject, JudgementResult result)
        {
            var osuObject = judgedObject as DrawableOsuHitObject;
            if (osuObject == null)
                return;

            OsuSpriteText text;
            Add(text = new OsuSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Text = result.IsHit ? "Hit!" : "Miss!",
                Colour = result.IsHit ? Color4.Green : Color4.Red,
                Font = OsuFont.GetFont(size: 30),
                Position = osuObject.HitObject.StackedEndPosition + judgementOffsetDirection * new Vector2(0, 45)
            });

            text.Delay(150)
                .Then().FadeOut(200)
                .Then().Expire();

            judgementOffsetDirection *= -1;
        }
    }
}
