// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;

namespace osu.Game.Input
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

        protected override bool Handle(UIEvent e)
        {
            switch (e)
            {
                case KeyDownEvent _:
                case KeyUpEvent _:
                case MouseDownEvent _:
                case MouseUpEvent _:
                case MouseMoveEvent _:
                    return updateLastInteractionTime();
                default:
                    return base.Handle(e);
            }
        }
    }
}
