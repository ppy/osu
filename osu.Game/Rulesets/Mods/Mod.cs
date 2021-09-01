// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Game.Configuration;
using osu.Game.Rulesets.UI;
using osu.Game.Utils;

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// The base class for gameplay modifiers.
    /// </summary>
    [ExcludeFromDynamicCompile]
    public abstract class Mod : IMod, IEquatable<Mod>, IDeepCloneable<Mod>
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
        public abstract string Description { get; }

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
                    var bindable = (IBindable)property.GetValue(this);

                    if (!bindable.IsDefault)
                        tooltipTexts.Add($"{attr.Label} {bindable}");
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
        /// Whether this mod is playable by an end user.
        /// Should be <c>false</c> for cases where the user is not interacting with the game (so it can be excluded from mutliplayer selection, for example).
        /// </summary>
        [JsonIgnore]
        public virtual bool UserPlayable => true;

        [Obsolete("Going forward, the concept of \"ranked\" doesn't exist. The only exceptions are automation mods, which should now override and set UserPlayable to false.")] // Can be removed 20211009
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

        private IReadOnlyList<IBindable> settingsBacking;

        /// <summary>
        /// A list of the all <see cref="IBindable"/> settings within this mod.
        /// </summary>
        internal IReadOnlyList<IBindable> Settings =>
            settingsBacking ??= this.GetSettingsSourceProperties()
                                    .Select(p => p.Item2.GetValue(this))
                                    .Cast<IBindable>()
                                    .ToList();

        /// <summary>
        /// Creates a copy of this <see cref="Mod"/> initialised to a default state.
        /// </summary>
        public virtual Mod DeepClone()
        {
            var result = (Mod)Activator.CreateInstance(GetType());
            result.CopyFrom(this);
            return result;
        }

        /// <summary>
        /// Copies mod setting values from <paramref name="source"/> into this instance, overwriting all existing settings.
        /// </summary>
        /// <param name="source">The mod to copy properties from.</param>
        public void CopyFrom(Mod source)
        {
            if (source.GetType() != GetType())
                throw new ArgumentException($"Expected mod of type {GetType()}, got {source.GetType()}.", nameof(source));

            foreach (var (_, prop) in this.GetSettingsSourceProperties())
            {
                var targetBindable = (IBindable)prop.GetValue(this);
                var sourceBindable = (IBindable)prop.GetValue(source);

                CopyAdjustedSetting(targetBindable, sourceBindable);
            }
        }

        /// <summary>
        /// When creating copies or clones of a Mod, this method will be called
        /// to copy explicitly adjusted user settings from <paramref name="target"/>.
        /// The base implementation will transfer the value via <see cref="Bindable{T}.Parse"/>
        /// or by binding and unbinding (if <paramref name="source"/> is an <see cref="IBindable"/>)
        /// and should be called unless replaced with custom logic.
        /// </summary>
        /// <param name="target">The target bindable to apply the adjustment to.</param>
        /// <param name="source">The adjustment to apply.</param>
        internal virtual void CopyAdjustedSetting(IBindable target, object source)
        {
            if (source is IBindable sourceBindable)
            {
                // copy including transfer of default values.
                target.BindTo(sourceBindable);
                target.UnbindFrom(sourceBindable);
            }
            else
            {
                if (!(target is IParseable parseable))
                    throw new InvalidOperationException($"Bindable type {target.GetType().ReadableName()} is not {nameof(IParseable)}.");

                parseable.Parse(source);
            }
        }

        public bool Equals(IMod other) => other is Mod them && Equals(them);

        public bool Equals(Mod other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return GetType() == other.GetType() &&
                   Settings.SequenceEqual(other.Settings, ModSettingsEqualityComparer.Default);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();

            hashCode.Add(GetType());

            foreach (var setting in Settings)
                hashCode.Add(ModUtils.GetSettingUnderlyingValue(setting));

            return hashCode.ToHashCode();
        }

        /// <summary>
        /// Reset all custom settings for this mod back to their defaults.
        /// </summary>
        public virtual void ResetSettingsToDefaults() => CopyFrom((Mod)Activator.CreateInstance(GetType()));

        private class ModSettingsEqualityComparer : IEqualityComparer<IBindable>
        {
            public static ModSettingsEqualityComparer Default { get; } = new ModSettingsEqualityComparer();

            public bool Equals(IBindable x, IBindable y)
            {
                object xValue = x == null ? null : ModUtils.GetSettingUnderlyingValue(x);
                object yValue = y == null ? null : ModUtils.GetSettingUnderlyingValue(y);

                return EqualityComparer<object>.Default.Equals(xValue, yValue);
            }

            public int GetHashCode(IBindable obj) => ModUtils.GetSettingUnderlyingValue(obj).GetHashCode();
        }
    }
}
