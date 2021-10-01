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

        private readonly int rulesetId;

        protected RulesetConfigManager(SettingsStore store, RulesetInfo ruleset, int? variant = null)
        {
            realmFactory = store?.Realm;

            if (realmFactory != null && !ruleset.ID.HasValue)
                throw new InvalidOperationException("Attempted to add databased settings for a non-databased ruleset");

            rulesetId = ruleset.ID ?? -1;

            this.variant = variant ?? 0;

            Load();

            InitialiseDefaults();
        }

        protected override void PerformLoad()
        {
            if (realmFactory != null)
            {
                // As long as RulesetConfigCache exists, there is no need to subscribe to realm events.
                databasedSettings = realmFactory.Context.All<RealmRulesetSetting>().Where(b => b.RulesetID == rulesetId && b.Variant == variant).ToList();
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
                    Value = bindable.Value.ToString(),
                    RulesetID = rulesetId,
                    Variant = variant,
                };

                realmFactory?.Context.Write(() => realmFactory.Context.Add(setting));

                databasedSettings.Add(setting);
            }

            bindable.ValueChanged += b =>
            {
                realmFactory?.Context.Write(() => setting.Value = b.NewValue.ToString());
            };
        }
    }
}
