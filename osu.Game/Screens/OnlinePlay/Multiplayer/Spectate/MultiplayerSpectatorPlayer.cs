// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Scoring;
using osu.Game.Screens.OnlinePlay.Multiplayer.Spectate.Sync;
using osu.Game.Screens.Play;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate
{
    public class MultiplayerSpectatorPlayer : SpectatorPlayer
    {
        private readonly ISpectatorSlaveClock gameplayClock;

        public MultiplayerSpectatorPlayer(Score score, ISpectatorSlaveClock gameplayClock)
            : base(score)
        {
            this.gameplayClock = gameplayClock;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            gameplayClock.WaitingOnFrames.BindTo(DrawableRuleset.FrameStableClock.WaitingOnFrames);
        }

        protected override GameplayClockContainer CreateGameplayClockContainer(WorkingBeatmap beatmap, double gameplayStart)
            => new SubGameplayClockContainer(gameplayClock);
    }

    public class SubGameplayClockContainer : GameplayClockContainer
    {
        public SubGameplayClockContainer(IClock sourceClock)
            : base(sourceClock)
        {
        }

        protected override GameplayClock CreateGameplayClock(IFrameBasedClock source) => new GameplayClock(source);
    }
}
