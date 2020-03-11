// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Catch.Judgements;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    public class TestSceneCatcherArea : SkinnableTestScene
    {
        private RulesetInfo catchRuleset;

        public TestSceneCatcherArea()
        {
            AddSliderStep<float>("CircleSize", 0, 8, 5, createCatcher);
            AddToggleStep("Hyperdash", t =>
                CreatedDrawables.OfType<CatchInputManager>().Select(i => i.Child)
                                .OfType<TestCatcherArea>().ForEach(c => c.ToggleHyperDash(t)));

            AddRepeatStep("catch fruit", () => catchFruit(new TestFruit(false)
            {
                X = this.ChildrenOfType<CatcherArea>().First().MovableCatcher.X
            }), 20);
            AddRepeatStep("catch fruit last in combo", () => catchFruit(new TestFruit(false)
            {
                X = this.ChildrenOfType<CatcherArea>().First().MovableCatcher.X,
                LastInCombo = true,
            }), 20);
            AddRepeatStep("catch kiai fruit", () => catchFruit(new TestFruit(true)
            {
                X = this.ChildrenOfType<CatcherArea>().First().MovableCatcher.X,
            }), 20);
            AddRepeatStep("miss fruit", () => catchFruit(new Fruit
            {
                X = this.ChildrenOfType<CatcherArea>().First().MovableCatcher.X + 100,
                LastInCombo = true,
            }, true), 20);
        }

        private void catchFruit(Fruit fruit, bool miss = false)
        {
            this.ChildrenOfType<CatcherArea>().ForEach(area =>
            {
                DrawableFruit drawable = new DrawableFruit(fruit);
                area.Add(drawable);

                Schedule(() =>
                {
                    area.AttemptCatch(fruit);
                    area.OnResult(drawable, new JudgementResult(fruit, new CatchJudgement()) { Type = miss ? HitResult.Miss : HitResult.Great });

                    drawable.Expire();
                });
            });
        }

        private void createCatcher(float size)
        {
            SetContents(() => new CatchInputManager(catchRuleset)
            {
                RelativeSizeAxes = Axes.Both,
                Child = new TestCatcherArea(new BeatmapDifficulty { CircleSize = size })
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.TopLeft,
                    CreateDrawableRepresentation = ((DrawableRuleset<CatchHitObject>)catchRuleset.CreateInstance().CreateDrawableRulesetWith(new CatchBeatmap())).CreateDrawableRepresentation
                },
            });
        }

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            catchRuleset = rulesets.GetRuleset(2);
        }

        public class TestFruit : Fruit
        {
            public TestFruit(bool kiai)
            {
                var kiaiCpi = new ControlPointInfo();
                kiaiCpi.Add(0, new EffectControlPoint { KiaiMode = kiai });

                ApplyDefaultsToSelf(kiaiCpi, new BeatmapDifficulty());
            }
        }

        private class TestCatcherArea : CatcherArea
        {
            public TestCatcherArea(BeatmapDifficulty beatmapDifficulty)
                : base(beatmapDifficulty)
            {
            }

            public new Catcher MovableCatcher => base.MovableCatcher;

            public void ToggleHyperDash(bool status) => MovableCatcher.SetHyperDashState(status ? 2 : 1);
        }
    }
}
