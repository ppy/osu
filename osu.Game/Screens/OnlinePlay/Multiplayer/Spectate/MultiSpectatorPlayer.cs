// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Game.Beatmaps;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Ranking;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate
{
    /// <summary>
    /// A single spectated player within a <see cref="MultiSpectatorScreen"/>.
    /// </summary>
    public partial class MultiSpectatorPlayer : SpectatorPlayer
    {
        /// <summary>
        /// All adjustments applied to the clock of this <see cref="MultiSpectatorPlayer"/> which come from mods.
        /// </summary>
        public IAggregateAudioAdjustment ClockAdjustmentsFromMods => clockAdjustmentsFromMods;

        private readonly AudioAdjustments clockAdjustmentsFromMods = new AudioAdjustments();
        private readonly SpectatorPlayerClock spectatorPlayerClock;

        /// <summary>
        /// Creates a new <see cref="MultiSpectatorPlayer"/>.
        /// </summary>
        /// <param name="score">The score containing the player's replay.</param>
        /// <param name="spectatorPlayerClock">The clock controlling the gameplay running state.</param>
        public MultiSpectatorPlayer(Score score, SpectatorPlayerClock spectatorPlayerClock)
            : base(score, new PlayerConfiguration { AllowUserInteraction = false })
        {
            this.spectatorPlayerClock = spectatorPlayerClock;
        }

        [BackgroundDependencyLoader]
        private void load(CancellationToken cancellationToken)
        {
            // HUD overlay may not be loaded if load has been cancelled early.
            if (cancellationToken.IsCancellationRequested)
                return;

            HUDOverlay.PlayerSettingsOverlay.Expire();
            HUDOverlay.HoldToQuit.Expire();
        }

        protected override void Update()
        {
            // The player clock's running state is controlled externally, but the local pausing state needs to be updated to start/stop gameplay.
            if (GameplayClockContainer.IsRunning)
                GameplayClockContainer.Start();
            else
                GameplayClockContainer.Stop();

            base.Update();
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            // This is required because the frame stable clock is set to WaitingOnFrames = false for one frame.
            spectatorPlayerClock.WaitingOnFrames = DrawableRuleset.FrameStableClock.WaitingOnFrames.Value || Score.Replay.Frames.Count == 0;
        }

        protected override GameplayClockContainer CreateGameplayClockContainer(WorkingBeatmap beatmap, double gameplayStart)
        {
            // Importantly, we don't want to apply decoupling because SpectatorPlayerClock updates its IsRunning directly.
            // If we applied decoupling, this state change wouldn't actually cause the clock to stop.
            // TODO: Can we just use Start/Stop rather than this workaround, now that DecouplingClock is more sane?
            var gameplayClockContainer = new GameplayClockContainer(spectatorPlayerClock, applyOffsets: false, requireDecoupling: false);
            clockAdjustmentsFromMods.BindAdjustments(gameplayClockContainer.AdjustmentsFromMods);
            return gameplayClockContainer;
        }

        protected override ResultsScreen CreateResults(ScoreInfo score) => new MultiSpectatorResultsScreen(score);
    }
}
