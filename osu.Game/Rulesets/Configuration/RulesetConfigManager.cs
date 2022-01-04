// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Game.Configuration;
using osu.Game.Database;

namespace osu.Game.Rulesets.Configuration
{
    public abstract class RulesetConfigManager<TLookup> : ConfigManager<TLookup>, IRulesetConfigManager
        where TLookup : struct, Enum
    {
        private readonly RealmContextFactory realmFactory;

        private readonly int variant;

        private List<RealmRulesetSetting> databasedSettings = new List<RealmRulesetSetting>();

        private readonly string rulesetName;

        protected RulesetConfigManager(SettingsStore store, RulesetInfo ruleset, int? variant = null)
        {
            realmFactory = store?.Realm;

            rulesetName = ruleset.ShortName;

            this.variant = variant ?? 0;

            Load();

            InitialiseDefaults();
        }

        protected override void PerformLoad()
        {
            if (realmFactory != null)
            {
                // As long as RulesetConfigCache exists, there is no need to subscribe to realm events.
                databasedSettings = realmFactory.Context.All<RealmRulesetSetting>().Where(b => b.RulesetName == rulesetName && b.Variant == variant).ToList();
            }
        }

        private readonly HashSet<TLookup> pendingWrites = new HashSet<TLookup>();

        protected override bool PerformSave()
        {
            TLookup[] changed;

            lock (pendingWrites)
            {
                changed = pendingWrites.ToArray();
                pendingWrites.Clear();
            }

            if (realmFactory == null)
                return true;

            using (var context = realmFactory.CreateContext())
            {
                context.Write(realm =>
                {
                    foreach (var c in changed)
                    {
                        var setting = realm.All<RealmRulesetSetting>().First(s => s.RulesetName == rulesetName && s.Variant == variant && s.Key == c.ToString());

                        setting.Value = ConfigStore[c].ToString();
                    }
                });
            }

            return true;
        }

        protected override void AddBindable<TBindable>(TLookup lookup, Bindable<TBindable> bindable)
        {
            base.AddBindable(lookup, bindable);

            var setting = databasedSettings.Find(s => s.Key == lookup.ToString());

            if (setting != null)
            {
                bindable.Parse(setting.Value);
            }
            else
            {
                setting = new RealmRulesetSetting
                {
                    Key = lookup.ToString(),
                    Value = bindable.Value.ToString(),
                    RulesetName = rulesetName,
                    Variant = variant,
                };

                realmFactory?.Context.Write(() => realmFactory.Context.Add(setting));

                databasedSettings.Add(setting);
            }

            bindable.ValueChanged += b =>
            {
                lock (pendingWrites)
                    pendingWrites.Add(lookup);
            };
        }
    }
}
