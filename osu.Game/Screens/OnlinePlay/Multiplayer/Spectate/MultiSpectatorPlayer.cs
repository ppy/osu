// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Scoring;
using osu.Game.Screens.OnlinePlay.Multiplayer.Spectate.Sync;
using osu.Game.Screens.Play;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Spectate
{
    public class MultiSpectatorPlayer : SpectatorPlayer
    {
        private readonly Bindable<bool> waitingOnFrames = new Bindable<bool>(true);
        private readonly Score score;
        private readonly ISlaveClock slaveClock;

        public MultiSpectatorPlayer(Score score, ISlaveClock slaveClock)
            : base(score)
        {
            this.score = score;
            this.slaveClock = slaveClock;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            slaveClock.WaitingOnFrames.BindTo(waitingOnFrames);
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();
            waitingOnFrames.Value = DrawableRuleset.FrameStableClock.WaitingOnFrames.Value || score.Replay.Frames.Count == 0;
        }

        protected override GameplayClockContainer CreateGameplayClockContainer(WorkingBeatmap beatmap, double gameplayStart)
            => new SlaveGameplayClockContainer(slaveClock);

        private class SlaveGameplayClockContainer : GameplayClockContainer
        {
            public SlaveGameplayClockContainer(IClock sourceClock)
                : base(sourceClock)
            {
            }

            protected override void Update()
            {
                // The slave clock's running state is controlled by the sync manager, but the local pausing state needs to be updated to stop gameplay.
                if (SourceClock.IsRunning)
                    Start();
                else
                    Stop();

                base.Update();
            }

            protected override GameplayClock CreateGameplayClock(IFrameBasedClock source) => new GameplayClock(source);
        }
    }
}
