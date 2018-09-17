// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Graphics.Cursor;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.States;
using osu.Framework.Timing;

namespace osu.Game.Rulesets.UI.Cursor
{
    public class RulesetCursor : MenuCursor
    {
        private double lastActiveTime = 0;
        private StopwatchClock clock = new StopwatchClock(true);
        private double idleTime => clock.CurrentTime - lastActiveTime;
        ///<summary> 
        ///Time of idling after which the cursor is hidden.
        ///<summary/>
        public double IdleDelay = 2000;

        protected override bool OnMouseMove(InputState state)
        {
            if (ActiveCursor.Position != state.Mouse.Position)
            {
                lastActiveTime = clock.CurrentTime;
                if (State == Visibility.Hidden)
                    Show();
            }

            return base.OnMouseMove(state);
        }

        protected override void Update()
        {
            base.Update();

            if (State == Visibility.Visible && idleTime > IdleDelay)
                Hide();
        }
    }
}
