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

        /// <summary>
        /// Whether we running up-to-date with our parent clock.
        /// If not, we will need to keep processing children until we catch up.
        /// </summary>
        private bool requireMoreUpdateLoops;

        /// <summary>
        /// Whether we in a valid state (ie. should we keep processing children frames).
        /// This should be set to false when the replay is, for instance, waiting for future frames to arrive.
        /// </summary>
        private bool validState;

        protected override bool RequiresChildrenUpdate => base.RequiresChildrenUpdate && validState;

        private bool isAttached => replayInputHandler != null && !UseParentState;

        private const int max_catch_up_updates_per_frame = 50;

        public override bool UpdateSubTree()
        {
            requireMoreUpdateLoops = true;
            validState = true;

            int loops = 0;

            while (validState && requireMoreUpdateLoops && loops++ < 50)
                if (!base.UpdateSubTree())
                    return false;

            return true;
        }

        protected override void Update()
        {
            if (parentClock == null) return;

            clock.Rate = parentClock.Rate;
            clock.IsRunning = parentClock.IsRunning;

            if (!isAttached)
            {
                clock.CurrentTime = parentClock.CurrentTime;
            }
            else
            {
                double? newTime = replayInputHandler.SetFrameFromTime(parentClock.CurrentTime);

                if (newTime == null)
                {
                    // we shouldn't execute for this time value. probably waiting on more replay data.
                    validState = false;
                    return;
                }

                clock.CurrentTime = newTime.Value;
            }

            requireMoreUpdateLoops = clock.CurrentTime != parentClock.CurrentTime;
            base.Update();
        }
    }
}
