// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Overlays.Mods
{
    /// <summary>
    /// Wrapper class used to store the current state of a mod shown on the <see cref="ModSelectOverlay"/>.
    /// Used primarily to decouple data from drawable logic.
    /// </summary>
    public class ModState
    {
        /// <summary>
        /// The mod that whose state this instance describes.
        /// </summary>
        public Mod Mod { get; }

        /// <summary>
        /// Whether the mod is currently selected.
        /// </summary>
        public BindableBool Active { get; } = new BindableBool();

        /// <summary>
        /// Whether the mod is currently filtered out due to not matching imposed criteria.
        /// </summary>
        public BindableBool Filtered { get; } = new BindableBool();

        public ModState(Mod mod)
        {
            Mod = mod;
        }
    }
}
