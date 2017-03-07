// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Input;
using osu.Game.Configuration;
using System.Linq;
using osu.Framework.Timing;
using osu.Game.Input.Handlers;
using OpenTK.Input;
using KeyboardState = osu.Framework.Input.KeyboardState;
using MouseState = osu.Framework.Input.MouseState;

namespace osu.Game.Screens.Play
{
    public class PlayerInputManager : PassThroughInputManager
    {
        private bool leftViaKeyboard;
        private bool rightViaKeyboard;
        private Bindable<bool> mouseDisabled;

        private ManualClock clock = new ManualClock();
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

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            mouseDisabled = config.GetBindable<bool>(OsuConfig.MouseDisableButtons);
        }

        protected override void TransformState(InputState state)
        {
            base.TransformState(state);

            var mouse = state.Mouse as MouseState;
            var keyboard = state.Keyboard as KeyboardState;

            if (keyboard != null)
            {
                leftViaKeyboard = keyboard.Keys.Contains(Key.Z);
                rightViaKeyboard = keyboard.Keys.Contains(Key.X);
            }

            if (mouse != null)
            {
                if (mouseDisabled.Value)
                {
                    mouse.ButtonStates.Find(s => s.Button == MouseButton.Left).State = false;
                    mouse.ButtonStates.Find(s => s.Button == MouseButton.Right).State = false;
                }

                if (leftViaKeyboard)
                    mouse.ButtonStates.Find(s => s.Button == MouseButton.Left).State = true;
                if (rightViaKeyboard)
                    mouse.ButtonStates.Find(s => s.Button == MouseButton.Right).State = true;
            }
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
