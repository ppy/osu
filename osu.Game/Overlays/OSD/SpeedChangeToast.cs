// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Input;
using osu.Game.Input.Bindings;
using osu.Game.Localisation;

namespace osu.Game.Overlays.OSD
{
    public partial class SpeedChangeToast : Toast
    {
        public SpeedChangeToast(RealmKeyBindingStore keyBindingStore, double newSpeed)
            : base(ModSelectOverlayStrings.ModCustomisation, ToastStrings.SpeedChangedTo(newSpeed), keyBindingStore.GetBindingsStringFor(GlobalAction.IncreaseModSpeed) + " / " + keyBindingStore.GetBindingsStringFor(GlobalAction.DecreaseModSpeed))
        {
        }
    }
}
