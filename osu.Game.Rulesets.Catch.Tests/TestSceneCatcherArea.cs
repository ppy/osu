// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Framework.Threading;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Configuration;
using osu.Game.Rulesets.Catch.Judgements;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    public partial class TestSceneCatcherArea : CatchSkinnableTestScene
    {
        private RulesetInfo catchRuleset;

        [Resolved]
        private OsuConfigManager config { get; set; }

        private Catcher catcher => this.ChildrenOfType<Catcher>().First();

        private float circleSize;

        private ScheduledDelegate addManyFruit;

        private IBeatmapDifficultyInfo beatmapDifficulty;

        public TestSceneCatcherArea()
        {
            AddSliderStep<float>("circle size", 0, 8, 5, createCatcher);
            AddToggleStep("hyper dash", t => this.ChildrenOfType<TestCatcherArea>().ForEach(area => area.ToggleHyperDash(t)));
            AddToggleStep("toggle hit lighting", lighting => config.SetValue(OsuSetting.HitLighting, lighting));

            AddStep("catch centered fruit", () => attemptCatch(new Fruit()));
            AddStep("catch many random fruit", () =>
            {
                int count = 50;

                addManyFruit?.Cancel();
                addManyFruit = Scheduler.AddDelayed(() =>
                {
                    attemptCatch(new Fruit
                    {
                        X = (RNG.NextSingle() - 0.5f) * Catcher.CalculateCatchWidth(beatmapDifficulty) * 0.6f,
                    });

                    if (count-- == 0)
                        addManyFruit?.Cancel();
                }, 50, true);
            });
            AddStep("catch fruit last in combo", () => attemptCatch(new Fruit { LastInCombo = true }));
            AddStep("catch kiai fruit", () => attemptCatch(new TestSceneCatcher.TestKiaiFruit()));
            AddStep("miss last in combo", () => attemptCatch(new Fruit { X = 100, LastInCombo = true }));
        }

        private void attemptCatch(Fruit fruit)
        {
            fruit.X = fruit.OriginalX + catcher.X;
            fruit.ApplyDefaults(new ControlPointInfo(), beatmapDifficulty);

            foreach (var area in this.ChildrenOfType<CatcherArea>())
            {
                DrawableFruit drawable = new DrawableFruit(fruit);
                area.Add(drawable);

                Schedule(() =>
                {
                    area.OnNewResult(drawable, new CatchJudgementResult(fruit, new CatchJudgement())
                    {
                        Type = area.Catcher.CanCatch(fruit) ? HitResult.Great : HitResult.Miss
                    });

                    drawable.Expire();
                });
            }
        }

        private void createCatcher(float size)
        {
            circleSize = size;

            beatmapDifficulty = new BeatmapDifficulty
            {
                CircleSize = circleSize
            };

            SetContents(_ =>
            {
                return new CatchInputManager(catchRuleset)
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new TestCatcherArea(beatmapDifficulty)
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.TopCentre,
                        }
                    }
                };
            });
        }

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            catchRuleset = rulesets.GetRuleset(2);
        }

        private partial class TestCatcherArea : CatcherArea
        {
            public TestCatcherArea(IBeatmapDifficultyInfo beatmapDifficulty)
            {
                var droppedObjectContainer = new DroppedObjectContainer();
                Add(droppedObjectContainer);

                Catcher = new Catcher(droppedObjectContainer, beatmapDifficulty)
                {
                    X = CatchPlayfield.CENTER_X
                };
            }

            public void ToggleHyperDash(bool status) => Catcher.SetHyperDashState(status ? 2 : 1);
        }
    }
}
