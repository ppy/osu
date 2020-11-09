// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using Humanizer;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Online.API
{
    public class APIMod : IMod
    {
        [JsonProperty("acronym")]
        public string Acronym { get; set; }

        [JsonProperty("settings")]
        public Dictionary<string, object> Settings { get; set; } = new Dictionary<string, object>();

        [JsonConstructor]
        private APIMod()
        {
        }

        public APIMod(Mod mod)
        {
            Acronym = mod.Acronym;

            foreach (var (_, property) in mod.GetSettingsSourceProperties())
                Settings.Add(property.Name.Underscore(), property.GetValue(mod));
        }

        public Mod ToMod(Ruleset ruleset)
        {
            Mod resultMod = ruleset.GetAllMods().FirstOrDefault(m => m.Acronym == Acronym);

            if (resultMod == null)
                throw new InvalidOperationException($"There is no mod in the ruleset ({ruleset.ShortName}) matching the acronym {Acronym}.");

            foreach (var (_, property) in resultMod.GetSettingsSourceProperties())
            {
                if (!Settings.TryGetValue(property.Name.Underscore(), out object settingValue))
                    continue;

                ((IBindable)property.GetValue(resultMod)).Parse(settingValue);
            }

            return resultMod;
        }

        public bool Equals(IMod other) => Acronym == other?.Acronym;

        public override string ToString()
        {
            if (Settings.Count > 0)
                return $"{Acronym} ({string.Join(',', Settings.Select(kvp => $"{kvp.Key}:{kvp.Value}"))})";

            return $"{Acronym}";
        }
    }
}
