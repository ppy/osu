using System;
using osu.Framework.Graphics;

namespace osu.Game.Input
{
    /// <summary>
    /// A simple placeholder container which allows handling keyboard input at a higher level than otherwise possible.
    /// </summary>
    public class GlobalHotkeys : Drawable, IHandleActions<GlobalAction>
    {
        public Func<GlobalAction, bool> Handler;

        public override bool HandleInput => true;

        public GlobalHotkeys()
        {
            RelativeSizeAxes = Axes.Both;
        }

        public bool OnPressed(GlobalAction action) => Handler(action);

        public bool OnReleased(GlobalAction action) => false;
    }
}
