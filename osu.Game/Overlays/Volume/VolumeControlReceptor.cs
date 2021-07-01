// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Threading;
using osu.Game.Extensions;
using osu.Game.Input.Bindings;

namespace osu.Game.Overlays.Volume
{
    public class VolumeControlReceptor : Container, IScrollBindingHandler<GlobalAction>, IHandleGlobalKeyboardInput
    {
        public Func<GlobalAction, bool> ActionRequested;
        public Func<GlobalAction, float, bool, bool> ScrollActionRequested;

        private ScheduledDelegate keyRepeat;

        public bool OnPressed(GlobalAction action)
        {
            switch (action)
            {
                case GlobalAction.DecreaseVolume:
                case GlobalAction.IncreaseVolume:
                    keyRepeat?.Cancel();
                    keyRepeat = this.BeginKeyRepeat(Scheduler, () => ActionRequested?.Invoke(action), 150);
                    return true;

                case GlobalAction.ToggleMute:
                    ActionRequested?.Invoke(action);
                    return true;
            }

            return false;
        }

        public void OnReleased(GlobalAction action)
        {
            keyRepeat?.Cancel();
        }

        protected override bool OnScroll(ScrollEvent e)
        {
            if (e.ScrollDelta.Y == 0)
                return false;

            // forward any unhandled mouse scroll events to the volume control.
            ScrollActionRequested?.Invoke(GlobalAction.IncreaseVolume, e.ScrollDelta.Y, e.IsPrecise);
            return true;
        }

        public bool OnScroll(GlobalAction action, float amount, bool isPrecise) =>
            ScrollActionRequested?.Invoke(action, amount, isPrecise) ?? false;
    }
}
