// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Framework.Logging;
using osu.Game.Configuration;
using osu.Game.Extensions;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Online.API
{
    [MessagePackObject]
    public class APIMod : IEquatable<APIMod>
    {
        [JsonProperty("acronym")]
        [Key(0)]
        public string Acronym { get; set; } = string.Empty;

        [JsonProperty("settings")]
        [Key(1)]
        [MessagePackFormatter(typeof(ModSettingsDictionaryFormatter))]
        public Dictionary<string, object> Settings { get; set; } = new Dictionary<string, object>();

        [JsonConstructor]
        [SerializationConstructor]
        public APIMod()
        {
        }

        public APIMod(Mod mod)
        {
            Acronym = mod.Acronym;

            foreach (var (_, property) in mod.GetSettingsSourceProperties())
            {
                var bindable = (IBindable)property.GetValue(mod)!;

                if (!bindable.IsDefault)
                    Settings.Add(property.Name.ToSnakeCase(), bindable.GetUnderlyingSettingValue());
            }
        }

        public Mod ToMod(Ruleset ruleset)
        {
            Mod? resultMod = ruleset.CreateModFromAcronym(Acronym);

            if (resultMod == null)
            {
                Logger.Log($"There is no mod in the ruleset ({ruleset.ShortName}) matching the acronym {Acronym}.");
                return new UnknownMod(Acronym);
            }

            if (Settings.Count > 0)
            {
                foreach (var (_, property) in resultMod.GetSettingsSourceProperties())
                {
                    if (!Settings.TryGetValue(property.Name.ToSnakeCase(), out object? settingValue))
                        continue;

                    try
                    {
                        resultMod.CopyAdjustedSetting((IBindable)property.GetValue(resultMod)!, settingValue);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Failed to copy mod setting value '{settingValue}' to \"{property.Name}\": {ex.Message}");
                    }
                }
            }

            return resultMod;
        }

        public bool ShouldSerializeSettings() => Settings.Count > 0;

        public bool Equals(APIMod? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return Acronym == other.Acronym && Settings.SequenceEqual(other.Settings, ModSettingsEqualityComparer.Default);
        }

        public override string ToString()
        {
            if (Settings.Count > 0)
                return $"{Acronym} ({string.Join(',', Settings.Select(kvp => $"{kvp.Key}:{kvp.Value}"))})";

            return $"{Acronym}";
        }

        private class ModSettingsEqualityComparer : IEqualityComparer<KeyValuePair<string, object>>
        {
            public static ModSettingsEqualityComparer Default { get; } = new ModSettingsEqualityComparer();

            public bool Equals(KeyValuePair<string, object> x, KeyValuePair<string, object> y)
            {
                object xValue = x.Value.GetUnderlyingSettingValue();
                object yValue = y.Value.GetUnderlyingSettingValue();

                return x.Key == y.Key && EqualityComparer<object>.Default.Equals(xValue, yValue);
            }

            public int GetHashCode(KeyValuePair<string, object> obj) => HashCode.Combine(obj.Key, obj.Value.GetUnderlyingSettingValue());
        }
    }
}
