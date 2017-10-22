﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
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
        public event Action KeyBindingChanged;

        public KeyBindingStore(Func<OsuDbContext> getContext, RulesetStore rulesets, Storage storage = null)
            : base(getContext, storage)
        {
            foreach (var info in rulesets.AvailableRulesets)
            {
                var ruleset = info.CreateInstance();
                foreach (var variant in ruleset.AvailableVariants)
                    insertDefaults(ruleset.GetDefaultKeyBindings(variant), info.ID, variant);
            }
        }

        public void Register(KeyBindingInputManager manager) => insertDefaults(manager.DefaultKeyBindings);

        protected override void Prepare(bool reset = false)
        {
            if (reset)
                GetContext().Database.ExecuteSqlCommand("DELETE FROM KeyBinding");
        }

        private void insertDefaults(IEnumerable<KeyBinding> defaults, int? rulesetId = null, int? variant = null)
        {
            using (var context = GetContext())
            using (var transaction = context.Database.BeginTransaction())
            {
                // compare counts in database vs defaults
                foreach (var group in defaults.GroupBy(k => k.Action))
                {
                    int count = query(context, rulesetId, variant).Count(k => (int)k.Action == (int)group.Key);
                    int aimCount = group.Count();

                    if (aimCount <= count)
                        continue;

                    foreach (var insertable in group.Skip(count).Take(aimCount - count))
                        // insert any defaults which are missing.
                        context.DatabasedKeyBinding.Add(new DatabasedKeyBinding
                        {
                            KeyCombination = insertable.KeyCombination,
                            Action = insertable.Action,
                            RulesetID = rulesetId,
                            Variant = variant
                        });
                }

                context.SaveChanges();
                transaction.Commit();
            }
        }

        /// <summary>
        /// Retrieve <see cref="KeyBinding"/>s for a specified ruleset/variant content.
        /// </summary>
        /// <param name="rulesetId">The ruleset's internal ID.</param>
        /// <param name="variant">An optional variant.</param>
        /// <returns></returns>
        public IEnumerable<KeyBinding> Query(int? rulesetId = null, int? variant = null) => query(GetContext(), rulesetId, variant);

        private IEnumerable<KeyBinding> query(OsuDbContext context, int? rulesetId = null, int? variant = null) =>
            context.DatabasedKeyBinding.Where(b => b.RulesetID == rulesetId && b.Variant == variant);

        public void Update(KeyBinding keyBinding)
        {
            var context = GetContext();
            context.Update(keyBinding);
            context.SaveChanges();

            KeyBindingChanged?.Invoke();
        }
    }
}
