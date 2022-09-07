// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Scoring;
using osu.Game.Screens.Play;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate
{
    /// <summary>
    /// A single spectated player within a <see cref="MultiSpectatorScreen"/>.
    /// </summary>
    public class MultiSpectatorPlayer : SpectatorPlayer
    {
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
        private void load()
        {
            HUDOverlay.PlayerSettingsOverlay.Expire();
            HUDOverlay.HoldToQuit.Expire();
        }

        protected override void Update()
        {
            // The player clock's running state is controlled externally, but the local pausing state needs to be updated to start/stop gameplay.
            if (GameplayClockContainer.SourceClock.IsRunning)
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
            var gameplayClockContainer = new GameplayClockContainer(spectatorPlayerClock);

            // Directionality is important, as BindAdjustments is... not actually a bidirectional bind...
            // We want to ensure that any adjustments applied by the Player instance are applied to the SpectatorPlayerClock
            // so they can be consumed by the spectator screen (and applied to the master clock / track).
            spectatorPlayerClock.GameplayAdjustments.BindAdjustments(gameplayClockContainer.GameplayAdjustments);

            return gameplayClockContainer;
        }
    }
}
