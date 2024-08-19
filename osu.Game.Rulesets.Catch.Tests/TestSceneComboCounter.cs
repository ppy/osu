// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.Skinning.Legacy;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Skinning;
using osu.Game.Tests.Gameplay;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Catch.Tests
{
    public partial class TestSceneComboCounter : CatchSkinnableTestScene
    {
        private readonly GameplayState gameplayState = TestGameplayState.Create(new CatchRuleset());

        [Cached(typeof(ScoreProcessor))]
        private ScoreProcessor scoreProcessor;

        private Color4 judgedObjectColour = Color4.White;

        public TestSceneComboCounter()
        {
            scoreProcessor = gameplayState.ScoreProcessor;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            scoreProcessor.NewJudgement += result => gameplayState.ApplyResult(result);

            Dependencies.CacheAs(gameplayState);
        }

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            SetContents(skin =>
            {
                if (skin is LegacySkin)
                {
                    return new LegacyCatchComboCounter
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    };
                }

                return Empty();
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

        private void performJudgement(HitResult type, Judgement? judgement = null)
        {
            var judgedObject = new DrawableFruit(new ColourfulFruit(judgedObjectColour));

            var result = new JudgementResult(judgedObject.HitObject, judgement ?? new Judgement()) { Type = type };
            scoreProcessor.ApplyResult(result);
        }

        private class ColourfulFruit : Fruit, IHasComboInformation
        {
            private readonly Color4 colour;

            public ColourfulFruit(Color4 colour)
            {
                this.colour = colour;
            }

            Color4 IHasComboInformation.GetComboColour(ISkin skin) => colour;
        }
    }
}
