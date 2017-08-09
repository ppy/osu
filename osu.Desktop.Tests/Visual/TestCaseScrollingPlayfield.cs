// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using OpenTK;
using osu.Desktop.Tests.Beatmaps;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;

namespace osu.Desktop.Tests.Visual
{
    /// <summary>
    /// The most minimal implementation of a playfield with scrolling hit objects.
    /// </summary>
    public class TestCaseScrollingPlayfield : OsuTestCase
    {
        public TestCaseScrollingPlayfield()
        {
            Clock = new FramedClock();

            var objects = new List<HitObject>();

            int time = 1500;
            for (int i = 0; i < 50; i++)
            {
                objects.Add(new TestHitObject { StartTime = time });

                time += 500;
            }

            Beatmap b = new Beatmap
            {
                HitObjects = objects,
                BeatmapInfo = new BeatmapInfo
                {
                    Difficulty = new BeatmapDifficulty(),
                    Metadata = new BeatmapMetadata()
                }
            };

            WorkingBeatmap beatmap = new TestWorkingBeatmap(b);

            TestRulesetContainer horizontalRulesetContainer;
            Add(horizontalRulesetContainer = new TestRulesetContainer(Axes.X, beatmap, true));

            TestRulesetContainer verticalRulesetContainer;
            Add(verticalRulesetContainer = new TestRulesetContainer(Axes.Y, beatmap, true));

            AddStep("Reverse direction", () =>
            {
                horizontalRulesetContainer.Playfield.Reversed.Toggle();
                verticalRulesetContainer.Playfield.Reversed.Toggle();
            });
        }

        private class TestRulesetContainer : ScrollingRulesetContainer<TestPlayfield, TestHitObject, TestJudgement>
        {
            private readonly Axes scrollingAxes;

            public TestRulesetContainer(Axes scrollingAxes, WorkingBeatmap beatmap, bool isForCurrentRuleset)
                : base(null, beatmap, isForCurrentRuleset)
            {
                this.scrollingAxes = scrollingAxes;
            }

            public new TestPlayfield Playfield => base.Playfield;

            public override ScoreProcessor CreateScoreProcessor() => new TestScoreProcessor();

            protected override BeatmapConverter<TestHitObject> CreateBeatmapConverter() => new TestBeatmapConverter();

            protected override Playfield<TestHitObject, TestJudgement> CreatePlayfield() => new TestPlayfield(scrollingAxes);

            protected override DrawableHitObject<TestHitObject, TestJudgement> GetVisualRepresentation(TestHitObject h) => new DrawableTestHitObject(scrollingAxes, h);
        }

        private class TestScoreProcessor : ScoreProcessor<TestHitObject, TestJudgement>
        {
            protected override void OnNewJudgement(TestJudgement judgement)
            {
            }
        }

        private class TestBeatmapConverter : BeatmapConverter<TestHitObject>
        {
            protected override IEnumerable<Type> ValidConversionTypes => new[] { typeof(HitObject) };

            protected override IEnumerable<TestHitObject> ConvertHitObject(HitObject original, Beatmap beatmap)
            {
                yield return original as TestHitObject;
            }
        }

        private class DrawableTestHitObject : DrawableScrollingHitObject<TestHitObject, TestJudgement>
        {
            public DrawableTestHitObject(Axes scrollingAxes, TestHitObject hitObject)
                : base(hitObject)
            {
                Anchor = scrollingAxes == Axes.Y ? Anchor.TopCentre : Anchor.CentreLeft;
                Origin = Anchor.Centre;

                AutoSizeAxes = Axes.Both;

                Add(new Circle
                {
                    Size = new Vector2(50)
                });
            }

            protected override TestJudgement CreateJudgement() => new TestJudgement();

            protected override void UpdateState(ArmedState state)
            {
            }
        }

        private class TestPlayfield : ScrollingPlayfield<TestHitObject, TestJudgement>
        {
            protected override Container<Drawable> Content => content;
            private readonly Container<Drawable> content;

            public TestPlayfield(Axes scrollingAxes)
                : base(scrollingAxes)
            {
                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0.2f
                    },
                    content = new Container { RelativeSizeAxes = Axes.Both }
                };
            }
        }


        private class TestHitObject : HitObject
        {
        }

        private class TestJudgement : Judgement
        {
            public override string ResultString { get { throw new NotImplementedException(); } }
            public override string MaxResultString { get { throw new NotImplementedException(); } }
        }
    }
}