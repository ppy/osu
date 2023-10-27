// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Mania.Mods
{
    /// <summary>
    /// Interface for a <see cref="Mod"/> that adjust <see cref="DrawableManiaRuleset"/>'s scroll speed.
    /// </summary>
    public interface IManiaAdjustScrollSpeed : IApplicableMod
    {
        /// <summary>
        /// The scroll speed in <see cref="DrawableManiaRuleset"/>.
        /// </summary>
        public BindableInt ScrollSpeed { get; set; }
    }
}
