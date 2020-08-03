// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Tests
{
    public class TestSceneComboCounter : CatchSkinnableTestScene
    {
        private ScoreProcessor scoreProcessor;
        private GameplayBeatmap gameplayBeatmap;
        private readonly Bindable<bool> isBreakTime = new BindableBool();

        [BackgroundDependencyLoader]
        private void load()
        {
            gameplayBeatmap = new GameplayBeatmap(CreateBeatmapForSkinProvider());
            gameplayBeatmap.IsBreakTime.BindTo(isBreakTime);
            Dependencies.Cache(gameplayBeatmap);
            Add(gameplayBeatmap);
        }

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            scoreProcessor = new ScoreProcessor();

            SetContents(() => new CatchComboDisplay
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Scale = new Vector2(2.5f),
            });
        });

        [Test]
        public void TestCatchComboCounter()
        {
            AddRepeatStep("perform hit", () => performJudgement(HitResult.Perfect), 20);
            AddStep("perform miss", () => performJudgement(HitResult.Miss));
            AddToggleStep("toggle gameplay break", v => isBreakTime.Value = v);
        }

        private void performJudgement(HitResult type, Judgement judgement = null)
        {
            var judgedObject = new TestDrawableCatchHitObject(new TestCatchHitObject());
            var result = new JudgementResult(judgedObject.HitObject, judgement ?? new Judgement()) { Type = type };
            scoreProcessor.ApplyResult(result);

            foreach (var counter in CreatedDrawables.Cast<CatchComboDisplay>())
                counter.OnNewResult(judgedObject, result);
        }

        private class TestDrawableCatchHitObject : DrawableCatchHitObject
        {
            public TestDrawableCatchHitObject(CatchHitObject hitObject)
                : base(hitObject)
            {
                AccentColour.Value = Color4.White;
            }
        }

        private class TestCatchHitObject : CatchHitObject
        {
        }
    }
}
