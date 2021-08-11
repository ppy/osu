// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Timing;
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
        public MultiSpectatorPlayer([NotNull] Score score, [NotNull] ISpectatorPlayerClock spectatorPlayerClock)
            : base(score)
        {
            this.spectatorPlayerClock = spectatorPlayerClock;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            spectatorPlayerClock.WaitingOnFrames.BindTo(waitingOnFrames);
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            // This is required because the frame stable clock is set to WaitingOnFrames = false for one frame.
            waitingOnFrames.Value = DrawableRuleset.FrameStableClock.WaitingOnFrames.Value || Score.Replay.Frames.Count == 0;
        }

        protected override GameplayClockContainer CreateGameplayClockContainer(WorkingBeatmap beatmap, double gameplayStart)
            => new SpectatorGameplayClockContainer(spectatorPlayerClock);

        private class SpectatorGameplayClockContainer : GameplayClockContainer
        {
            public SpectatorGameplayClockContainer([NotNull] IClock sourceClock)
                : base(sourceClock)
            {
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                updateContainerState();
            }

            protected override void Update()
            {
                updateContainerState();
                base.Update();
            }

            protected override GameplayClock CreateGameplayClock(IFrameBasedClock source) => new GameplayClock(source);

            private void updateContainerState()
            {
                // The player clock's running state is controlled externally, but the local pausing state needs to be updated to stop gameplay.
                if (SourceClock.IsRunning)
                    Start();
                else
                    Stop();
            }
        }
    }
}
