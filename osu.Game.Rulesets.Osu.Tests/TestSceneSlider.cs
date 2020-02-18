﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
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
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public class TestSceneSlider : SkinnableTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(Slider),
            typeof(SliderTick),
            typeof(SliderTailCircle),
            typeof(SliderBall),
            typeof(SliderBody),
            typeof(SnakingSliderBody),
            typeof(DrawableSlider),
            typeof(DrawableSliderTick),
            typeof(DrawableSliderTail),
            typeof(DrawableSliderHead),
            typeof(DrawableRepeatPoint),
            typeof(DrawableOsuHitObject)
        };

        private Container content;

        protected override Container<Drawable> Content
        {
            get
            {
                if (content == null)
                    base.Content.Add(content = new OsuInputManager(new RulesetInfo { ID = 0 }));

                return content;
            }
        }

        private int depthIndex;

        public TestSceneSlider()
        {
            AddStep("Big Single", () => SetContents(() => testSimpleBig()));
            AddStep("Medium Single", () => SetContents(() => testSimpleMedium()));
            AddStep("Small Single", () => SetContents(() => testSimpleSmall()));
            AddStep("Big 1 Repeat", () => SetContents(() => testSimpleBig(1)));
            AddStep("Medium 1 Repeat", () => SetContents(() => testSimpleMedium(1)));
            AddStep("Small 1 Repeat", () => SetContents(() => testSimpleSmall(1)));
            AddStep("Big 2 Repeats", () => SetContents(() => testSimpleBig(2)));
            AddStep("Medium 2 Repeats", () => SetContents(() => testSimpleMedium(2)));
            AddStep("Small 2 Repeats", () => SetContents(() => testSimpleSmall(2)));

            AddStep("Slow Slider", () => SetContents(testSlowSpeed)); // slow long sliders take ages already so no repeat steps
            AddStep("Slow Short Slider", () => SetContents(() => testShortSlowSpeed()));
            AddStep("Slow Short Slider 1 Repeats", () => SetContents(() => testShortSlowSpeed(1)));
            AddStep("Slow Short Slider 2 Repeats", () => SetContents(() => testShortSlowSpeed(2)));

            AddStep("Fast Slider", () => SetContents(() => testHighSpeed()));
            AddStep("Fast Slider 1 Repeat", () => SetContents(() => testHighSpeed(1)));
            AddStep("Fast Slider 2 Repeats", () => SetContents(() => testHighSpeed(2)));
            AddStep("Fast Short Slider", () => SetContents(() => testShortHighSpeed()));
            AddStep("Fast Short Slider 1 Repeat", () => SetContents(() => testShortHighSpeed(1)));
            AddStep("Fast Short Slider 2 Repeats", () => SetContents(() => testShortHighSpeed(2)));
            AddStep("Fast Short Slider 6 Repeats", () => SetContents(() => testShortHighSpeed(6)));

            AddStep("Perfect Curve", () => SetContents(() => testPerfect()));
            AddStep("Perfect Curve 1 Repeat", () => SetContents(() => testPerfect(1)));
            AddStep("Perfect Curve 2 Repeats", () => SetContents(() => testPerfect(2)));

            AddStep("Linear Slider", () => SetContents(() => testLinear()));
            AddStep("Linear Slider 1 Repeat", () => SetContents(() => testLinear(1)));
            AddStep("Linear Slider 2 Repeats", () => SetContents(() => testLinear(2)));

            AddStep("Bezier Slider", () => SetContents(() => testBezier()));
            AddStep("Bezier Slider 1 Repeat", () => SetContents(() => testBezier(1)));
            AddStep("Bezier Slider 2 Repeats", () => SetContents(() => testBezier(2)));

            AddStep("Linear Overlapping", () => SetContents(() => testLinearOverlapping()));
            AddStep("Linear Overlapping 1 Repeat", () => SetContents(() => testLinearOverlapping(1)));
            AddStep("Linear Overlapping 2 Repeats", () => SetContents(() => testLinearOverlapping(2)));

            AddStep("Catmull Slider", () => SetContents(() => testCatmull()));
            AddStep("Catmull Slider 1 Repeat", () => SetContents(() => testCatmull(1)));
            AddStep("Catmull Slider 2 Repeats", () => SetContents(() => testCatmull(2)));

            AddStep("Big Single, Large StackOffset", () => SetContents(() => testSimpleBigLargeStackOffset()));
            AddStep("Big 1 Repeat, Large StackOffset", () => SetContents(() => testSimpleBigLargeStackOffset(1)));

            AddStep("Distance Overflow", () => SetContents(() => testDistanceOverflow()));
            AddStep("Distance Overflow 1 Repeat", () => SetContents(() => testDistanceOverflow(1)));
        }

        [Test]
        public void TestChangeStackHeight()
        {
            DrawableSlider slider = null;

            AddStep("create slider", () =>
            {
                slider = (DrawableSlider)createSlider(repeats: 1);
                Add(slider);
            });

            AddStep("change stack height", () => slider.HitObject.StackHeight = 10);
            AddAssert("body positioned correctly", () => slider.Position == slider.HitObject.StackedPosition);
        }

        [Test]
        public void TestChangeSamplesWithNoNodeSamples()
        {
            DrawableSlider slider = null;

            AddStep("create slider", () =>
            {
                slider = (DrawableSlider)createSlider(repeats: 1);
                Add(slider);
            });

            AddStep("change samples", () => slider.HitObject.Samples = new[]
            {
                new HitSampleInfo { Name = HitSampleInfo.HIT_CLAP },
                new HitSampleInfo { Name = HitSampleInfo.HIT_WHISTLE },
            });

            AddAssert("head samples updated", () => assertSamples(((Slider)slider.HitObject).HeadCircle));
            AddAssert("tick samples not updated", () => ((Slider)slider.HitObject).NestedHitObjects.OfType<SliderTick>().All(assertTickSamples));
            AddAssert("repeat samples updated", () => ((Slider)slider.HitObject).NestedHitObjects.OfType<RepeatPoint>().All(assertSamples));
            AddAssert("tail has no samples", () => ((Slider)slider.HitObject).TailCircle.Samples.Count == 0);

            static bool assertTickSamples(SliderTick tick) => tick.Samples.Single().Name == "slidertick";

            static bool assertSamples(HitObject hitObject)
            {
                return hitObject.Samples.Any(s => s.Name == HitSampleInfo.HIT_CLAP)
                       && hitObject.Samples.Any(s => s.Name == HitSampleInfo.HIT_WHISTLE);
            }
        }

        [Test]
        public void TestChangeSamplesWithNodeSamples()
        {
            DrawableSlider slider = null;

            AddStep("create slider", () =>
            {
                slider = (DrawableSlider)createSlider(repeats: 1);

                for (int i = 0; i < 2; i++)
                    ((Slider)slider.HitObject).NodeSamples.Add(new List<HitSampleInfo> { new HitSampleInfo { Name = HitSampleInfo.HIT_FINISH } });

                Add(slider);
            });

            AddStep("change samples", () => slider.HitObject.Samples = new[]
            {
                new HitSampleInfo { Name = HitSampleInfo.HIT_CLAP },
                new HitSampleInfo { Name = HitSampleInfo.HIT_WHISTLE },
            });

            AddAssert("head samples not updated", () => assertSamples(((Slider)slider.HitObject).HeadCircle));
            AddAssert("tick samples not updated", () => ((Slider)slider.HitObject).NestedHitObjects.OfType<SliderTick>().All(assertTickSamples));
            AddAssert("repeat samples not updated", () => ((Slider)slider.HitObject).NestedHitObjects.OfType<RepeatPoint>().All(assertSamples));
            AddAssert("tail has no samples", () => ((Slider)slider.HitObject).TailCircle.Samples.Count == 0);

            static bool assertTickSamples(SliderTick tick) => tick.Samples.Single().Name == "slidertick";

            static bool assertSamples(HitObject hitObject) => hitObject.Samples.All(s => s.Name != HitSampleInfo.HIT_CLAP && s.Name != HitSampleInfo.HIT_WHISTLE);
        }

        private Drawable testSimpleBig(int repeats = 0) => createSlider(2, repeats: repeats);

        private Drawable testSimpleBigLargeStackOffset(int repeats = 0) => createSlider(2, repeats: repeats, stackHeight: 10);

        private Drawable testDistanceOverflow(int repeats = 0)
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
                StackHeight = 10
            };

            return createDrawable(slider, 2, 2);
        }

        private Drawable testSimpleMedium(int repeats = 0) => createSlider(5, repeats: repeats);

        private Drawable testSimpleSmall(int repeats = 0) => createSlider(7, repeats: repeats);

        private Drawable testSlowSpeed() => createSlider(speedMultiplier: 0.5);

        private Drawable testShortSlowSpeed(int repeats = 0) => createSlider(distance: 100, repeats: repeats, speedMultiplier: 0.5);

        private Drawable testHighSpeed(int repeats = 0) => createSlider(repeats: repeats, speedMultiplier: 15);

        private Drawable testShortHighSpeed(int repeats = 0) => createSlider(distance: 100, repeats: repeats, speedMultiplier: 15);

        private Drawable createSlider(float circleSize = 2, float distance = 400, int repeats = 0, double speedMultiplier = 2, int stackHeight = 0)
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
                StackHeight = stackHeight
            };

            return createDrawable(slider, circleSize, speedMultiplier);
        }

        private Drawable testPerfect(int repeats = 0)
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
            };

            return createDrawable(slider, 2, 3);
        }

        private Drawable testLinear(int repeats = 0) => createLinear(repeats);

        private Drawable createLinear(int repeats)
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
            };

            return createDrawable(slider, 2, 3);
        }

        private Drawable testBezier(int repeats = 0) => createBezier(repeats);

        private Drawable createBezier(int repeats)
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
            };

            return createDrawable(slider, 2, 3);
        }

        private Drawable testLinearOverlapping(int repeats = 0) => createOverlapping(repeats);

        private Drawable createOverlapping(int repeats)
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
            };

            return createDrawable(slider, 2, 3);
        }

        private Drawable testCatmull(int repeats = 0) => createCatmull(repeats);

        private Drawable createCatmull(int repeats = 0)
        {
            var repeatSamples = new List<IList<HitSampleInfo>>();
            for (int i = 0; i < repeats; i++)
                repeatSamples.Add(new List<HitSampleInfo>());

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

            return createDrawable(slider, 3, 1);
        }

        private Drawable createDrawable(Slider slider, float circleSize, double speedMultiplier)
        {
            var cpi = new ControlPointInfo();
            cpi.Add(0, new DifficultyControlPoint { SpeedMultiplier = speedMultiplier });

            slider.ApplyDefaults(cpi, new BeatmapDifficulty { CircleSize = circleSize, SliderTickRate = 3 });

            var drawable = CreateDrawableSlider(slider);

            foreach (var mod in SelectedMods.Value.OfType<IApplicableToDrawableHitObjects>())
                mod.ApplyToDrawableHitObjects(new[] { drawable });

            drawable.OnNewResult += onNewResult;

            return drawable;
        }

        protected virtual DrawableSlider CreateDrawableSlider(Slider slider) => new DrawableSlider(slider)
        {
            Anchor = Anchor.Centre,
            Depth = depthIndex++
        };

        private float judgementOffsetDirection = 1;

        private void onNewResult(DrawableHitObject judgedObject, JudgementResult result)
        {
            if (!(judgedObject is DrawableOsuHitObject osuObject))
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
