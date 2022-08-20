// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
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
        private readonly Bindable<bool> waitingOnFrames = new Bindable<bool>(true);
        private readonly ISpectatorPlayerClock spectatorPlayerClock;

        /// <summary>
        /// Creates a new <see cref="MultiSpectatorPlayer"/>.
        /// </summary>
        /// <param name="score">The score containing the player's replay.</param>
        /// <param name="spectatorPlayerClock">The clock controlling the gameplay running state.</param>
        public MultiSpectatorPlayer(Score score, ISpectatorPlayerClock spectatorPlayerClock)
            : base(score, new PlayerConfiguration { AllowUserInteraction = false })
        {
            this.spectatorPlayerClock = spectatorPlayerClock;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            spectatorPlayerClock.WaitingOnFrames.BindTo(waitingOnFrames);

            HUDOverlay.PlayerSettingsOverlay.Expire();
            HUDOverlay.HoldToQuit.Expire();
        }

        protected override void Update()
        {
            // The player clock's running state is controlled externally, but the local pausing state needs to be updated to start/stop gameplay.
            CatchUpSpectatorPlayerClock catchUpClock = (CatchUpSpectatorPlayerClock)GameplayClockContainer.SourceClock;

            if (catchUpClock.IsRunning)
                GameplayClockContainer.Start();
            else
                GameplayClockContainer.Stop();

            base.Update();
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            // This is required because the frame stable clock is set to WaitingOnFrames = false for one frame.
            waitingOnFrames.Value = DrawableRuleset.FrameStableClock.WaitingOnFrames.Value || Score.Replay.Frames.Count == 0;
        }

        protected override GameplayClockContainer CreateGameplayClockContainer(WorkingBeatmap beatmap, double gameplayStart)
            => new GameplayClockContainer(spectatorPlayerClock);
    }
}
