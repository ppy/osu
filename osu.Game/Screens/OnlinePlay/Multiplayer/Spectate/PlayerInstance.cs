// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Users;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate
{
    public class PlayerInstance : CompositeDrawable
    {
        private const double catchup_rate = 2;
        private const double max_sync_offset = catchup_rate * 2; // Double the catchup rate to prevent ringing.

        public bool PlayerLoaded => stack?.CurrentScreen is Player;

        public User User => Score.ScoreInfo.User;

        public WorkingBeatmap Beatmap { get; private set; }
        public Ruleset Ruleset { get; private set; }

        public readonly Score Score;

        private OsuScreenStack stack;
        private MultiplayerSpectatorPlayer player;

        public PlayerInstance(Score score)
        {
            Score = score;

            RelativeSizeAxes = Axes.Both;
            Masking = true;
        }

        [BackgroundDependencyLoader]
        private void load(BeatmapManager beatmapManager)
        {
            Beatmap = beatmapManager.GetWorkingBeatmap(Score.ScoreInfo.Beatmap, bypassCache: true);
            Ruleset = Score.ScoreInfo.Ruleset.CreateInstance();

            InternalChild = new GameplayIsolationContainer(Beatmap, Score.ScoreInfo.Ruleset, Score.ScoreInfo.Mods)
            {
                RelativeSizeAxes = Axes.Both,
                Child = new DrawSizePreservingFillContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = stack = new OsuScreenStack()
                }
            };

            stack.Push(new MultiplayerSpectatorPlayerLoader(Score, () => player = new MultiplayerSpectatorPlayer(Score)));
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();
            updateCatchup();
        }

        private readonly BindableDouble catchupFrequencyAdjustment = new BindableDouble(catchup_rate);
        private double targetTrackTime;
        private bool isCatchingUp;

        private void updateCatchup()
        {
            if (player?.IsLoaded != true)
                return;

            if (Score.Replay.Frames.Count == 0)
                return;

            if (player.GameplayClockContainer.IsPaused.Value)
                return;

            double currentTime = Beatmap.Track.CurrentTime;
            bool catchupRequired = targetTrackTime > currentTime + max_sync_offset;

            // Skip catchup if nothing needs to be done.
            if (catchupRequired == isCatchingUp)
                return;

            if (catchupRequired)
            {
                Beatmap.Track.AddAdjustment(AdjustableProperty.Frequency, catchupFrequencyAdjustment);
                isCatchingUp = true;
            }
            else
            {
                Beatmap.Track.RemoveAdjustment(AdjustableProperty.Frequency, catchupFrequencyAdjustment);
                isCatchingUp = false;
            }
        }

        public double GetCurrentGameplayTime()
        {
            if (player?.IsLoaded != true)
                return 0;

            return player.GameplayClockContainer.GameplayClock.CurrentTime;
        }

        public double GetCurrentTrackTime()
        {
            if (player?.IsLoaded != true)
                return 0;

            return Beatmap.Track.CurrentTime;
        }

        public void ContinueGameplay(double targetTrackTime)
        {
            if (player?.IsLoaded != true)
                return;

            player.GameplayClockContainer.Start();
            this.targetTrackTime = targetTrackTime;
        }

        public void PauseGameplay()
        {
            if (player?.IsLoaded != true)
                return;

            player.GameplayClockContainer.Stop();
        }
    }
}
