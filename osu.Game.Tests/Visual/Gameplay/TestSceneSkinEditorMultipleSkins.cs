// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Overlays.SkinEditor;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD;
using osu.Game.Tests.Gameplay;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneSkinEditorMultipleSkins : SkinnableTestScene
    {
        [Cached(typeof(ScoreProcessor))]
        private ScoreProcessor scoreProcessor { get; set; }

        [Cached(typeof(HealthProcessor))]
        private HealthProcessor healthProcessor = new DrainingHealthProcessor(0);

        [Cached]
        private GameplayState gameplayState = TestGameplayState.Create(new OsuRuleset());

        [Cached(typeof(IGameplayClock))]
        private readonly IGameplayClock gameplayClock = new GameplayClockContainer(new TrackVirtual(60000), false, false);

        [Cached]
        public readonly EditorClipboard Clipboard = new EditorClipboard();

        public TestSceneSkinEditorMultipleSkins()
        {
            scoreProcessor = gameplayState.ScoreProcessor;
        }

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
                    hudOverlay.InputCountController.Add(new KeyCounterKeyboardTrigger(Key.Space));
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
