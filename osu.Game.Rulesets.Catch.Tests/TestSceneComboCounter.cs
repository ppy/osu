// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Tests
{
    public class TestSceneComboCounter : CatchSkinnableTestScene
    {
        private ScoreProcessor scoreProcessor;

        private Color4 judgedObjectColour = Color4.White;

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
            AddRepeatStep("perform hit", () => performJudgement(HitResult.Great), 20);
            AddStep("perform miss", () => performJudgement(HitResult.Miss));

            AddStep("randomize judged object colour", () =>
            {
                judgedObjectColour = new Color4(
                    RNG.NextSingle(1f),
                    RNG.NextSingle(1f),
                    RNG.NextSingle(1f),
                    1f
                );
            });
        }

        private void performJudgement(HitResult type, Judgement judgement = null)
        {
            var judgedObject = new DrawableFruit(new Fruit()) { AccentColour = { Value = judgedObjectColour } };

            var result = new JudgementResult(judgedObject.HitObject, judgement ?? new Judgement()) { Type = type };
            scoreProcessor.ApplyResult(result);

            foreach (var counter in CreatedDrawables.Cast<CatchComboDisplay>())
                counter.OnNewResult(judgedObject, result);
        }
    }
}
