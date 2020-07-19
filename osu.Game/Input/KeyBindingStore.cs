// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
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

        public KeyBindingStore(DatabaseContextFactory contextFactory, RulesetStore rulesets, Storage storage = null)
            : base(contextFactory, storage)
        {
            using (ContextFactory.GetForWrite())
            {
                foreach (var info in rulesets.AvailableRulesets)
                {
                    var ruleset = info.CreateInstance();

                    foreach (var variant in ruleset.AvailableVariants)
                    {
                        migrate(ruleset.GetDefaultKeyBindings(variant), info.ID, variant);
                        insertDefaults(ruleset.GetDefaultKeyBindings(variant), info.ID, variant);
                    }
                }
            }
        }

        public void Register(KeyBindingContainer manager)
        {
            migrate(manager.DefaultKeyBindings);
            insertDefaults(manager.DefaultKeyBindings);
        }

        // See https://github.com/ppy/osu/pull/9549
        private void migrate(IEnumerable<KeyBinding> defaults, int? rulesetId = null, int? variant = null)
        {
            using (var usage = ContextFactory.GetForWrite())
            {
                foreach (var group in defaults.GroupBy(k => k.Action))
                {
                    foreach (var databasedKeyBinding in usage.Context.DatabasedKeyBinding.Where(
                        b => b.RulesetID == rulesetId && b.Variant == variant && b.IntAction == (int)group.Key && b.ActionName == null))
                    {
                        databasedKeyBinding.ActionName = group.Key.ToString();
                        databasedKeyBinding.IntAction = -1;
                        usage.Context.DatabasedKeyBinding.Update(databasedKeyBinding);
                    }

                    usage.Context.SaveChanges();
                }
            }
        }

        private void insertDefaults(IEnumerable<KeyBinding> defaults, int? rulesetId = null, int? variant = null)
        {
            using (var usage = ContextFactory.GetForWrite())
            {
                // compare counts in database vs defaults
                foreach (var group in defaults.GroupBy(k => k.Action))
                {
                    int count = Query(rulesetId, variant).Count(k => k.ActionName == group.Key.ToString());
                    int aimCount = group.Count();

                    if (aimCount <= count)
                        continue;

                    foreach (var insertable in group.Skip(count).Take(aimCount - count))
                    {
                        // insert any defaults which are missing.
                        usage.Context.DatabasedKeyBinding.Add(new DatabasedKeyBinding
                        {
                            KeyCombination = insertable.KeyCombination,
                            Action = insertable.Action,
                            IntAction = -1,
                            RulesetID = rulesetId,
                            Variant = variant
                        });

                        // required to ensure stable insert order (https://github.com/dotnet/efcore/issues/11686)
                        usage.Context.SaveChanges();
                    }
                }
            }
        }

        /// <summary>
        /// Retrieve <see cref="DatabasedKeyBinding"/>s for a specified ruleset/variant content.
        /// </summary>
        /// <param name="rulesetId">The ruleset's internal ID.</param>
        /// <param name="variant">An optional variant.</param>
        /// <returns></returns>
        public List<DatabasedKeyBinding> Query(int? rulesetId = null, int? variant = null) =>
            ContextFactory.Get().DatabasedKeyBinding.Where(b => b.RulesetID == rulesetId && b.Variant == variant).ToList();

        public void Update(KeyBinding keyBinding)
        {
            using (ContextFactory.GetForWrite())
            {
                var dbKeyBinding = (DatabasedKeyBinding)keyBinding;
                Refresh(ref dbKeyBinding);

                if (dbKeyBinding.KeyCombination.Equals(keyBinding.KeyCombination))
                    return;

                dbKeyBinding.KeyCombination = keyBinding.KeyCombination;
            }

            KeyBindingChanged?.Invoke();
        }
    }
}
