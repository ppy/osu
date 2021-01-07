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
    public class RealmKeyBindingStore : RealmBackedStore, IKeyBindingStore
    {
        public event Action KeyBindingChanged;

        public RealmKeyBindingStore(RealmContextFactory contextFactory, RulesetStore rulesets, Storage storage = null)
            : base(contextFactory, storage)
        {
            using (ContextFactory.GetForWrite())
            {
                foreach (RulesetInfo info in rulesets.AvailableRulesets)
                {
                    var ruleset = info.CreateInstance();
                    foreach (var variant in ruleset.AvailableVariants)
                        insertDefaults(ruleset.GetDefaultKeyBindings(variant), info.ID, variant);
                }
            }
        }

        /// <summary>
        /// Retrieve all user-defined key combinations (in a format that can be displayed) for a specific action.
        /// </summary>
        /// <param name="globalAction">The action to lookup.</param>
        /// <returns>A set of display strings for all the user's key configuration for the action.</returns>
        public IEnumerable<string> GetReadableKeyCombinationsFor(GlobalAction globalAction)
        {
            foreach (var action in query().Where(b => (GlobalAction)b.KeyBinding.Action == globalAction))
            {
                string str = action.KeyBinding.KeyCombination.ReadableString();

                // even if found, the readable string may be empty for an unbound action.
                if (str.Length > 0)
                    yield return str;
            }
        }

        public void Register(KeyBindingContainer manager) => insertDefaults(manager.DefaultKeyBindings);

        private void insertDefaults(IEnumerable<KeyBinding> defaults, int? rulesetId = null, int? variant = null)
        {
            using (var usage = ContextFactory.GetForWrite())
            {
                // compare counts in database vs defaults
                foreach (var group in defaults.GroupBy(k => k.Action))
                {
                    int count = query(rulesetId, variant).Count(k => (int)k.KeyBinding.Action == (int)group.Key);
                    int aimCount = group.Count();

                    if (aimCount <= count)
                        continue;

                    foreach (var insertable in group.Skip(count).Take(aimCount - count))
                    {
                        // insert any defaults which are missing.
                        usage.Context.Add(new RealmKeyBinding
                        {
                            ID = Guid.NewGuid().ToString(),
                            KeyBinding = new KeyBinding
                            {
                                KeyCombination = insertable.KeyCombination,
                                Action = insertable.Action,
                            },
                            RulesetID = rulesetId,
                            Variant = variant
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Retrieve <see cref="RealmKeyBinding"/>s for a specified ruleset/variant content.
        /// </summary>
        /// <param name="rulesetId">The ruleset's internal ID.</param>
        /// <param name="variant">An optional variant.</param>
        /// <returns></returns>
        private List<RealmKeyBinding> query(int? rulesetId = null, int? variant = null) =>
            ContextFactory.Get().All<RealmKeyBinding>().Where(b => b.RulesetID == rulesetId && b.Variant == variant).ToList();

        public List<KeyBinding> Query(int? rulesetId = null, int? variant = null)
            => query(rulesetId, variant).Select(k => k.KeyBinding).ToList();

        public List<KeyBinding> Query<T>(T action)
            where T : Enum
        {
            int lookup = (int)(object)action;

            return query(null, null).Where(rkb => rkb.Action == lookup).Select(k => k.KeyBinding).ToList();
        }

        public void Update(KeyBinding keyBinding)
        {
            using (ContextFactory.GetForWrite())
            {
                //todo: fix
                // var dbKeyBinding = (RealmKeyBinding)keyBinding;
                // Refresh(ref dbKeyBinding);
                //
                // if (dbKeyBinding.KeyCombination.Equals(keyBinding.KeyCombination))
                //     return;
                //
                // dbKeyBinding.KeyCombination = keyBinding.KeyCombination;
            }

            KeyBindingChanged?.Invoke();
        }
    }
}
