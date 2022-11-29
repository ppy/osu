// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Spectator;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Replays.Types;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking;

namespace osu.Game.Screens.Play
{
    public abstract partial class SpectatorPlayer : Player
    {
        [Resolved]
        protected SpectatorClient SpectatorClient { get; private set; }

        private readonly Score score;

        protected override bool CheckModsAllowFailure() => false; // todo: better support starting mid-way through beatmap

        protected SpectatorPlayer(Score score, PlayerConfiguration configuration = null)
            : base(configuration)
        {
            this.score = score;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(new OsuSpriteText
            {
                Text = $"Watching {score.ScoreInfo.User.Username} playing live!",
                Font = OsuFont.Default.With(size: 30),
                Y = 100,
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            DrawableRuleset.FrameStableClock.WaitingOnFrames.BindValueChanged(waiting =>
            {
                if (GameplayClockContainer is MasterGameplayClockContainer master)
                {
                    if (master.UserPlaybackRate.Value > 1 && waiting.NewValue)
                        master.UserPlaybackRate.Value = 1;
                }
            }, true);
        }

        protected override void StartGameplay()
        {
            base.StartGameplay();

            // Start gameplay along with the very first arrival frame (the latest one).
            score.Replay.Frames.Clear();
            SpectatorClient.OnNewFrames += userSentFrames;
        }

        private void userSentFrames(int userId, FrameDataBundle bundle)
        {
            if (userId != score.ScoreInfo.User.OnlineID)
                return;

            if (!LoadedBeatmapSuccessfully)
                return;

            if (!this.IsCurrentScreen())
                return;

            bool isFirstBundle = score.Replay.Frames.Count == 0;

            foreach (var frame in bundle.Frames)
            {
                IConvertibleReplayFrame convertibleFrame = GameplayState.Ruleset.CreateConvertibleReplayFrame();
                Debug.Assert(convertibleFrame != null);
                convertibleFrame.FromLegacy(frame, GameplayState.Beatmap);

                var convertedFrame = (ReplayFrame)convertibleFrame;
                convertedFrame.Time = frame.Time;
                convertedFrame.Header = frame.Header;

                score.Replay.Frames.Add(convertedFrame);
            }

            if (isFirstBundle && score.Replay.Frames.Count > 0)
                SetGameplayStartTime(score.Replay.Frames[0].Time);
        }

        protected override Score CreateScore(IBeatmap beatmap) => score;

        protected override ResultsScreen CreateResults(ScoreInfo score)
            => new SpectatorResultsScreen(score);

        protected override void PrepareReplay()
        {
            DrawableRuleset?.SetReplayScore(score);
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            SpectatorClient.OnNewFrames -= userSentFrames;

            return base.OnExiting(e);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (SpectatorClient != null)
                SpectatorClient.OnNewFrames -= userSentFrames;
        }
    }
}
