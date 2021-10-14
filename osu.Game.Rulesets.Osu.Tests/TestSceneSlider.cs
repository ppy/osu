// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Graphics;
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
using osu.Game.Beatmaps.Legacy;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public class TestSceneSlider : OsuSkinnableTestScene
    {
        private int depthIndex;

        [Test]
        public void TestVariousSliders()
        {
            AddStep("Big Single", () => SetContents(_ => testSimpleBig()));
            AddStep("Medium Single", () => SetContents(_ => testSimpleMedium()));
            AddStep("Small Single", () => SetContents(_ => testSimpleSmall()));
            AddStep("Big 1 Repeat", () => SetContents(_ => testSimpleBig(1)));
            AddStep("Medium 1 Repeat", () => SetContents(_ => testSimpleMedium(1)));
            AddStep("Small 1 Repeat", () => SetContents(_ => testSimpleSmall(1)));
            AddStep("Big 2 Repeats", () => SetContents(_ => testSimpleBig(2)));
            AddStep("Medium 2 Repeats", () => SetContents(_ => testSimpleMedium(2)));
            AddStep("Small 2 Repeats", () => SetContents(_ => testSimpleSmall(2)));

            AddStep("Slow Slider", () => SetContents(_ => testSlowSpeed())); // slow long sliders take ages already so no repeat steps
            AddStep("Slow Short Slider", () => SetContents(_ => testShortSlowSpeed()));
            AddStep("Slow Short Slider 1 Repeats", () => SetContents(_ => testShortSlowSpeed(1)));
            AddStep("Slow Short Slider 2 Repeats", () => SetContents(_ => testShortSlowSpeed(2)));

            AddStep("Fast Slider", () => SetContents(_ => testHighSpeed()));
            AddStep("Fast Slider 1 Repeat", () => SetContents(_ => testHighSpeed(1)));
            AddStep("Fast Slider 2 Repeats", () => SetContents(_ => testHighSpeed(2)));
            AddStep("Fast Short Slider", () => SetContents(_ => testShortHighSpeed()));
            AddStep("Fast Short Slider 1 Repeat", () => SetContents(_ => testShortHighSpeed(1)));
            AddStep("Fast Short Slider 2 Repeats", () => SetContents(_ => testShortHighSpeed(2)));
            AddStep("Fast Short Slider 6 Repeats", () => SetContents(_ => testShortHighSpeed(6)));

            AddStep("Perfect Curve", () => SetContents(_ => testPerfect()));
            AddStep("Perfect Curve 1 Repeat", () => SetContents(_ => testPerfect(1)));
            AddStep("Perfect Curve 2 Repeats", () => SetContents(_ => testPerfect(2)));

            AddStep("Linear Slider", () => SetContents(_ => testLinear()));
            AddStep("Linear Slider 1 Repeat", () => SetContents(_ => testLinear(1)));
            AddStep("Linear Slider 2 Repeats", () => SetContents(_ => testLinear(2)));

            AddStep("Bezier Slider", () => SetContents(_ => testBezier()));
            AddStep("Bezier Slider 1 Repeat", () => SetContents(_ => testBezier(1)));
            AddStep("Bezier Slider 2 Repeats", () => SetContents(_ => testBezier(2)));

            AddStep("Linear Overlapping", () => SetContents(_ => testLinearOverlapping()));
            AddStep("Linear Overlapping 1 Repeat", () => SetContents(_ => testLinearOverlapping(1)));
            AddStep("Linear Overlapping 2 Repeats", () => SetContents(_ => testLinearOverlapping(2)));

            AddStep("Catmull Slider", () => SetContents(_ => testCatmull()));
            AddStep("Catmull Slider 1 Repeat", () => SetContents(_ => testCatmull(1)));
            AddStep("Catmull Slider 2 Repeats", () => SetContents(_ => testCatmull(2)));

            AddStep("Big Single, Large StackOffset", () => SetContents(_ => testSimpleBigLargeStackOffset()));
            AddStep("Big 1 Repeat, Large StackOffset", () => SetContents(_ => testSimpleBigLargeStackOffset(1)));

            AddStep("Distance Overflow", () => SetContents(_ => testDistanceOverflow()));
            AddStep("Distance Overflow 1 Repeat", () => SetContents(_ => testDistanceOverflow(1)));
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
                new HitSampleInfo(HitSampleInfo.HIT_CLAP),
                new HitSampleInfo(HitSampleInfo.HIT_WHISTLE),
            });

            AddAssert("head samples updated", () => assertSamples(slider.HitObject.HeadCircle));
            AddAssert("tick samples not updated", () => slider.HitObject.NestedHitObjects.OfType<SliderTick>().All(assertTickSamples));
            AddAssert("repeat samples updated", () => slider.HitObject.NestedHitObjects.OfType<SliderRepeat>().All(assertSamples));
            AddAssert("tail has no samples", () => slider.HitObject.TailCircle.Samples.Count == 0);

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
                    slider.HitObject.NodeSamples.Add(new List<HitSampleInfo> { new HitSampleInfo(HitSampleInfo.HIT_FINISH) });

                Add(slider);
            });

            AddStep("change samples", () => slider.HitObject.Samples = new[]
            {
                new HitSampleInfo(HitSampleInfo.HIT_CLAP),
                new HitSampleInfo(HitSampleInfo.HIT_WHISTLE),
            });

            AddAssert("head samples not updated", () => assertSamples(slider.HitObject.HeadCircle));
            AddAssert("tick samples not updated", () => slider.HitObject.NestedHitObjects.OfType<SliderTick>().All(assertTickSamples));
            AddAssert("repeat samples not updated", () => slider.HitObject.NestedHitObjects.OfType<SliderRepeat>().All(assertSamples));
            AddAssert("tail has no samples", () => slider.HitObject.TailCircle.Samples.Count == 0);

            static bool assertTickSamples(SliderTick tick) => tick.Samples.Single().Name == "slidertick";

            static bool assertSamples(HitObject hitObject) => hitObject.Samples.All(s => s.Name != HitSampleInfo.HIT_CLAP && s.Name != HitSampleInfo.HIT_WHISTLE);
        }

        private Drawable testSimpleBig(int repeats = 0) => createSlider(2, repeats: repeats);

        private Drawable testSimpleBigLargeStackOffset(int repeats = 0) => createSlider(2, repeats: repeats, stackHeight: 10);

        private Drawable testDistanceOverflow(int repeats = 0)
        {
            var slider = new Slider
            {
                StartTime = Time.Current + time_offset,
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

        private Drawable testShortSlowSpeed(int repeats = 0) => createSlider(distance: max_length / 4, repeats: repeats, speedMultiplier: 0.5);

        private Drawable testHighSpeed(int repeats = 0) => createSlider(repeats: repeats, speedMultiplier: 15);

        private Drawable testShortHighSpeed(int repeats = 0) => createSlider(distance: max_length / 4, repeats: repeats, speedMultiplier: 15);

        private const double time_offset = 1500;

        private const float max_length = 200;

        private Drawable createSlider(float circleSize = 2, float distance = max_length, int repeats = 0, double speedMultiplier = 2, int stackHeight = 0)
        {
            var slider = new Slider
            {
                StartTime = Time.Current + time_offset,
                Position = new Vector2(0, -(distance / 2)),
                Path = new SliderPath(PathType.PerfectCurve, new[]
                {
                    Vector2.Zero,
                    new Vector2(0, distance),
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
                StartTime = Time.Current + time_offset,
                Position = new Vector2(-max_length / 2, 0),
                Path = new SliderPath(PathType.PerfectCurve, new[]
                {
                    Vector2.Zero,
                    new Vector2(max_length / 2, max_length / 2),
                    new Vector2(max_length, 0)
                }, max_length * 1.5f),
                RepeatCount = repeats,
            };

            return createDrawable(slider, 2, 3);
        }

        private Drawable testLinear(int repeats = 0) => createLinear(repeats);

        private Drawable createLinear(int repeats)
        {
            var slider = new Slider
            {
                StartTime = Time.Current + time_offset,
                Position = new Vector2(-max_length / 2, 0),
                Path = new SliderPath(PathType.Linear, new[]
                {
                    Vector2.Zero,
                    new Vector2(max_length * 0.375f, max_length * 0.18f),
                    new Vector2(max_length / 2, 0),
                    new Vector2(max_length * 0.75f, -max_length / 2),
                    new Vector2(max_length * 0.95f, 0),
                    new Vector2(max_length, 0)
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
                StartTime = Time.Current + time_offset,
                Position = new Vector2(-max_length / 2, 0),
                Path = new SliderPath(PathType.Bezier, new[]
                {
                    Vector2.Zero,
                    new Vector2(max_length * 0.375f, max_length * 0.18f),
                    new Vector2(max_length / 2, max_length / 4),
                    new Vector2(max_length * 0.75f, -max_length / 2),
                    new Vector2(max_length, 0)
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
                StartTime = Time.Current + time_offset,
                Position = new Vector2(0, 0),
                Path = new SliderPath(PathType.Linear, new[]
                {
                    Vector2.Zero,
                    new Vector2(-max_length / 2, 0),
                    new Vector2(0, 0),
                    new Vector2(0, -max_length / 2),
                    new Vector2(-max_length / 2, -max_length / 2),
                    new Vector2(0, -max_length / 2)
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
                StartTime = Time.Current + time_offset,
                Position = new Vector2(-max_length / 4, 0),
                Path = new SliderPath(PathType.Catmull, new[]
                {
                    Vector2.Zero,
                    new Vector2(max_length * 0.125f, max_length * 0.125f),
                    new Vector2(max_length * 0.375f, max_length * 0.125f),
                    new Vector2(max_length / 2, 0)
                }),
                RepeatCount = repeats,
                NodeSamples = repeatSamples
            };

            return createDrawable(slider, 3, 1);
        }

        private Drawable createDrawable(Slider slider, float circleSize, double speedMultiplier)
        {
            var cpi = new LegacyControlPointInfo();
            cpi.Add(0, new DifficultyControlPoint { SliderVelocity = speedMultiplier });

            slider.ApplyDefaults(cpi, new BeatmapDifficulty
            {
                CircleSize = circleSize,
                SliderTickRate = 3
            });

            var drawable = CreateDrawableSlider(slider);

            foreach (var mod in SelectedMods.Value.OfType<IApplicableToDrawableHitObject>())
                mod.ApplyToDrawableHitObject(drawable);

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
