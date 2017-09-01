// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Input.Bindings;
using osu.Framework.Platform;
using osu.Game.Database;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets;
using SQLite.Net;

namespace osu.Game.Input
{
    public class KeyBindingStore : DatabaseBackedStore
    {
        public KeyBindingStore(SQLiteConnection connection, RulesetStore rulesets, Storage storage = null)
            : base(connection, storage)
        {
            foreach (var info in rulesets.AllRulesets)
            {
                var ruleset = info.CreateInstance();
                foreach (var variant in ruleset.AvailableVariants)
                    insertDefaults(ruleset.GetDefaultKeyBindings(variant), info.ID, variant);
            }
        }

        public void Register(KeyBindingInputManager manager) => insertDefaults(manager.DefaultKeyBindings);

        protected override int StoreVersion => 3;

        protected override void PerformMigration(int currentVersion, int targetVersion)
        {
            base.PerformMigration(currentVersion, targetVersion);

            while (currentVersion++ < targetVersion)
            {
                switch (currentVersion)
                {
                    case 1:
                    case 2:
                    case 3:
                        // cannot migrate; breaking underlying changes.
                        Reset();
                        break;
                }
            }
        }

        protected override void Prepare(bool reset = false)
        {
            if (reset)
                Connection.DropTable<DatabasedKeyBinding>();

            Connection.CreateTable<DatabasedKeyBinding>();
        }

        private void insertDefaults(IEnumerable<KeyBinding> defaults, int? rulesetId = null, int? variant = null)
        {
            var query = Query(rulesetId, variant);

            // compare counts in database vs defaults
            foreach (var group in defaults.GroupBy(k => k.Action))
            {
                int count;
                while (group.Count() > (count = query.Count(k => (int)k.Action == (int)group.Key)))
                {
                    var insertable = group.Skip(count).First();

                    // insert any defaults which are missing.
                    Connection.Insert(new DatabasedKeyBinding
                    {
                        KeyCombination = insertable.KeyCombination,
                        Action = insertable.Action,
                        RulesetID = rulesetId,
                        Variant = variant
                    });
                }
            }
        }

        protected override Type[] ValidTypes => new[]
        {
            typeof(DatabasedKeyBinding)
        };

        public IEnumerable<KeyBinding> Query(int? rulesetId = null, int? variant = null) =>
            Query<DatabasedKeyBinding>(b => b.RulesetID == rulesetId && b.Variant == variant);

        public void Update(KeyBinding keyBinding) => Connection.Update(keyBinding);
    }
}
