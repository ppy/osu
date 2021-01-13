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
        public RealmKeyBindingStore(RealmContextFactory realmFactory, Storage? storage = null)
            : base(realmFactory, storage)
        {
        }

        /// <summary>
        /// Retrieve all user-defined key combinations (in a format that can be displayed) for a specific action.
        /// </summary>
        /// <param name="globalAction">The action to lookup.</param>
        /// <returns>A set of display strings for all the user's key configuration for the action.</returns>
        public IEnumerable<string> GetReadableKeyCombinationsFor(GlobalAction globalAction)
        {
            foreach (var action in RealmFactory.Context.All<RealmKeyBinding>().Where(b => (GlobalAction)b.Action == globalAction))
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
        /// Register a ruleset, adding default bindings for each of its variants.
        /// </summary>
        /// <param name="ruleset">The ruleset to populate defaults from.</param>
        public void Register(RulesetInfo ruleset)
        {
            var instance = ruleset.CreateInstance();

            using (RealmFactory.GetForWrite())
            {
                foreach (var variant in instance.AvailableVariants)
                    insertDefaults(instance.GetDefaultKeyBindings(variant), ruleset.ID, variant);
            }
        }

        private void insertDefaults(IEnumerable<IKeyBinding> defaults, int? rulesetId = null, int? variant = null)
        {
            using (var usage = RealmFactory.GetForWrite())
            {
                // compare counts in database vs defaults
                foreach (var group in defaults.GroupBy(k => k.Action))
                {
                    int count = usage.Context.All<RealmKeyBinding>().Count(k => k.RulesetID == rulesetId && k.Variant == variant && k.Action == (int)group.Key);
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
    }
}
