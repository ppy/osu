// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play.HUD.HitErrorMeters;
using osuTK;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneColourHitErrorMeter : OsuTestScene
    {
        private DependencyProvidingContainer dependencyContainer = null!;

        private readonly Bindable<Judgement> lastJudgementResult = new Bindable<Judgement>();
        private ScoreProcessor scoreProcessor = null!;

        private int iteration;

        private ColourHitErrorMeter colourHitErrorMeter = null!;

        public TestSceneColourHitErrorMeter()
        {
            AddSliderStep("Judgement spacing", 0, 10, 2, spacing =>
            {
                if (colourHitErrorMeter.IsNotNull())
                    colourHitErrorMeter.JudgementSpacing.Value = spacing;
            });

            AddSliderStep("Judgement count", 1, 50, 5, spacing =>
            {
                if (colourHitErrorMeter.IsNotNull())
                    colourHitErrorMeter.JudgementCount.Value = spacing;
            });
        }

        [SetUpSteps]
        public void SetupSteps() => AddStep("Create components", () =>
        {
            var ruleset = CreateRuleset();

            Debug.Assert(ruleset != null);

            scoreProcessor = new ScoreProcessor(ruleset);
            Child = dependencyContainer = new DependencyProvidingContainer
            {
                RelativeSizeAxes = Axes.Both,
                CachedDependencies = new (Type, object)[]
                {
                    (typeof(ScoreProcessor), scoreProcessor)
                }
            };
            dependencyContainer.Child = colourHitErrorMeter = new ColourHitErrorMeter
            {
                Margin = new MarginPadding
                {
                    Top = 100
                },
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                Scale = new Vector2(2),
            };
        });

        protected override Ruleset CreateRuleset() => new OsuRuleset();

        [Test]
        public void TestSpacingChange()
        {
            AddRepeatStep("Add judgement", applyOneJudgement, 5);
            AddStep("Change spacing", () => colourHitErrorMeter.JudgementSpacing.Value = 10);
            AddRepeatStep("Add judgement", applyOneJudgement, 5);
        }

        [Test]
        public void TestJudgementAmountChange()
        {
            AddRepeatStep("Add judgement", applyOneJudgement, 10);
            AddStep("Judgement count change to 4", () => colourHitErrorMeter.JudgementCount.Value = 4);
            AddRepeatStep("Add judgement", applyOneJudgement, 8);
        }

        [Test]
        public void TestHitErrorShapeChange()
        {
            AddRepeatStep("Add judgement", applyOneJudgement, 8);
            AddStep("Change shape square", () => colourHitErrorMeter.JudgementShape.Value = ColourHitErrorMeter.ShapeStyle.Square);
            AddRepeatStep("Add judgement", applyOneJudgement, 10);
            AddStep("Change shape circle", () => colourHitErrorMeter.JudgementShape.Value = ColourHitErrorMeter.ShapeStyle.Circle);
        }

        private void applyOneJudgement()
        {
            lastJudgementResult.Value = new OsuJudgement(new HitObject
            {
                StartTime = iteration * 10000,
            }, new OsuJudgementCriteria())
            {
                Type = HitResult.Great,
            };
            scoreProcessor.ApplyResult(lastJudgementResult.Value);

            iteration++;
        }
    }
}
