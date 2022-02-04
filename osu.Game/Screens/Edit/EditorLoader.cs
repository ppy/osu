// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Play;

namespace osu.Game.Screens.Edit
{
    /// <summary>
    /// Transition screen for the editor.
    /// Used to avoid backing out to main menu/song select when switching difficulties from within the editor.
    /// </summary>
    public class EditorLoader : ScreenWithBeatmapBackground
    {
        /// <summary>
        /// The stored state from the last editor opened.
        /// This will be read by the next editor instance to be opened to restore any relevant previous state.
        /// </summary>
        [CanBeNull]
        private EditorState state;

        public override float BackgroundParallaxAmount => 0.1f;

        public override bool AllowBackButton => false;

        public override bool HideOverlaysOnEnter => true;

        public override bool DisallowExternalBeatmapRulesetChanges => true;

        [Resolved]
        private BeatmapManager beatmapManager { get; set; }

        [CanBeNull]
        private ScheduledDelegate scheduledDifficultySwitch;

        [BackgroundDependencyLoader]
        private void load()
        {
            AddRangeInternal(new Drawable[]
            {
                new LoadingSpinner(true)
                {
                    State = { Value = Visibility.Visible },
                }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // will be restored via lease, see `DisallowExternalBeatmapRulesetChanges`.
            Mods.Value = Array.Empty<Mod>();
        }

        protected virtual Editor CreateEditor() => new Editor(this);

        protected override void LogoArriving(OsuLogo logo, bool resuming)
        {
            base.LogoArriving(logo, resuming);

            if (!resuming)
            {
                // the push cannot happen in OnEntering() or similar (even if scheduled), because the transition from main menu will look bad.
                // that is because this screen pushing the editor makes it no longer current, and OsuScreen checks if the screen is current
                // before enqueueing this screen's LogoArriving onto the logo animation sequence.
                pushEditor();
            }
        }

        public void ScheduleSwitchToNewDifficulty(BeatmapSetInfo beatmapSetInfo, RulesetInfo rulesetInfo, EditorState editorState)
            => scheduleDifficultySwitch(() =>
            {
                try
                {
                    return beatmapManager.CreateNewBlankDifficulty(beatmapSetInfo, rulesetInfo);
                }
                catch (Exception ex)
                {
                    // if the beatmap creation fails (e.g. due to duplicated difficulty names),
                    // bring the user back to the previous beatmap as a best-effort.
                    Logger.Error(ex, ex.Message);
                    return Beatmap.Value;
                }
            }, editorState);

        public void ScheduleSwitchToExistingDifficulty(BeatmapInfo beatmapInfo, EditorState editorState)
            => scheduleDifficultySwitch(() => beatmapManager.GetWorkingBeatmap(beatmapInfo), editorState);

        private void scheduleDifficultySwitch(Func<WorkingBeatmap> nextBeatmap, EditorState editorState)
        {
            scheduledDifficultySwitch?.Cancel();
            ValidForResume = true;

            this.MakeCurrent();

            scheduledDifficultySwitch = Schedule(() =>
            {
                Beatmap.Value = nextBeatmap.Invoke();
                state = editorState;

                // This screen is a weird exception to the rule that nothing after song select changes the global beatmap.
                // Because of this, we need to update the background stack's beatmap to match.
                // If we don't do this, the editor will see a discrepancy and create a new background, along with an unnecessary transition.
                ApplyToBackground(b => b.Beatmap = Beatmap.Value);

                pushEditor();
            });
        }

        private void pushEditor()
        {
            var editor = CreateEditor();

            this.Push(editor);

            if (state != null)
                editor.RestoreState(state);

            ValidForResume = false;
        }

        public void CancelPendingDifficultySwitch()
        {
            scheduledDifficultySwitch?.Cancel();
            ValidForResume = false;
        }
    }
}
