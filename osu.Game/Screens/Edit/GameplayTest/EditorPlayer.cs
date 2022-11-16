// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Overlays;
using osu.Game.Screens.Play;

namespace osu.Game.Screens.Edit.GameplayTest
{
    public class EditorPlayer : Player, IGameplaySettings
    {
        private readonly Editor editor;
        private readonly EditorState editorState;

        [Resolved]
        private MusicController musicController { get; set; } = null!;

        private OsuConfigManager config = null!;

        public EditorPlayer(Editor editor)
            : base(new PlayerConfiguration { ShowResults = false })
        {
            this.editor = editor;
            editorState = editor.GetState();
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            // needs to be populated before BDL to work correctly.
            config = parent.Get<OsuConfigManager>();

            return base.CreateChildDependencies(parent);
        }

        protected override GameplayClockContainer CreateGameplayClockContainer(WorkingBeatmap beatmap, double gameplayStart)
        {
            var masterGameplayClockContainer = new MasterGameplayClockContainer(beatmap, gameplayStart);
            masterGameplayClockContainer.Reset(editorState.Time);
            return masterGameplayClockContainer;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            ScoreProcessor.HasCompleted.BindValueChanged(completed =>
            {
                if (completed.NewValue)
                {
                    Scheduler.AddDelayed(() =>
                    {
                        if (this.IsCurrentScreen())
                            this.Exit();
                    }, RESULTS_DISPLAY_DELAY);
                }
            });
        }

        protected override void PrepareReplay()
        {
            // don't record replays.
        }

        protected override bool CheckModsAllowFailure() => false; // never fail.

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);

            // finish alpha transforms on entering to avoid gameplay starting in a half-hidden state.
            // the finish calls are purposefully not propagated to children to avoid messing up their state.
            FinishTransforms();
            GameplayClockContainer.FinishTransforms(false, nameof(Alpha));
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            musicController.Stop();

            editorState.Time = GameplayClockContainer.CurrentTime;
            editor.RestoreState(editorState);
            return base.OnExiting(e);
        }

        // Editor overrides but we actually want to use game-wide settings here.
        public IBindable<float> ComboColourNormalisationAmount => ((IGameplaySettings)config).ComboColourNormalisationAmount;
        public IBindable<float> PositionalHitsoundsLevel => ((IGameplaySettings)config).PositionalHitsoundsLevel;
    }
}
