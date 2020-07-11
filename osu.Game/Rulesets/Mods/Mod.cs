// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.IO.Serialization;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// The base class for gameplay modifiers.
    /// </summary>
    public abstract class Mod : IMod, IJsonSerializable
    {
        /// <summary>
        /// The name of this mod.
        /// </summary>
        [JsonIgnore]
        public abstract string Name { get; }

        /// <summary>
        /// The shortened name of this mod.
        /// </summary>
        public abstract string Acronym { get; }

        /// <summary>
        /// The icon of this mod.
        /// </summary>
        [JsonIgnore]
        public virtual IconUsage? Icon => null;

        /// <summary>
        /// The type of this mod.
        /// </summary>
        [JsonIgnore]
        public virtual ModType Type => ModType.Fun;

        /// <summary>
        /// The user readable description of this mod.
        /// </summary>
        [JsonIgnore]
        public virtual string Description => string.Empty;

        /// <summary>
        /// The tooltip to display for this mod when used in a <see cref="ModIcon"/>.
        /// </summary>
        /// <remarks>
        /// Differs from <see cref="Name"/>, as the value of attributes (AR, CS, etc) changeable via the mod
        /// are displayed in the tooltip.
        /// </remarks>
        [JsonIgnore]
        public string IconTooltip
        {
            get
            {
                string description = SettingDescription;

                return string.IsNullOrEmpty(description) ? Name : $"{Name} ({description})";
            }
        }

        /// <summary>
        /// The description of editable settings of a mod to use in the <see cref="IconTooltip"/>.
        /// </summary>
        /// <remarks>
        /// Parentheses are added to the tooltip, surrounding the value of this property. If this property is <c>string.Empty</c>,
        /// the tooltip will not have parentheses.
        /// </remarks>
        public virtual string SettingDescription
        {
            get
            {
                var tooltipTexts = new List<string>();

                foreach ((SettingSourceAttribute attr, PropertyInfo property) in this.GetOrderedSettingsSourceProperties())
                {
                    object bindableObj = property.GetValue(this);

                    if ((bindableObj as IHasDefaultValue)?.IsDefault == true)
                        continue;

                    tooltipTexts.Add($"{attr.Label} {bindableObj}");
                }

                return string.Join(", ", tooltipTexts.Where(s => !string.IsNullOrEmpty(s)));
            }
        }

        /// <summary>
        /// The score multiplier of this mod.
        /// </summary>
        [JsonIgnore]
        public abstract double ScoreMultiplier { get; }

        /// <summary>
        /// Returns true if this mod is implemented (and playable).
        /// </summary>
        [JsonIgnore]
        public virtual bool HasImplementation => this is IApplicableMod;

        /// <summary>
        /// Returns if this mod is ranked.
        /// </summary>
        [JsonIgnore]
        public virtual bool Ranked => false;

        /// <summary>
        /// Whether this mod requires configuration to apply changes to the game.
        /// </summary>
        [JsonIgnore]
        public virtual bool RequiresConfiguration => false;

        /// <summary>
        /// The mods this mod cannot be enabled with.
        /// </summary>
        [JsonIgnore]
        public virtual Type[] IncompatibleMods => Array.Empty<Type>();

        /// <summary>
        /// Creates a copy of this <see cref="Mod"/> initialised to a default state.
        /// </summary>
        public virtual Mod CreateCopy() => (Mod)MemberwiseClone();

        public bool Equals(IMod other) => GetType() == other?.GetType();
    }
}
