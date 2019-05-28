﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Game.Rulesets;

namespace osu.Game.Configuration
{
    public abstract class DatabasedConfigManager<T> : ConfigManager<T>
        where T : struct
    {
        private readonly SettingsStore settings;

        private readonly int? variant;

        private readonly List<DatabasedSetting> databasedSettings;

        private readonly RulesetInfo ruleset;

        private readonly bool legacySettingsExist;

        protected DatabasedConfigManager(SettingsStore settings, RulesetInfo ruleset = null, int? variant = null)
        {
            this.settings = settings;
            this.ruleset = ruleset;
            this.variant = variant;

            databasedSettings = settings.Query(ruleset?.ID, variant);
            legacySettingsExist = databasedSettings.Any(s => int.TryParse(s.Key, out var _));

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

            if (legacySettingsExist)
            {
                var legacySetting = databasedSettings.Find(s => s.Key == ((int)(object)lookup).ToString());

                if (legacySetting != null)
                {
                    bindable.Parse(legacySetting.Value);
                    settings.Delete(legacySetting);
                }
            }

            var setting = databasedSettings.Find(s => s.Key == lookup.ToString());

            if (setting != null)
            {
                bindable.Parse(setting.Value);
            }
            else
            {
                settings.Update(setting = new DatabasedSetting
                {
                    Key = lookup.ToString(),
                    Value = bindable.Value,
                    RulesetID = ruleset?.ID,
                    Variant = variant,
                });

                databasedSettings.Add(setting);
            }

            bindable.ValueChanged += b =>
            {
                setting.Value = b.NewValue;
                settings.Update(setting);
            };
        }
    }
}
