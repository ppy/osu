// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Input;
using osu.Framework.Timing;
using osu.Game.Input.Handlers;

namespace osu.Game.Screens.Play
{
    public class PlayerInputManager : PassThroughInputManager
    {
        private readonly ManualClock clock = new ManualClock();
        private IFrameBasedClock parentClock;

        private ReplayInputHandler replayInputHandler;
        public ReplayInputHandler ReplayInputHandler
        {
            get { return replayInputHandler; }
            set
            {
                if (replayInputHandler != null) RemoveHandler(replayInputHandler);

                replayInputHandler = value;
                UseParentState = replayInputHandler == null;

                if (replayInputHandler != null)
                    AddHandler(replayInputHandler);
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            parentClock = Clock;
            Clock = new FramedClock(clock);
        }

        protected override void Update()
        {
            base.Update();

            if (parentClock == null) return;

            clock.Rate = parentClock.Rate;
            clock.IsRunning = parentClock.IsRunning;

            //if a replayHandler is not attached, we should just pass-through.
            if (UseParentState || replayInputHandler == null)
            {
                clock.CurrentTime = parentClock.CurrentTime;
                base.Update();
                return;
            }

            while (true)
            {
                double? newTime = replayInputHandler.SetFrameFromTime(parentClock.CurrentTime);

                if (newTime == null)
                    //we shouldn't execute for this time value
                    break;

                if (clock.CurrentTime == parentClock.CurrentTime)
                    break;

                clock.CurrentTime = newTime.Value;
                base.Update();
            }
        }
    }
}
