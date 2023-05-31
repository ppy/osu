﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Extensions;
using osu.Game.Rulesets.UI;
using osu.Game.Utils;

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// The base class for gameplay modifiers.
    /// </summary>
    public abstract class Mod : IMod, IEquatable<Mod>, IDeepCloneable<Mod>
    {
        [JsonIgnore]
        public abstract string Name { get; }

        public abstract string Acronym { get; }

        [JsonIgnore]
        public virtual IconUsage? Icon => null;

        [JsonIgnore]
        public virtual ModType Type => ModType.Fun;

        [JsonIgnore]
        public abstract LocalisableString Description { get; }

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
                    var bindable = (IBindable)property.GetValue(this)!;

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

        [JsonIgnore]
        public virtual bool UserPlayable => true;

        [JsonIgnore]
        public virtual bool ValidForMultiplayer => true;

        [JsonIgnore]
        public virtual bool ValidForMultiplayerAsFreeMod => true;

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

        private IReadOnlyDictionary<string, IBindable>? settingsBacking;

        /// <summary>
        /// All <see cref="IBindable"/> settings within this mod.
        /// </summary>
        /// <remarks>
        /// The settings are returned in ascending key order as per <see cref="SettingsMap"/>.
        /// The ordering is intentionally enforced manually, as ordering of <see cref="Dictionary{TKey,TValue}.Values"/> is unspecified.
        /// </remarks>
        internal IEnumerable<IBindable> SettingsBindables => SettingsMap.OrderBy(pair => pair.Key).Select(pair => pair.Value);

        /// <summary>
        /// Provides mapping of names to <see cref="IBindable"/>s of all settings within this mod.
        /// </summary>
        internal IReadOnlyDictionary<string, IBindable> SettingsMap =>
            settingsBacking ??= this.GetSettingsSourceProperties()
                                    .Select(p => p.Item2)
                                    .ToDictionary(property => property.Name.ToSnakeCase(), property => (IBindable)property.GetValue(this)!);

        /// <summary>
        /// Whether all settings in this mod are set to their default state.
        /// </summary>
        protected virtual bool UsesDefaultConfiguration => SettingsBindables.All(s => s.IsDefault);

        /// <summary>
        /// Creates a copy of this <see cref="Mod"/> initialised to a default state.
        /// </summary>
        public virtual Mod DeepClone()
        {
            var result = (Mod)Activator.CreateInstance(GetType())!;
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

            foreach (var (_, property) in this.GetSettingsSourceProperties())
            {
                var targetBindable = (IBindable)property.GetValue(this)!;
                var sourceBindable = (IBindable)property.GetValue(source)!;

                CopyAdjustedSetting(targetBindable, sourceBindable);
            }
        }

        /// <summary>
        /// This method copies the values of all settings from <paramref name="source"/> that share the same names with this mod instance.
        /// The most frequent use of this is when switching rulesets, in order to preserve values of common settings during the switch.
        /// </summary>
        /// <remarks>
        /// The values are copied directly, without adjusting for possibly different allowed ranges of values.
        /// If the value of a setting is not valid for this instance due to not falling inside of the allowed range, it will be clamped accordingly.
        /// </remarks>
        /// <param name="source">The mod to extract settings from.</param>
        public void CopyCommonSettingsFrom(Mod source)
        {
            if (source.UsesDefaultConfiguration)
                return;

            foreach (var (name, targetSetting) in SettingsMap)
            {
                if (!source.SettingsMap.TryGetValue(name, out IBindable? sourceSetting))
                    continue;

                if (sourceSetting.IsDefault)
                    continue;

                var targetBindableType = targetSetting.GetType();
                var sourceBindableType = sourceSetting.GetType();

                // if either the target is assignable to the source or the source is assignable to the target,
                // then we presume that the data types contained in both bindables are compatible and we can proceed with the copy.
                // this handles cases like `Bindable<int>` and `BindableInt`.
                if (!targetBindableType.IsAssignableFrom(sourceBindableType) && !sourceBindableType.IsAssignableFrom(targetBindableType))
                    continue;

                // TODO: special case for handling number types

                PropertyInfo property = targetSetting.GetType().GetProperty(nameof(Bindable<bool>.Value))!;
                property.SetValue(targetSetting, property.GetValue(sourceSetting));
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

        public bool Equals(IMod? other) => other is Mod them && Equals(them);

        public bool Equals(Mod? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return GetType() == other.GetType() &&
                   SettingsBindables.SequenceEqual(other.SettingsBindables, ModSettingsEqualityComparer.Default);
        }

        public override int GetHashCode()
        {
            var hashCode = new HashCode();

            hashCode.Add(GetType());

            foreach (var setting in SettingsBindables)
                hashCode.Add(setting.GetUnderlyingSettingValue());

            return hashCode.ToHashCode();
        }

        /// <summary>
        /// Reset all custom settings for this mod back to their defaults.
        /// </summary>
        public virtual void ResetSettingsToDefaults() => CopyFrom((Mod)Activator.CreateInstance(GetType())!);

        private class ModSettingsEqualityComparer : IEqualityComparer<IBindable>
        {
            public static ModSettingsEqualityComparer Default { get; } = new ModSettingsEqualityComparer();

            public bool Equals(IBindable? x, IBindable? y)
            {
                object? xValue = x?.GetUnderlyingSettingValue();
                object? yValue = y?.GetUnderlyingSettingValue();

                return EqualityComparer<object>.Default.Equals(xValue, yValue);
            }

            public int GetHashCode(IBindable obj) => obj.GetUnderlyingSettingValue().GetHashCode();
        }
    }
}
