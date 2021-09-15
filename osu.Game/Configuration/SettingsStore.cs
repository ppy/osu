// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Database;

namespace osu.Game.Configuration
{
    public class SettingsStore
    {
        private readonly RealmContextFactory realmFactory;

        public SettingsStore(RealmContextFactory realmFactory)
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
    }
}
