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
            foreach (var info in rulesets.Query<RulesetInfo>())
            {
                var ruleset = info.CreateInstance();
                foreach (var variant in ruleset.AvailableVariants)
                    GetProcessedList(ruleset.GetDefaultKeyBindings(), info.ID, variant);
            }
        }

        public void Register(KeyBindingInputManager manager)
        {
            GetProcessedList(manager.DefaultMappings);
        }

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
            {
                Connection.DropTable<DatabasedKeyBinding>();
            }

            Connection.CreateTable<DatabasedKeyBinding>();
        }

        protected override Type[] ValidTypes => new[]
        {
            typeof(DatabasedKeyBinding)
        };

        public IEnumerable<KeyBinding> GetProcessedList(IEnumerable<KeyBinding> defaults, int? rulesetId = null, int? variant = null)
        {
            var databaseEntries = Query<DatabasedKeyBinding>(b => b.RulesetID == rulesetId && b.Variant == variant);

            if (!databaseEntries.Any())
            {
                // if there are no entries for this category in the database, we should populate our defaults.
                Connection.InsertAll(defaults.Select(k => new DatabasedKeyBinding
                {
                    KeyCombination = k.KeyCombination,
                    Action = (int)k.Action,
                    RulesetID = rulesetId,
                    Variant = variant
                }));
            }

            return databaseEntries;
        }

        public void Update(KeyBinding keyBinding)
        {
            Connection.Update(keyBinding);
        }
    }
}
