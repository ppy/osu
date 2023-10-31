// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Overlays.Mods
{
    public partial class DeselectAllModsButton : ShearedButton
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
    }
}
