// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

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
        /// Whether the mod requires further customisation.
        /// This flag is read by the <see cref="ModSelectOverlay"/> to determine if the customisation panel should be opened after a mod change
        /// and cleared after reading.
        /// </summary>
        public bool PendingConfiguration { get; set; }

        /// <summary>
        /// Whether the mod is currently valid for selection.
        /// This can be <see langword="false"/> in scenarios such as the free mod select overlay, where not all mods are selectable
        /// regardless of search criteria imposed by the user selecting.
        /// </summary>
        public BindableBool ValidForSelection { get; } = new BindableBool(true);

        /// <summary>
        /// Determine if <see cref="Mod"/> is valid and can be shown
        /// </summary>
        public bool IsValid => MatchingFilter.Value && ValidForSelection.Value;

        /// <summary>
        /// Whether the mod is matching the current filter, i.e. it is available for user selection.
        /// </summary>
        public BindableBool MatchingFilter { get; } = new BindableBool(true);

        public ModState(Mod mod)
        {
            Mod = mod;
        }
    }
}
