// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Play;
using osu.Game.Users;

namespace osu.Game.Screens.Edit.GameplayTest
{
    public partial class EditorPlayer : Player, IKeyBindingHandler<GlobalAction>
    {
        private readonly Editor editor;
        private readonly EditorState editorState;

        protected override UserActivity InitialActivity => new UserActivity.TestingBeatmap(Beatmap.Value.BeatmapInfo);

        [Resolved]
        private MusicController musicController { get; set; } = null!;

        public EditorPlayer(Editor editor)
            : base(new PlayerConfiguration { ShowResults = false })
        {
            this.editor = editor;
            editorState = editor.GetState();
        }

        protected override GameplayClockContainer CreateGameplayClockContainer(WorkingBeatmap beatmap, double gameplayStart)
        {
            var masterGameplayClockContainer = new MasterGameplayClockContainer(beatmap, gameplayStart);

            // Only reset the time to the current point if the editor is later than the normal start time (and the first object).
            // This allows more sane test playing from the start of the beatmap (ie. correctly adding lead-in time).
            if (editorState.Time > gameplayStart && editorState.Time > DrawableRuleset.Objects.FirstOrDefault()?.StartTime)
                masterGameplayClockContainer.Reset(editorState.Time);

            return masterGameplayClockContainer;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            markPreviousObjectsHit();
            markVisibleDrawableObjectsHit();

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

        private void markPreviousObjectsHit()
        {
            foreach (var hitObject in enumerateHitObjects(DrawableRuleset.Objects, editorState.Time))
            {
                var judgement = hitObject.Judgement;
                var result = new JudgementResult(hitObject, judgement)
                {
                    Type = judgement.MaxResult,
                    GameplayRate = GameplayClockContainer.GetTrueGameplayRate(),
                };

                HealthProcessor.ApplyResult(result);
                ScoreProcessor.ApplyResult(result);
            }

            static IEnumerable<HitObject> enumerateHitObjects(IEnumerable<HitObject> hitObjects, double cutoffTime)
            {
                foreach (var hitObject in hitObjects)
                {
                    foreach (var nested in enumerateHitObjects(hitObject.NestedHitObjects, cutoffTime))
                    {
                        if (nested.GetEndTime() < cutoffTime)
                            yield return nested;
                    }

                    if (hitObject.GetEndTime() < cutoffTime)
                        yield return hitObject;
                }
            }
        }

        private void markVisibleDrawableObjectsHit()
        {
            if (!DrawableRuleset.Playfield.IsLoaded)
            {
                Schedule(markVisibleDrawableObjectsHit);
                return;
            }

            foreach (var drawableObjectEntry in enumerateDrawableEntries(
                         DrawableRuleset.Playfield.AllHitObjects
                                        .Select(ho => ho.Entry)
                                        .Where(e => e != null)
                                        .Cast<HitObjectLifetimeEntry>(), editorState.Time))
            {
                drawableObjectEntry.Result = new JudgementResult(drawableObjectEntry.HitObject, drawableObjectEntry.HitObject.Judgement)
                {
                    Type = drawableObjectEntry.HitObject.Judgement.MaxResult
                };
            }

            static IEnumerable<HitObjectLifetimeEntry> enumerateDrawableEntries(IEnumerable<HitObjectLifetimeEntry> entries, double cutoffTime)
            {
                foreach (var entry in entries)
                {
                    foreach (var nested in enumerateDrawableEntries(entry.NestedEntries, cutoffTime))
                    {
                        if (nested.HitObject.GetEndTime() < cutoffTime)
                            yield return nested;
                    }

                    if (entry.HitObject.GetEndTime() < cutoffTime)
                        yield return entry;
                }
            }
        }

        protected override void PrepareReplay()
        {
            // don't record replays.
        }

        protected override bool CheckModsAllowFailure() => false; // never fail.

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Repeat)
                return false;

            switch (e.Action)
            {
                case GlobalAction.EditorTestPlayToggleAutoplay:
                    toggleAutoplay();
                    return true;

                case GlobalAction.EditorTestPlayToggleQuickPause:
                    toggleQuickPause();
                    return true;

                case GlobalAction.EditorTestPlayQuickExitToInitialTime:
                    quickExit(false);
                    return true;

                case GlobalAction.EditorTestPlayQuickExitToCurrentTime:
                    quickExit(true);
                    return true;

                default:
                    return false;
            }
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        private void toggleAutoplay()
        {
            if (DrawableRuleset.ReplayScore == null)
            {
                var autoplay = Ruleset.Value.CreateInstance().GetAutoplayMod();
                if (autoplay == null)
                    return;

                var score = autoplay.CreateScoreFromReplayData(GameplayState.Beatmap, [autoplay]);

                // remove past frames to prevent replay frame handler from seeking back to start in an attempt to play back the entirety of the replay.
                score.Replay.Frames.RemoveAll(f => f.Time <= GameplayClockContainer.CurrentTime);

                DrawableRuleset.SetReplayScore(score);
                // Without this schedule, the `GlobalCursorDisplay.Update()` machinery will fade the gameplay cursor out, but we still want it to show.
                Schedule(() => DrawableRuleset.Cursor?.Show());
            }
            else
                DrawableRuleset.SetReplayScore(null);
        }

        private void toggleQuickPause()
        {
            if (GameplayClockContainer.IsPaused.Value)
                GameplayClockContainer.Start();
            else
                GameplayClockContainer.Stop();
        }

        private void quickExit(bool useCurrentTime)
        {
            if (useCurrentTime)
                editorState.Time = GameplayClockContainer.CurrentTime;

            editor.RestoreState(editorState);
            this.Exit();
        }

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

            editor.RestoreState(editorState);
            return base.OnExiting(e);
        }
    }
}
