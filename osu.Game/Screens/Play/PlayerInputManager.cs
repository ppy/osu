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

        private Bindable<bool> mouseDisabled;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            mouseDisabled = config.GetBindable<bool>(OsuConfig.MouseDisableButtons);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            parentClock = Clock;
            Clock = new FramedClock(clock);
        }

        protected override void Update()
        {
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
