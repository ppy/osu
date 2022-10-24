// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.OnlinePlay;

namespace osu.Game.Overlays.Mods
{
    public class SelectAllModsButton : ShearedButton, IKeyBindingHandler<PlatformAction>
    {
        private readonly Bindable<IReadOnlyList<Mod>> selectedMods = new Bindable<IReadOnlyList<Mod>>();
        private readonly Bindable<Dictionary<ModType, IReadOnlyList<ModState>>> availableMods = new Bindable<Dictionary<ModType, IReadOnlyList<ModState>>>();

        public SelectAllModsButton(FreeModSelectOverlay modSelectOverlay)
            : base(ModSelectOverlay.BUTTON_WIDTH)
        {
            Text = CommonStrings.SelectAll;
            Action = modSelectOverlay.SelectAll;

            selectedMods.BindTo(modSelectOverlay.SelectedMods);
            availableMods.BindTo(modSelectOverlay.AvailableMods);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            selectedMods.BindValueChanged(_ => Scheduler.AddOnce(updateEnabledState));
            availableMods.BindValueChanged(_ => Scheduler.AddOnce(updateEnabledState));
            updateEnabledState();
        }

        private void updateEnabledState()
        {
            Enabled.Value = availableMods.Value
                                         .SelectMany(pair => pair.Value)
                                         .Any(modState => !modState.Active.Value && !modState.Filtered.Value);
        }

        public bool OnPressed(KeyBindingPressEvent<PlatformAction> e)
        {
            if (e.Repeat || e.Action != PlatformAction.SelectAll)
                return false;

            TriggerClick();
            return true;
        }

        public void OnReleased(KeyBindingReleaseEvent<PlatformAction> e)
        {
        }
    }
}
