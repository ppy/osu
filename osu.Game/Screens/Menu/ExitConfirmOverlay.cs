// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Input.Bindings;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;

namespace osu.Game.Screens.Menu
{
    public class ExitConfirmOverlay : HoldToConfirmOverlay, IKeyBindingHandler<GlobalAction>
    {
        protected override bool AllowMultipleFires => true;

        public void Abort() => AbortConfirm();

        public bool OnPressed(GlobalAction action)
        {
            if (action == GlobalAction.Back)
            {
                BeginConfirm();
                return true;
            }

            return false;
        }

        public bool OnReleased(GlobalAction action)
        {
            if (action == GlobalAction.Back)
            {
                if (!Fired)
                    AbortConfirm();
                return true;
            }

            return false;
        }
    }
}
