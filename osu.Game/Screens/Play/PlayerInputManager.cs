// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Input;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Input;
using osu.Framework.Timing;
using osu.Game.Configuration;
using osu.Game.Input.Handlers;

namespace osu.Game.Screens.Play
{
    public class PlayerInputManager : PassThroughInputManager
    {
        private ManualClock clock;
        private IFrameBasedClock parentClock;

        private ReplayInputHandler replayInputHandler;
        public ReplayInputHandler ReplayInputHandler
        {
            get
            {
                return replayInputHandler;
            }
            set
            {
                if (replayInputHandler != null) RemoveHandler(replayInputHandler);

                replayInputHandler = value;
                UseParentState = replayInputHandler == null;

                if (replayInputHandler != null)
                    AddHandler(replayInputHandler);
            }
        }

        private Bindable<bool> mouseDisabled;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            mouseDisabled = config.GetBindable<bool>(OsuSetting.MouseDisableButtons);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            //our clock will now be our parent's clock, but we want to replace this to allow manual control.
            parentClock = Clock;

            Clock = new FramedClock(clock = new ManualClock
            {
                CurrentTime = parentClock.CurrentTime,
                Rate = parentClock.Rate,
            });
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

            while (validState && requireMoreUpdateLoops && loops++ < max_catch_up_updates_per_frame)
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

        protected override void TransformState(InputState state)
        {
            base.TransformState(state);

            // we don't want to transform the state if a replay is present (for now, at least).
            if (replayInputHandler != null) return;

            var mouse = state.Mouse as Framework.Input.MouseState;

            if (mouse != null)
            {
                if (mouseDisabled.Value)
                {
                    mouse.SetPressed(MouseButton.Left, false);
                    mouse.SetPressed(MouseButton.Right, false);
                }
            }
        }
    }
}
