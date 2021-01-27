// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Overlays.Mods
{
    public class SoloModSelectOverlay : ModSelectOverlay
    {
        public SoloModSelectOverlay(Func<Mod, bool> isValidMod = null)
            : base(isValidMod)
        {
        }

        protected override void OnModSelected(Mod mod)
        {
            base.OnModSelected(mod);

            foreach (var section in ModSectionsContainer.Children)
                section.DeselectTypes(mod.IncompatibleMods, true);
        }
    }
}
