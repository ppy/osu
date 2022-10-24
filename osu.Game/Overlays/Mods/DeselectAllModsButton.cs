// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osu.Game.Localisation;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Overlays.Mods
{
    public class DeselectAllModsButton : ShearedButton, IKeyBindingHandler<GlobalAction>
    {
        private readonly Bindable<IReadOnlyList<Mod>> selectedMods = new Bindable<IReadOnlyList<Mod>>();

        public DeselectAllModsButton(ModSelectOverlay modSelectOverlay)
            : base(ModSelectOverlay.BUTTON_WIDTH)
        {
            Text = CommonStrings.DeselectAll;
            Action = modSelectOverlay.DeselectAll;

            selectedMods.BindTo(modSelectOverlay.SelectedMods);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            selectedMods.BindValueChanged(_ => updateEnabledState(), true);
        }

        private void updateEnabledState()
        {
            Enabled.Value = selectedMods.Value.Any();
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Repeat || e.Action != GlobalAction.DeselectAllMods)
                return false;

            TriggerClick();
            return true;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }
    }
}
