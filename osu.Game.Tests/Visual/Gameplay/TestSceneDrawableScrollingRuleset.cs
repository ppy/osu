// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Framework.MathUtils;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Configuration;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneDrawableScrollingRuleset : OsuTestScene
    {
        /// <summary>
        /// The amount of time visible by the "view window" of the playfield.
        /// All hitobjects added through <see cref="createBeatmap"/> are spaced apart by this value, such that for a beat length of 1000,
        /// there will be at most 2 hitobjects visible in the "view window".
        /// </summary>
        private const double time_range = 1000;

        private readonly ManualClock testClock = new ManualClock();
        private TestDrawableScrollingRuleset drawableRuleset;

        [SetUp]
        public void Setup() => Schedule(() => testClock.CurrentTime = 0);

        [Test]
        public void TestRelativeBeatLengthScaleSingleTimingPoint()
        {
            var beatmap = createBeatmap();
            beatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = time_range / 2 });

            createTest(beatmap, d => d.RelativeScaleBeatLengthsOverride = true);

            assertPosition(0, 0f);

            // The single timing point is 1x speed relative to itself, such that the hitobject occurring time_range milliseconds later should appear
            // at the bottom of the view window regardless of the timing point's beat length
            assertPosition(1, 1f);
        }

        [Test]
        public void TestRelativeBeatLengthScaleTimingPointBeyondEndDoesNotBecomeDominant()
        {
            var beatmap = createBeatmap();
            beatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = time_range / 2 });
            beatmap.ControlPointInfo.Add(12000, new TimingControlPoint { BeatLength = time_range });
            beatmap.ControlPointInfo.Add(100000, new TimingControlPoint { BeatLength = time_range });

            createTest(beatmap, d => d.RelativeScaleBeatLengthsOverride = true);

            assertPosition(0, 0f);
            assertPosition(1, 1f);
        }

        [Test]
        public void TestRelativeBeatLengthScaleFromSecondTimingPoint()
        {
            var beatmap = createBeatmap();
            beatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = time_range });
            beatmap.ControlPointInfo.Add(3 * time_range, new TimingControlPoint { BeatLength = time_range / 2 });

            createTest(beatmap, d => d.RelativeScaleBeatLengthsOverride = true);

            // The first timing point should have a relative velocity of 2
            assertPosition(0, 0f);
            assertPosition(1, 0.5f);
            assertPosition(2, 1f);

            // Move to the second timing point
            setTime(3 * time_range);
            assertPosition(3, 0f);

            // As above, this is the timing point that is 1x speed relative to itself, so the hitobject occurring time_range milliseconds later should be at the bottom of the view window
            assertPosition(4, 1f);
        }

        [Test]
        public void TestNonRelativeScale()
        {
            var beatmap = createBeatmap();
            beatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = time_range });
            beatmap.ControlPointInfo.Add(3 * time_range, new TimingControlPoint { BeatLength = time_range / 2 });

            createTest(beatmap);

            assertPosition(0, 0f);
            assertPosition(1, 1);

            // Move to the second timing point
            setTime(3 * time_range);
            assertPosition(3, 0f);

            // For a beat length of 500, the view window of this timing point is elongated 2x (1000 / 500), such that the second hitobject is two TimeRanges away (offscreen)
            // To bring it on-screen, half TimeRange is added to the current time, bringing the second half of the view window into view, and the hitobject should appear at the bottom
            setTime(3 * time_range + time_range / 2);
            assertPosition(4, 1f);
        }

        [Test]
        public void TestSliderMultiplierDoesNotAffectRelativeBeatLength()
        {
            var beatmap = createBeatmap();
            beatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = time_range });
            beatmap.BeatmapInfo.BaseDifficulty.SliderMultiplier = 2;

            createTest(beatmap, d => d.RelativeScaleBeatLengthsOverride = true);
            AddStep("adjust time range", () => drawableRuleset.TimeRange.Value = 5000);

            for (int i = 0; i < 5; i++)
                assertPosition(i, i / 5f);
        }

        [Test]
        public void TestSliderMultiplierAffectsNonRelativeBeatLength()
        {
            var beatmap = createBeatmap();
            beatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = time_range });
            beatmap.BeatmapInfo.BaseDifficulty.SliderMultiplier = 2;

            createTest(beatmap);
            AddStep("adjust time range", () => drawableRuleset.TimeRange.Value = 2000);

            assertPosition(0, 0);
            assertPosition(1, 1);
        }

        private void assertPosition(int index, float relativeY) => AddAssert($"hitobject {index} at {relativeY}",
            () => Precision.AlmostEquals(drawableRuleset.Playfield.AllHitObjects.ElementAt(index).DrawPosition.Y, drawableRuleset.Playfield.HitObjectContainer.DrawHeight * relativeY));

        private void setTime(double time)
        {
            AddStep($"set time = {time}", () => testClock.CurrentTime = time);
        }

        /// <summary>
        /// Creates an <see cref="IBeatmap"/>, containing 10 hitobjects and user-provided timing points.
        /// The hitobjects are spaced <see cref="time_range"/> milliseconds apart.
        /// </summary>
        /// <returns>The <see cref="IBeatmap"/>.</returns>
        private IBeatmap createBeatmap()
        {
            var beatmap = new Beatmap<HitObject> { BeatmapInfo = { Ruleset = new OsuRuleset().RulesetInfo } };

            for (int i = 0; i < 10; i++)
                beatmap.HitObjects.Add(new HitObject { StartTime = i * time_range });

            return beatmap;
        }

        private void createTest(IBeatmap beatmap, Action<TestDrawableScrollingRuleset> overrideAction = null) => AddStep("create test", () =>
        {
            var ruleset = new TestScrollingRuleset();

            drawableRuleset = (TestDrawableScrollingRuleset)ruleset.CreateDrawableRulesetWith(CreateWorkingBeatmap(beatmap), Array.Empty<Mod>());
            drawableRuleset.FrameStablePlayback = false;

            overrideAction?.Invoke(drawableRuleset);

            Child = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Y,
                Height = 0.75f,
                Width = 400,
                Masking = true,
                Clock = new FramedClock(testClock),
                Child = drawableRuleset
            };
        });

        #region Ruleset

        private class TestScrollingRuleset : Ruleset
        {
            public TestScrollingRuleset(RulesetInfo rulesetInfo = null)
                : base(rulesetInfo)
            {
            }

            public override IEnumerable<Mod> GetModsFor(ModType type) => throw new NotImplementedException();

            public override DrawableRuleset CreateDrawableRulesetWith(IWorkingBeatmap beatmap, IReadOnlyList<Mod> mods) => new TestDrawableScrollingRuleset(this, beatmap, mods);

            public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap) => new TestBeatmapConverter(beatmap);

            public override DifficultyCalculator CreateDifficultyCalculator(WorkingBeatmap beatmap) => throw new NotImplementedException();

            public override string Description { get; } = string.Empty;

            public override string ShortName { get; } = string.Empty;
        }

        private class TestDrawableScrollingRuleset : DrawableScrollingRuleset<TestHitObject>
        {
            public bool RelativeScaleBeatLengthsOverride { get; set; }

            protected override bool RelativeScaleBeatLengths => RelativeScaleBeatLengthsOverride;

            protected override ScrollVisualisationMethod VisualisationMethod => ScrollVisualisationMethod.Overlapping;

            public new Bindable<double> TimeRange => base.TimeRange;

            public TestDrawableScrollingRuleset(Ruleset ruleset, IWorkingBeatmap beatmap, IReadOnlyList<Mod> mods)
                : base(ruleset, beatmap, mods)
            {
                TimeRange.Value = time_range;
            }

            public override DrawableHitObject<TestHitObject> CreateDrawableRepresentation(TestHitObject h) => new DrawableTestHitObject(h);

            protected override PassThroughInputManager CreateInputManager() => new PassThroughInputManager();

            protected override Playfield CreatePlayfield() => new TestPlayfield();
        }

        private class TestPlayfield : ScrollingPlayfield
        {
            public TestPlayfield()
            {
                AddInternal(new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0.2f,
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Top = 150 },
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.X,
                                    Height = 2,
                                    Colour = Color4.Green
                                },
                                HitObjectContainer
                            }
                        }
                    }
                });
            }
        }

        private class TestBeatmapConverter : BeatmapConverter<TestHitObject>
        {
            public TestBeatmapConverter(IBeatmap beatmap)
                : base(beatmap)
            {
            }

            protected override IEnumerable<Type> ValidConversionTypes => new[] { typeof(HitObject) };

            protected override IEnumerable<TestHitObject> ConvertHitObject(HitObject original, IBeatmap beatmap)
            {
                yield return new TestHitObject
                {
                    StartTime = original.StartTime,
                    EndTime = (original as IHasEndTime)?.EndTime ?? (original.StartTime + 100)
                };
            }
        }

        #endregion

        #region HitObject

        private class TestHitObject : HitObject, IHasEndTime
        {
            public double EndTime { get; set; }

            public double Duration => EndTime - StartTime;
        }

        private class DrawableTestHitObject : DrawableHitObject<TestHitObject>
        {
            public DrawableTestHitObject(TestHitObject hitObject)
                : base(hitObject)
            {
                Anchor = Anchor.TopCentre;
                Origin = Anchor.TopCentre;

                Size = new Vector2(100, 25);

                AddRangeInternal(new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.LightPink
                    },
                    new Box
                    {
                        Origin = Anchor.CentreLeft,
                        RelativeSizeAxes = Axes.X,
                        Height = 2,
                        Colour = Color4.Red
                    }
                });
            }
        }

        #endregion
    }
}
