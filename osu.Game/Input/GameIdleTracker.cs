// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Input;
using osu.Game.Configuration;

namespace osu.Game.Input
{
    public class GameIdleTracker : IdleTracker
    {
        private InputManager inputManager;

        public GameIdleTracker(int time, SessionStatics statics)
            : base(time)
        {
            IsIdle.ValueChanged += _ => UpdateStatics(_, statics);
        }

        protected static void UpdateStatics(ValueChangedEvent<bool> e, SessionStatics statics)
        {
            if (e.OldValue != e.NewValue && e.NewValue)
                statics.ResetValues();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            inputManager = GetContainingInputManager();
        }

        protected override bool AllowIdle => inputManager.FocusedDrawable == null;
    }
}
