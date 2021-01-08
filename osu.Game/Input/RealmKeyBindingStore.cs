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
            foreach (var action in query().Where(b => (GlobalAction)b.Action == globalAction))
            {
                string str = ((IKeyBinding)action).KeyCombination.ReadableString();

                // even if found, the readable string may be empty for an unbound action.
                if (str.Length > 0)
                    yield return str;
            }
        }

        public void Register(KeyBindingContainer manager) => insertDefaults(manager.DefaultKeyBindings);

        private void insertDefaults(IEnumerable<IKeyBinding> defaults, int? rulesetId = null, int? variant = null)
        {
            using (var usage = ContextFactory.GetForWrite())
            {
                // compare counts in database vs defaults
                foreach (var group in defaults.GroupBy(k => k.Action))
                {
                    int count = query(rulesetId, variant).Count(k => k.Action == (int)group.Key);
                    int aimCount = group.Count();

                    if (aimCount <= count)
                        continue;

                    foreach (var insertable in group.Skip(count).Take(aimCount - count))
                    {
                        // insert any defaults which are missing.
                        usage.Context.Add(new RealmKeyBinding
                        {
                            ID = Guid.NewGuid().ToString(),
                            KeyCombination = insertable.KeyCombination.ToString(),
                            Action = (int)insertable.Action,
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
        private IQueryable<RealmKeyBinding> query(int? rulesetId = null, int? variant = null) =>
            ContextFactory.Get().All<RealmKeyBinding>().Where(b => b.RulesetID == rulesetId && b.Variant == variant);

        public List<IKeyBinding> Query(int? rulesetId = null, int? variant = null)
            => query(rulesetId, variant).ToList().Select(r => r.Detach()).ToList<IKeyBinding>();

        public List<IKeyBinding> Query<T>(T action)
            where T : Enum
        {
            int lookup = (int)(object)action;

            return query(null, null).Where(rkb => rkb.Action == lookup).ToList().Select(r => r.Detach()).ToList<IKeyBinding>();
        }

        public void Update(IHasGuidPrimaryKey keyBinding, Action<IKeyBinding> modification)
        {
            using (var realm = ContextFactory.GetForWrite())
            {
                var realmKeyBinding = realm.Context.Find<RealmKeyBinding>(keyBinding.ID);
                modification(realmKeyBinding);
            }

            KeyBindingChanged?.Invoke();
        }
    }
}
