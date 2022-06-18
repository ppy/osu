// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Game.Rulesets.Mods;
using osu.Game.Utils;
using osuTK.Input;

namespace osu.Game.Overlays.Mods
{
    public class UserModSelectOverlay : ModSelectOverlay
    {
        public UserModSelectOverlay(OverlayColourScheme colourScheme = OverlayColourScheme.Green)
            : base(colourScheme)
        {
        }

        protected override ModColumn CreateModColumn(ModType modType, Key[] toggleKeys = null) => new UserModColumn(modType, false, toggleKeys);

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

        private class UserModColumn : ModColumn
        {
            public UserModColumn(ModType modType, bool allowBulkSelection, [CanBeNull] Key[] toggleKeys = null)
                : base(modType, allowBulkSelection, toggleKeys)
            {
            }

            protected override ModPanel CreateModPanel(ModState modState) => new IncompatibilityDisplayingModPanel(modState);
        }
    }
}
