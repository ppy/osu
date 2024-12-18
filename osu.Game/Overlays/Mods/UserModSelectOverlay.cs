// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Input.Events;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Select;
using osu.Game.Utils;

namespace osu.Game.Overlays.Mods
{
    public partial class UserModSelectOverlay : ModSelectOverlay
    {
        private ModSpeedHotkeyHandler modSpeedHotkeyHandler = null!;

        public UserModSelectOverlay(OverlayColourScheme colourScheme = OverlayColourScheme.Green)
            : base(colourScheme)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(modSpeedHotkeyHandler = new ModSpeedHotkeyHandler());
        }

        protected override ModColumn CreateModColumn(ModType modType) => new UserModColumn(modType, false);

        protected override IReadOnlyList<Mod> ComputeNewModsFromSelection(IReadOnlyList<Mod> oldSelection, IReadOnlyList<Mod> newSelection)
        {
            var addedMods = newSelection.Except(oldSelection);
            var removedMods = oldSelection.Except(newSelection);

            IEnumerable<Mod> modsAfterRemoval = newSelection.Except(removedMods).ToList();

            // the preference is that all new mods should override potential incompatible old mods.
            // in general that's a bit difficult to compute if more than one mod is added at a time,
            // so be conservative and just remove all mods that aren't compatible with any one added mod.
            foreach (var addedMod in addedMods)
            {
                if (!ModUtils.CheckCompatibleSet(modsAfterRemoval.Append(addedMod), out var invalidMods))
                    modsAfterRemoval = modsAfterRemoval.Except(invalidMods);

                modsAfterRemoval = modsAfterRemoval.Append(addedMod).ToList();
            }

            return modsAfterRemoval.ToList();
        }

        public override bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            switch (e.Action)
            {
                case GlobalAction.IncreaseModSpeed:
                    return modSpeedHotkeyHandler.ChangeSpeed(0.05, AllAvailableMods.Where(state => state.ValidForSelection.Value).Select(state => state.Mod));

                case GlobalAction.DecreaseModSpeed:
                    return modSpeedHotkeyHandler.ChangeSpeed(-0.05, AllAvailableMods.Where(state => state.ValidForSelection.Value).Select(state => state.Mod));
            }

            return base.OnPressed(e);
        }

        private partial class UserModColumn : ModColumn
        {
            public UserModColumn(ModType modType, bool allowIncompatibleSelection)
                : base(modType, allowIncompatibleSelection)
            {
            }

            protected override ModPanel CreateModPanel(ModState modState) => new IncompatibilityDisplayingModPanel(modState);
        }
    }
}
