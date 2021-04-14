// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Users;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate
{
    public class PlayerInstance : CompositeDrawable
    {
        /// <summary>
        /// The rate at which a user catches up after becoming desynchronised.
        /// </summary>
        private const double catchup_rate = 2;

        /// <summary>
        /// The offset from the expected time at which to START synchronisation.
        /// </summary>
        public const double MAX_OFFSET = 50;

        /// <summary>
        /// The maximum offset from the expected time at which to STOP synchronisation.
        /// </summary>
        public const double SYNC_TARGET = 16;

        public bool PlayerLoaded => stack?.CurrentScreen is Player;

        public User User => Score.ScoreInfo.User;

        public WorkingBeatmap Beatmap { get; private set; }

        public readonly Score Score;
        private readonly GameplayClock gameplayClock;

        public bool IsCatchingUp { get; private set; }

        private OsuScreenStack stack;
        private MultiplayerSpectatorPlayer player;

        public PlayerInstance(Score score, GameplayClock gameplayClock)
        {
            Score = score;
            this.gameplayClock = gameplayClock;

            RelativeSizeAxes = Axes.Both;
            Masking = true;
        }

        [BackgroundDependencyLoader]
        private void load(BeatmapManager beatmapManager)
        {
            Beatmap = beatmapManager.GetWorkingBeatmap(Score.ScoreInfo.Beatmap, bypassCache: true);

            InternalChild = new GameplayIsolationContainer(Beatmap, Score.ScoreInfo.Ruleset, Score.ScoreInfo.Mods)
            {
                RelativeSizeAxes = Axes.Both,
                Child = new DrawSizePreservingFillContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = stack = new OsuScreenStack()
                }
            };

            stack.Push(new MultiplayerSpectatorPlayerLoader(Score, () => player = new MultiplayerSpectatorPlayer(Score, gameplayClock)));
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();
            updateCatchup();
        }

        private double targetGameplayTime;

        private void updateCatchup()
        {
            if (player?.IsLoaded != true)
                return;

            if (Score.Replay.Frames.Count == 0)
                return;

            if (player.GameplayClockContainer.IsPaused.Value)
                return;

            double currentTime = Beatmap.Track.CurrentTime;
            double timeBehind = targetGameplayTime - currentTime;

            double offsetForCatchup = IsCatchingUp ? SYNC_TARGET : MAX_OFFSET;
            bool catchupRequired = timeBehind > offsetForCatchup;

            // Skip catchup if no work needs to be done.
            if (catchupRequired == IsCatchingUp)
                return;

            if (catchupRequired)
            {
                // player.GameplayClockContainer.AdjustableClock.Rate = catchup_rate;
                Logger.Log($"{User.Id} catchup started (behind: {(int)timeBehind})");
            }
            else
            {
                // player.GameplayClockContainer.AdjustableClock.Rate = 1;
                Logger.Log($"{User.Id} catchup finished (behind: {(int)timeBehind})");
            }

            IsCatchingUp = catchupRequired;
        }

        public double GetCurrentGameplayTime()
        {
            if (player?.IsLoaded != true)
                return 0;

            return player.GameplayClockContainer.GameplayClock.CurrentTime;
        }

        public bool IsPlaying()
        {
            if (player.IsLoaded != true)
                return false;

            return player.GameplayClockContainer.GameplayClock.IsRunning;
        }

        public void ContinueGameplay(double targetGameplayTime)
        {
            if (player?.IsLoaded != true)
                return;

            player.GameplayClockContainer.Start();
            this.targetGameplayTime = targetGameplayTime;
        }

        public void PauseGameplay()
        {
            if (player?.IsLoaded != true)
                return;

            player.GameplayClockContainer.Stop();
        }

        // Player interferes with global input, so disable input for now.
        public override bool PropagatePositionalInputSubTree => false;
        public override bool PropagateNonPositionalInputSubTree => false;
    }
}
