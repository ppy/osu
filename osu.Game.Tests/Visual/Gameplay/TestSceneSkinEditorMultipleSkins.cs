// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Skinning.Editor;
using osu.Game.Tests.Beatmaps;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneSkinEditorMultipleSkins : SkinnableTestScene
    {
        [Cached]
        private readonly ScoreProcessor scoreProcessor = new ScoreProcessor(new OsuRuleset());

        [Cached(typeof(HealthProcessor))]
        private HealthProcessor healthProcessor = new DrainingHealthProcessor(0);

        [Cached]
        private GameplayState gameplayState = new GameplayState(new TestBeatmap(new OsuRuleset().RulesetInfo), new OsuRuleset());

        [Cached]
        private readonly GameplayClock gameplayClock = new GameplayClock(new FramedClock());

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create editor overlay", () =>
            {
                SetContents(_ =>
                {
                    var ruleset = new OsuRuleset();
                    var mods = new[] { ruleset.GetAutoplayMod() };
                    var working = CreateWorkingBeatmap(ruleset.RulesetInfo);
                    var beatmap = working.GetPlayableBeatmap(ruleset.RulesetInfo, mods);

                    var drawableRuleset = ruleset.CreateDrawableRulesetWith(beatmap, mods);

                    var hudOverlay = new HUDOverlay(drawableRuleset, mods)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    };

                    // Add any key just to display the key counter visually.
                    hudOverlay.KeyCounter.Add(new KeyCounterKeyboard(Key.Space));
                    scoreProcessor.Combo.Value = 1;

                    return new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            drawableRuleset,
                            hudOverlay,
                            new SkinEditor(hudOverlay),
                        }
                    };
                });
            });
        }

        protected override Ruleset CreateRulesetForSkinProvider() => new OsuRuleset();
    }
}
