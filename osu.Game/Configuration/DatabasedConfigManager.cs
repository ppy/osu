﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Configuration;
using osu.Game.Rulesets;

namespace osu.Game.Configuration
{
    public abstract class DatabasedConfigManager<T> : ConfigManager<T>
        where T : struct
    {
        private readonly SettingsStore settings;

        private readonly int variant;

        private readonly List<DatabasedSetting> databasedSettings;

        private readonly RulesetInfo ruleset;

        protected DatabasedConfigManager(SettingsStore settings, RulesetInfo ruleset = null, int variant = 0)
        {
            this.settings = settings;
            this.ruleset = ruleset;
            this.variant = variant;

            databasedSettings = settings.Query(ruleset?.ID, variant);

            InitialiseDefaults();
        }

        protected override void PerformLoad()
        {
        }

        protected override bool PerformSave()
        {
            return true;
        }

        protected override void AddBindable<TBindable>(T lookup, Bindable<TBindable> bindable)
        {
            base.AddBindable(lookup, bindable);

            var setting = databasedSettings.FirstOrDefault(s => (int)s.Key == (int)(object)lookup);
            if (setting != null)
            {
                bindable.Parse(setting.Value);
            }
            else
            {
                settings.Update(setting = new DatabasedSetting
                {
                    Key = lookup,
                    Value = bindable.Value,
                    RulesetID = ruleset?.ID,
                    Variant = variant,
                });

                databasedSettings.Add(setting);
            }

            bindable.ValueChanged += v =>
            {
                setting.Value = v;
                settings.Update(setting);
            };
        }
    }
}
