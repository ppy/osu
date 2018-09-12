// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.EventArgs;
using osu.Framework.Input.States;

namespace osu.Game.Screens.Menu
{
    public class IdleTracker : Component, IKeyBindingHandler<PlatformAction>
    {
        private double lastInteractionTime;
        public double IdleTime => Clock.CurrentTime - lastInteractionTime;

        private bool updateLastInteractionTime()
        {
            lastInteractionTime = Clock.CurrentTime;
            return false;
        }

        public bool OnPressed(PlatformAction action) => updateLastInteractionTime();

        public bool OnReleased(PlatformAction action) => updateLastInteractionTime();

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args) => updateLastInteractionTime();

        protected override bool OnKeyUp(InputState state, KeyUpEventArgs args) => updateLastInteractionTime();

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args) => updateLastInteractionTime();

        protected override bool OnMouseUp(InputState state, MouseUpEventArgs args) => updateLastInteractionTime();

        protected override bool OnMouseMove(InputState state) => updateLastInteractionTime();
    }
}
