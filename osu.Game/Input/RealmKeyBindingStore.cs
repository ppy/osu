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

#nullable enable

namespace osu.Game.Input
{
    public class RealmKeyBindingStore : RealmBackedStore
    {
        /// <summary>
        /// Fired whenever any key binding change occurs, across all rulesets and types.
        /// </summary>
        public event Action? KeyBindingChanged;

        public RealmKeyBindingStore(RealmContextFactory contextFactory, RulesetStore? rulesets, Storage? storage = null)
            : base(contextFactory, storage)
        {
            if (rulesets != null)
            {
                // populate defaults from rulesets.
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

        /// <summary>
        /// Register a new type of <see cref="KeyBindingContainer{T}"/>, adding default bindings from <see cref="KeyBindingContainer.DefaultKeyBindings"/>.
        /// </summary>
        /// <param name="container">The container to populate defaults from.</param>
        public void Register(KeyBindingContainer container) => insertDefaults(container.DefaultKeyBindings);

        /// <summary>
        /// Retrieve all key bindings for the provided specification.
        /// </summary>
        /// <param name="rulesetId">An optional ruleset ID. If null, global bindings are returned.</param>
        /// <param name="variant">An optional ruleset variant. If null, the no-variant bindings are returned.</param>
        /// <returns>A list of all key bindings found for the query, detached from the database.</returns>
        public List<RealmKeyBinding> Query(int? rulesetId = null, int? variant = null) => query(rulesetId, variant).ToList();

        /// <summary>
        /// Retrieve all key bindings for the provided action type.
        /// </summary>
        /// <param name="action">The action to lookup.</param>
        /// <typeparam name="T">The enum type of the action.</typeparam>
        /// <returns>A list of all key bindings found for the query, detached from the database.</returns>
        public List<RealmKeyBinding> Query<T>(T action)
            where T : Enum
        {
            int lookup = (int)(object)action;

            return query(null, null).Where(rkb => rkb.Action == lookup).ToList();
        }

        /// <summary>
        /// Update the database mapping for the provided key binding.
        /// </summary>
        /// <param name="keyBinding">The key binding to update. Can be detached from the database.</param>
        /// <param name="modification">The modification to apply to the key binding.</param>
        public void Update(IHasGuidPrimaryKey keyBinding, Action<IKeyBinding> modification)
        {
            // the incoming instance could already be a live access object.
            Live<RealmKeyBinding>? realmBinding = keyBinding as Live<RealmKeyBinding>;

            using (var realm = ContextFactory.GetForWrite())
            {
                if (realmBinding == null)
                {
                    // the incoming instance could be a raw realm object.
                    if (!(keyBinding is RealmKeyBinding rkb))
                        // if neither of the above cases succeeded, retrieve a realm object for further processing.
                        rkb = realm.Context.Find<RealmKeyBinding>(keyBinding.ID);

                    realmBinding = new Live<RealmKeyBinding>(rkb, ContextFactory);
                }

                realmBinding.PerformUpdate(modification);
            }

            KeyBindingChanged?.Invoke();
        }

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
        /// Retrieve live queryable <see cref="RealmKeyBinding"/>s for a specified ruleset/variant content.
        /// </summary>
        /// <param name="rulesetId">An optional ruleset ID. If null, global bindings are returned.</param>
        /// <param name="variant">An optional ruleset variant. If null, the no-variant bindings are returned.</param>
        private IQueryable<RealmKeyBinding> query(int? rulesetId = null, int? variant = null) =>
            ContextFactory.Get().All<RealmKeyBinding>().Where(b => b.RulesetID == rulesetId && b.Variant == variant);
    }
}
