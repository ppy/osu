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

        private readonly int? variant;

        private List<RealmRulesetSetting> databasedSettings = new List<RealmRulesetSetting>();

        private readonly RulesetInfo ruleset;

        protected RulesetConfigManager(SettingsStore store, RulesetInfo ruleset, int? variant = null)
        {
            realmFactory = store?.Realm;
            this.ruleset = ruleset;
            this.variant = variant;

            Load();

            InitialiseDefaults();
        }

        protected override void PerformLoad()
        {
            var rulesetID = ruleset?.ID;

            if (realmFactory != null)
            {
                // As long as RulesetConfigCache exists, there is no need to subscribe to realm events.
                databasedSettings = realmFactory.Context.All<RealmRulesetSetting>().Where(b => b.RulesetID == rulesetID && b.Variant == variant).ToList();
            }
        }

        protected override bool PerformSave()
        {
            // do nothing, realm saves immediately
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
                    Value = bindable.Value,
                    RulesetID = ruleset?.ID,
                    Variant = variant,
                };

                realmFactory?.Context.Write(() => realmFactory.Context.Add(setting));

                databasedSettings.Add(setting);
            }

            bindable.ValueChanged += b =>
            {
                realmFactory?.Context.Write(() => setting.Value = b.NewValue);
            };
        }
    }
}
