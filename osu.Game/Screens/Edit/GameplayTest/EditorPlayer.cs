// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Screens.Play;

namespace osu.Game.Screens.Edit.GameplayTest
{
    public class EditorPlayer : Player
    {
        private readonly Editor editor;
        private readonly EditorState editorState;

        [Resolved]
        private MusicController musicController { get; set; }

        public EditorPlayer(Editor editor)
            : base(new PlayerConfiguration { ShowResults = false })
        {
            this.editor = editor;
            editorState = editor.GetState();
        }

        protected override GameplayClockContainer CreateGameplayClockContainer(WorkingBeatmap beatmap, double gameplayStart)
            => new MasterGameplayClockContainer(beatmap, editorState.Time, true);

        protected override void LoadComplete()
        {
            base.LoadComplete();
            ScoreProcessor.HasCompleted.BindValueChanged(completed =>
            {
                if (completed.NewValue)
                    Scheduler.AddDelayed(this.Exit, RESULTS_DISPLAY_DELAY);
            });
        }

        protected override void PrepareReplay()
        {
            // don't record replays.
        }

        protected override bool CheckModsAllowFailure() => false; // never fail.

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);

            // finish alpha transforms on entering to avoid gameplay starting in a half-hidden state.
            // the finish calls are purposefully not propagated to children to avoid messing up their state.
            FinishTransforms();
            GameplayClockContainer.FinishTransforms(false, nameof(Alpha));
        }

        public override bool OnExiting(IScreen next)
        {
            musicController.Stop();

            editorState.Time = GameplayClockContainer.CurrentTime;
            editor.RestoreState(editorState);
            return base.OnExiting(next);
        }
    }
}
