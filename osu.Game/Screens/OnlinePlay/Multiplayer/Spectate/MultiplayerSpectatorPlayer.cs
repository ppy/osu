// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Play;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate
{
    public class MultiplayerSpectatorPlayer : SpectatorPlayer
    {
        public new ScoreProcessor ScoreProcessor => base.ScoreProcessor;

        public new SubGameplayClockContainer GameplayClockContainer => (SubGameplayClockContainer)base.GameplayClockContainer;

        private readonly GameplayClock gameplayClock;

        public MultiplayerSpectatorPlayer(Score score, GameplayClock gameplayClock)
            : base(score)
        {
            this.gameplayClock = gameplayClock;
        }

        protected override GameplayClockContainer CreateGameplayClockContainer(WorkingBeatmap beatmap, double gameplayStart)
            => new SubGameplayClockContainer(gameplayClock);
    }

    public class SubGameplayClockContainer : GameplayClockContainer
    {
        public new DecoupleableInterpolatingFramedClock AdjustableClock => base.AdjustableClock;

        public SubGameplayClockContainer(IClock sourceClock)
            : base(sourceClock)
        {
        }

        protected override void OnIsPausedChanged(ValueChangedEvent<bool> isPaused)
        {
            if (isPaused.NewValue)
                AdjustableClock.Stop();
            else
                AdjustableClock.Start();
        }

        protected override GameplayClock CreateGameplayClock(IFrameBasedClock source) => new GameplayClock(source);
    }
}
