// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Database;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Overlays.Mods
{
    /// <summary>
    /// Wrapper class used to store the current state of a mod preset shown on the <see cref="ModSelectOverlay"/>.
    /// Used primarily to decouple data from drawable logic.
    /// </summary>
    public class ModPresetState
    {
        /// <summary>
        /// The live database object representing the preset this state wraps.
        /// </summary>
        public Live<ModPreset> Preset { get; }

        /// <summary>
        /// Whether the preset is currently selected (i.e., all its mods are selected).
        /// </summary>
        public BindableBool Active { get; } = new BindableBool();

        public BindableBool Preselected { get; } = new BindableBool();

        public ModPresetState(Live<ModPreset> preset)
        {
            Preset = preset;
        }
    }
}
