// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Configuration;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;

namespace osu.Game.Screens.Menu
{
    public class ExitConfirmOverlay : HoldToConfirmOverlay, IKeyBindingHandler<GlobalAction>
    {
        protected override bool AllowMultipleFires => true;

        public void Abort() => AbortConfirm();

        public ExitConfirmOverlay()
            : base(0.7f)
        {
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Repeat)
                return false;

            if (e.Action == GlobalAction.Back)
            {
                BeginConfirm();
                return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
            if (e.Action == GlobalAction.Back)
            {
                if (!Fired)
                    AbortConfirm();
            }
        }
    }

    /// <summary>
    /// An <see cref="ExitConfirmOverlay"/> that behaves as if the <see cref="OsuSetting.UIHoldActivationDelay"/> is always <c>0</c>.
    /// </summary>
    /// <remarks>This is useful for mobile devices using gesture navigation, where holding to confirm is not possible.</remarks>
    public class NoHoldExitConfirmOverlay : ExitConfirmOverlay, IKeyBindingHandler<GlobalAction>
    {
        public new bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Repeat)
                return false;

            if (e.Action == GlobalAction.Back)
            {
                Progress.Value = 1;
                Confirm();
                return true;
            }

            return false;
        }
    }
}
