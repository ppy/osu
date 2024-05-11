// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Input;

namespace osu.Game.Input
{
    public partial class GameIdleTracker : IdleTracker
    {
        private InputManager inputManager;

        public GameIdleTracker(int time)
            : base(time)
        {
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            inputManager = GetContainingInputManager();
        }

        protected override bool AllowIdle => inputManager.FocusedDrawable == null;
    }
}
