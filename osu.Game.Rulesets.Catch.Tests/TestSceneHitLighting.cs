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
    public class TestSceneHitLighting : CatchSkinnableTestScene
    {
        private RulesetInfo catchRuleset;
        private OsuConfigManager config;

        public TestSceneHitLighting()
        {
            AddToggleStep("toggle hit lighting", enabled => createCatcher(enabled));
            AddStep("catch fruit", () => catchFruit(new TestFruit()
            {
                X = this.ChildrenOfType<CatcherArea>().First().MovableCatcher.X
            }));
        }

        private void catchFruit(Fruit fruit)
        {
            this.ChildrenOfType<CatcherArea>().ForEach(area =>
            {
                DrawableFruit drawable = new DrawableFruit(fruit);
                area.Add(drawable);

                Schedule(() =>
                {
                    area.AttemptCatch(fruit);
                    area.OnResult(drawable, new JudgementResult(fruit, new CatchJudgement()) { Type = HitResult.Great });

                    drawable.Expire();
                });
            });
        }

        private void createCatcher(bool hitLighting)
        {
            config.Set(OsuSetting.HitLighting, hitLighting);
            SetContents(() => new CatchInputManager(catchRuleset)
            {
                RelativeSizeAxes = Axes.Both,
                Child = new TestCatcherArea()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.TopCentre,
                    CreateDrawableRepresentation = ((DrawableRuleset<CatchHitObject>)catchRuleset.CreateInstance().CreateDrawableRulesetWith(new CatchBeatmap())).CreateDrawableRepresentation
                },
            });
        }

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets, OsuConfigManager configManager)
        {
            catchRuleset = rulesets.GetRuleset(2);
            config = configManager;
        }

        public class TestFruit : Fruit
        {
            public TestFruit()
            {
                ApplyDefaultsToSelf(new ControlPointInfo(), new BeatmapDifficulty());
            }
        }

        private class TestCatcherArea : CatcherArea
        {
            public TestCatcherArea()
                : base(new BeatmapDifficulty())
            {
            }

            public new Catcher MovableCatcher => base.MovableCatcher;
        }
    }
}
