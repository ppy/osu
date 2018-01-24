// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Database;

namespace osu.Game.Configuration
{
    public class SettingsStore : DatabaseBackedStore
    {
        public event Action SettingChanged;

        public SettingsStore(Func<OsuDbContext> createContext)
            : base(createContext)
        {
        }

        /// <summary>
        /// Retrieve <see cref="DatabasedSetting"/>s for a specified ruleset/variant content.
        /// </summary>
        /// <param name="rulesetId">The ruleset's internal ID.</param>
        /// <param name="variant">An optional variant.</param>
        /// <returns></returns>
        public List<DatabasedSetting> Query(int? rulesetId = null) =>
            GetContext().DatabasedSetting.Where(b => b.RulesetID == rulesetId).ToList();

        public void Update(DatabasedSetting setting)
        {
            var context = GetContext();

            Refresh(ref setting);

            setting.Value = setting.Value;
            context.SaveChanges();

            SettingChanged?.Invoke();
        }
    }
}
