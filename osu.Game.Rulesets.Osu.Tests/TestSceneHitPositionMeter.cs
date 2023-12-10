// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play.HUD.HitErrorMeters;
using System.Diagnostics;
using System;
using osu.Game.Rulesets.Osu.Skinning;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    public partial class TestSceneHitPositionMeter : OsuTestScene
    {
        private DependencyProvidingContainer dependencyContainer = null!;
        private ScoreProcessor scoreProcessor = null!;

        private HitPositionMeter hitPositionMeter = null!;

        [SetUpSteps]
        public void SetupSteps() => AddStep("Create components", () =>
        {
            var ruleset = new OsuRuleset();

            scoreProcessor = new ScoreProcessor(ruleset);
            Child = dependencyContainer = new DependencyProvidingContainer
            {
                RelativeSizeAxes = Axes.Both,
                CachedDependencies = new (Type, object)[]
                {
                    (typeof(ScoreProcessor), scoreProcessor)
                }
            };
            dependencyContainer.Child = hitPositionMeter = new HitPositionMeter
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
    }
}
