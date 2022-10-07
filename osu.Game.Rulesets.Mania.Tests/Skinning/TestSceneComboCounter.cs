// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play.HUD;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mania.Tests.Skinning
{
    public class TestSceneComboCounter : ManiaSkinnableTestScene
    {
        [Cached]
        private ScoreProcessor scoreProcessor = new ScoreProcessor(new ManiaRuleset());

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("setup", () => SetContents(_ => new SkinnableDrawable(new ManiaSkinComponent(ManiaSkinComponents.ComboCounter),
                _ => new DefaultComboCounter())
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Width = 0.5f,
            }));

            AddRepeatStep("perform hit", () => scoreProcessor.ApplyResult(new JudgementResult(new HitObject(), new Judgement()) { Type = HitResult.Great }), 20);
            AddStep("perform miss", () => scoreProcessor.ApplyResult(new JudgementResult(new HitObject(), new Judgement()) { Type = HitResult.Miss }));
        }
    }
}
