// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Input.Bindings;

namespace osu.Game.Overlays.Volume
{
    public class VolumeControlReceptor : Container, IScrollBindingHandler<GlobalAction>, IHandleGlobalKeyboardInput
    {
        public Func<GlobalAction, bool> ActionRequested;
        public Func<GlobalAction, float, bool, bool> ScrollActionRequested;

        public bool OnPressed(GlobalAction action) =>
            ActionRequested?.Invoke(action) ?? false;

        protected override bool OnScroll(ScrollEvent e)
        {
            // forward any unhandled mouse scroll events to the volume control.
            ScrollActionRequested?.Invoke(GlobalAction.IncreaseVolume, e.ScrollDelta.Y, e.IsPrecise);
            return true;
        }

        public bool OnScroll(GlobalAction action, float amount, bool isPrecise) =>
            ScrollActionRequested?.Invoke(action, amount, isPrecise) ?? false;

        public void OnReleased(GlobalAction action)
        {
        }
    }
}
