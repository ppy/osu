// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Game.Input.Bindings;

namespace osu.Game.Overlays.Volume
{
    public class VolumeControlReceptor : Container, IScrollBindingHandler<GlobalAction>, IHandleGlobalKeyboardInput
    {
        public Func<GlobalAction, bool> ActionRequested;
        public Func<GlobalAction, float, bool, bool> ScrollActionRequested;

        public bool OnPressed(GlobalAction action)
        {
            // if nothing else handles selection actions in the game, it's safe to let volume be adjusted.
            switch (action)
            {
                case GlobalAction.SelectPrevious:
                    action = GlobalAction.IncreaseVolume;
                    break;

                case GlobalAction.SelectNext:
                    action = GlobalAction.DecreaseVolume;
                    break;
            }

            return ActionRequested?.Invoke(action) ?? false;
        }

        public bool OnScroll(GlobalAction action, float amount, bool isPrecise) =>
            ScrollActionRequested?.Invoke(action, amount, isPrecise) ?? false;

        public void OnReleased(GlobalAction action)
        {
        }
    }
}
