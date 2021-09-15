// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Game.Database;
using osu.Game.Rulesets;

namespace osu.Game.Configuration
{
    public abstract class DatabasedConfigManager<TLookup> : ConfigManager<TLookup>
        where TLookup : struct, Enum
    {
        private readonly RealmContextFactory realmFactory;

        private readonly int? variant;

        private List<RealmSetting> databasedSettings;

        private readonly RulesetInfo ruleset;

        protected DatabasedConfigManager(RealmContextFactory realmFactory, RulesetInfo ruleset = null, int? variant = null)
        {
            this.realmFactory = realmFactory;
            this.ruleset = ruleset;
            this.variant = variant;

            Load();

            InitialiseDefaults();
        }

        protected override void PerformLoad()
        {
            var rulesetID = ruleset?.ID;

            // As long as RulesetConfigCache exists, there is no need to subscribe to realm events.
            databasedSettings = realmFactory.Context.All<RealmSetting>().Where(b => b.RulesetID == rulesetID && b.Variant == variant).ToList();
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
                realmFactory.Context.Write(() =>
                {
                    realmFactory.Context.Add(setting = new RealmSetting
                    {
                        Key = lookup.ToString(),
                        Value = bindable.Value,
                        RulesetID = ruleset?.ID,
                        Variant = variant,
                    });
                });

                databasedSettings.Add(setting);
            }

            bindable.ValueChanged += b =>
            {
                realmFactory.Context.Write(() => setting.Value = b.NewValue);
            };
        }
    }
}
