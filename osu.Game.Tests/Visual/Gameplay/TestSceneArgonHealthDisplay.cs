// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play.HUD;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneArgonHealthDisplay : OsuTestScene
    {
        [Cached(typeof(HealthProcessor))]
        private HealthProcessor healthProcessor = new DrainingHealthProcessor(0);

        private ArgonHealthDisplay healthDisplay = null!;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep(@"Reset all", delegate
            {
                healthProcessor.Health.Value = 1;
                healthProcessor.Failed += () => false; // health won't be updated if the processor gets into a "fail" state.

                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Gray,
                    },
                    healthDisplay = new ArgonHealthDisplay
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
                };
            });

            AddSliderStep("Width", 0, 1f, 1f, val =>
            {
                if (healthDisplay.IsNotNull())
                    healthDisplay.BarLength.Value = val;
            });

            AddSliderStep("Height", 0, 64, 0, val =>
            {
                if (healthDisplay.IsNotNull())
                    healthDisplay.BarHeight.Value = val;
            });
        }

        [Test]
        public void TestHealthDisplayIncrementing()
        {
            AddRepeatStep("apply miss judgement", delegate
            {
                healthProcessor.ApplyResult(new JudgementResult(new HitObject(), new Judgement()) { Type = HitResult.Miss });
            }, 5);

            AddRepeatStep(@"decrease hp slightly", delegate
            {
                healthProcessor.Health.Value -= 0.01f;
            }, 10);

            AddRepeatStep(@"increase hp without flash", delegate
            {
                healthProcessor.Health.Value += 0.1f;
            }, 3);

            AddRepeatStep(@"increase hp with flash", delegate
            {
                healthProcessor.Health.Value += 0.1f;
                healthProcessor.ApplyResult(new JudgementResult(new HitCircle(), new OsuJudgement())
                {
                    Type = HitResult.Perfect
                });
            }, 3);
        }
    }
}
