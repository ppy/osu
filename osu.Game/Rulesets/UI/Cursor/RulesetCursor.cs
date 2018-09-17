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

        protected override bool OnMouseMove(InputState state)
        {
            if (ActiveCursor.Position != state.Mouse.Position)
            {
                lastActiveTime = clock.CurrentTime;
                if (State == Visibility.Hidden)
                {
                    Show();
                    PopIn();
                }
            }

            return base.OnMouseMove(state);
        }

        protected override void Update()
        {
            base.Update();

            if (State == Visibility.Visible && idleTime > 2000)
            {
                Hide();
                PopOut();
            }
        }
    }
}
