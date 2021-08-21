// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Database;
using Realms;

namespace osu.Game.Configuration
{
    public class RealmSettingsStore
    {
        private readonly RealmContextFactory realmFactory;

        public RealmSettingsStore(RealmContextFactory realmFactory)
        {
            this.realmFactory = realmFactory;
        }

        /// <summary>
        /// Retrieve <see cref="RealmSetting"/>s for a specified ruleset/variant content.
        /// </summary>
        /// <param name="rulesetId">The ruleset's internal ID.</param>
        /// <param name="variant">An optional variant.</param>
        public List<RealmSetting> Query(int? rulesetId = null, int? variant = null)
        {
            using (var context = realmFactory.GetForRead())
                return context.Realm.All<RealmSetting>().Where(b => b.RulesetID == rulesetId && b.Variant == variant).ToList();
        }

        public void Update(RealmSetting setting)
        {
            using (ContextFactory.GetForWrite())
            {
                var newValue = setting.Value;
                Refresh(ref setting);
                setting.Value = newValue;
            }
        }

        public void Delete(RealmSetting setting)
        {
            using (var usage = ContextFactory.GetForWrite())
                usage.Context.Remove(setting);
        }
    }
}
