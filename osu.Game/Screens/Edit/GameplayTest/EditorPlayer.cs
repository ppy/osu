// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Online.Spectator;
using osu.Game.Overlays;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Users;

namespace osu.Game.Screens.Edit.GameplayTest
{
    public partial class EditorPlayer : Player
    {
        private readonly Editor editor;
        private readonly EditorState editorState;
        private readonly IBeatmap playableBeatmap;

        protected override UserActivity InitialActivity => new UserActivity.TestingBeatmap(Beatmap.Value.BeatmapInfo, Ruleset.Value);

        [Resolved]
        private MusicController musicController { get; set; } = null!;

        public EditorPlayer(Editor editor, IBeatmap playableBeatmap)
            : base(new PlayerConfiguration { ShowResults = false })
        {
            this.editor = editor;
            editorState = editor.GetState();
            this.playableBeatmap = playableBeatmap;
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

            var frame = new ReplayFrame { Header = new FrameHeader(new ScoreInfo(), new ScoreProcessorStatistics()) };

            foreach (var hitObject in enumerateHitObjects(playableBeatmap.HitObjects.Where(h => h.StartTime < editorState.Time)))
            {
                var judgement = hitObject.CreateJudgement();
                frame.Header.Statistics.TryAdd(judgement.MaxResult, 0);
                frame.Header.Statistics[judgement.MaxResult]++;
            }

            HealthProcessor.ResetFromReplayFrame(frame);
            ScoreProcessor.ResetFromReplayFrame(frame);

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

            static IEnumerable<HitObject> enumerateHitObjects(IEnumerable<HitObject> hitObjects)
            {
                foreach (var hitObject in hitObjects)
                {
                    foreach (var nested in enumerateHitObjects(hitObject.NestedHitObjects))
                        yield return nested;

                    yield return hitObject;
                }
            }
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

            editor.RestoreState(editorState);
            return base.OnExiting(e);
        }
    }
}
