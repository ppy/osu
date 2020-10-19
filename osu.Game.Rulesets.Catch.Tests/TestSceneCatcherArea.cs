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
using osu.Game.Configuration;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Catch.Judgements;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    public class TestSceneCatcherArea : CatchSkinnableTestScene
    {
        private RulesetInfo catchRuleset;

        [Resolved]
        private OsuConfigManager config { get; set; }

        private Catcher catcher => this.ChildrenOfType<CatcherArea>().First().MovableCatcher;

        public TestSceneCatcherArea()
        {
            AddSliderStep<float>("CircleSize", 0, 8, 5, createCatcher);
            AddToggleStep("Hyperdash", t =>
                CreatedDrawables.OfType<CatchInputManager>().Select(i => i.Child)
                                .OfType<TestCatcherArea>().ForEach(c => c.ToggleHyperDash(t)));

            AddRepeatStep("catch fruit", () => catchFruit(new TestFruit(false)
            {
                X = catcher.X
            }), 20);
            AddRepeatStep("catch fruit last in combo", () => catchFruit(new TestFruit(false)
            {
                X = catcher.X,
                LastInCombo = true,
            }), 20);
            AddRepeatStep("catch kiai fruit", () => catchFruit(new TestFruit(true)
            {
                X = catcher.X
            }), 20);
            AddRepeatStep("miss fruit", () => catchFruit(new Fruit
            {
                X = catcher.X + 100,
                LastInCombo = true,
            }, true), 20);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestHitLighting(bool enable)
        {
            AddStep("create catcher", () => createCatcher(5));

            AddStep("toggle hit lighting", () => config.Set(OsuSetting.HitLighting, enable));
            AddStep("catch fruit", () => catchFruit(new TestFruit(false)
            {
                X = catcher.X
            }));
            AddStep("catch fruit last in combo", () => catchFruit(new TestFruit(false)
            {
                X = catcher.X,
                LastInCombo = true
            }));
            AddAssert("check hit explosion", () => catcher.ChildrenOfType<HitExplosion>().Any() == enable);
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
                    area.OnNewResult(drawable, new JudgementResult(fruit, new CatchJudgement()) { Type = miss ? HitResult.Miss : HitResult.Great });

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
                    Anchor = Anchor.Centre,
                    Origin = Anchor.TopCentre,
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
