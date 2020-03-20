// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
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
        public virtual string IconTooltip
        {
            get
            {
                List<string> attributes = new List<string>();

                foreach ((SettingSourceAttribute attr, System.Reflection.PropertyInfo property) in this.GetOrderedSettingsSourceProperties())
                {
                    // use TooltipText from SettingSource if available, but fall back to Label, which has to be provided
                    string tooltipText = attr.TooltipText ?? attr.Label + " {0}";
                    object bindableObj = property.GetValue(this);

                    if (bindableObj is BindableInt bindableInt && !bindableInt.IsDefault)
                    {
                        attributes.Add(string.Format(tooltipText, bindableInt.Value));
                        continue;
                    }

                    if (bindableObj is BindableFloat bindableFloat && !bindableFloat.IsDefault)
                    {
                        attributes.Add(string.Format(tooltipText, bindableFloat.Value));
                        continue;
                    }

                    if (bindableObj is BindableDouble bindableDouble && !bindableDouble.IsDefault)
                    {
                        attributes.Add(string.Format(tooltipText, bindableDouble.Value));
                    }
                }

                return $"{Name}{(attributes.Any() ? $" ({string.Join(", ", attributes)})" : "")}";
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
