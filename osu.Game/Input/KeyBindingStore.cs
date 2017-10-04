// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using osu.Framework.Input.Bindings;
using osu.Framework.Platform;
using osu.Game.Database;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets;

namespace osu.Game.Input
{
    public class KeyBindingStore : DatabaseBackedStore
    {
        public KeyBindingStore(OsuDbContext connection, RulesetStore rulesets, Storage storage = null)
            : base(connection, storage)
        {
            foreach (var info in rulesets.AllRulesets)
            {
                var ruleset = info.CreateInstance();
                foreach (var variant in ruleset.AvailableVariants)
                    insertDefaults(ruleset.GetDefaultKeyBindings(variant), info.Id, variant);
            }
        }

        public void Register(KeyBindingInputManager manager) => insertDefaults(manager.DefaultKeyBindings);

        protected override void Prepare(bool reset = false)
        {
            Connection.Database.ExecuteSqlCommand("DELETE FROM KeyBinding");
        }

        private void insertDefaults(IEnumerable<KeyBinding> defaults, int? rulesetId = null, int? variant = null)
        {
            // compare counts in database vs defaults
            foreach (var group in defaults.GroupBy(k => k.Action))
            {
                int count;
                while (group.Count() > (count = Query(rulesetId, variant).Count(k => (int)k.Action == (int)group.Key)))
                {
                    var insertable = group.Skip(count).First();

                    // insert any defaults which are missing.
                    Connection.DatabasedKeyBinding.Add(new DatabasedKeyBinding
                    {
                        KeyCombination = insertable.KeyCombination,
                        Action = insertable.Action,
                        RulesetInfoId = rulesetId,
                        Variant = variant
                    });
                    Connection.SaveChanges();
                }
            }
        }

        protected override Type[] ValidTypes => new[]
        {
            typeof(DatabasedKeyBinding)
        };

        public List<KeyBinding> Query(int? rulesetId = null, int? variant = null) =>
            new List<KeyBinding>(Connection.DatabasedKeyBinding.Where(b => b.RulesetInfoId == rulesetId && b.Variant == variant));

        public void Update(KeyBinding keyBinding)
        {
            var dbKeyBinding = Connection.DatabasedKeyBinding.FirstOrDefault(kb => kb.ToString() == keyBinding.ToString());
            if (dbKeyBinding!=null)
            {
                dbKeyBinding.KeyCombination = keyBinding.KeyCombination;
                dbKeyBinding.Action = keyBinding.Action;
            }
            Connection.SaveChanges();
        }
    }
}
