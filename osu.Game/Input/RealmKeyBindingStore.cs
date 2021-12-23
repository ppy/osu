// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Game.Database;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets;
using Realms;

#nullable enable

namespace osu.Game.Input
{
    public class RealmKeyBindingStore
    {
        private readonly RealmContextFactory realmFactory;
        private readonly ReadableKeyCombinationProvider keyCombinationProvider;

        public RealmKeyBindingStore(RealmContextFactory realmFactory, ReadableKeyCombinationProvider keyCombinationProvider)
        {
            this.realmFactory = realmFactory;
            this.keyCombinationProvider = keyCombinationProvider;
        }

        /// <summary>
        /// Retrieve all user-defined key combinations (in a format that can be displayed) for a specific action.
        /// </summary>
        /// <param name="globalAction">The action to lookup.</param>
        /// <returns>A set of display strings for all the user's key configuration for the action.</returns>
        public IReadOnlyList<string> GetReadableKeyCombinationsFor(GlobalAction globalAction)
        {
            List<string> combinations = new List<string>();

            using (var context = realmFactory.CreateContext())
            {
                foreach (var action in context.All<RealmKeyBinding>().Where(b => string.IsNullOrEmpty(b.RulesetName) && (GlobalAction)b.ActionInt == globalAction))
                {
                    string str = keyCombinationProvider.GetReadableString(action.KeyCombination);

                    // even if found, the readable string may be empty for an unbound action.
                    if (str.Length > 0)
                        combinations.Add(str);
                }
            }

            return combinations;
        }

        /// <summary>
        /// Register all defaults for this store.
        /// </summary>
        /// <param name="container">The container to populate defaults from.</param>
        /// <param name="rulesets">The rulesets to populate defaults from.</param>
        public void Register(KeyBindingContainer container, IEnumerable<RulesetInfo> rulesets)
        {
            using (var realm = realmFactory.CreateContext())
            using (var transaction = realm.BeginWrite())
            {
                // intentionally flattened to a list rather than querying against the IQueryable, as nullable fields being queried against aren't indexed.
                // this is much faster as a result.
                var existingBindings = realm.All<RealmKeyBinding>().ToList();

                insertDefaults(realm, existingBindings, container.DefaultKeyBindings);

                foreach (var ruleset in rulesets)
                {
                    var instance = ruleset.CreateInstance();
                    foreach (int variant in instance.AvailableVariants)
                        insertDefaults(realm, existingBindings, instance.GetDefaultKeyBindings(variant), ruleset.ShortName, variant);
                }

                transaction.Commit();
            }
        }

        private void insertDefaults(Realm realm, List<RealmKeyBinding> existingBindings, IEnumerable<IKeyBinding> defaults, string? rulesetName = null, int? variant = null)
        {
            // compare counts in database vs defaults for each action type.
            foreach (var defaultsForAction in defaults.GroupBy(k => k.Action))
            {
                IEnumerable<RealmKeyBinding> existing = existingBindings.Where(k =>
                    k.RulesetName == rulesetName
                    && k.Variant == variant
                    && k.ActionInt == (int)defaultsForAction.Key);

                int defaultsCount = defaultsForAction.Count();
                int existingCount = existing.Count();

                if (defaultsCount > existingCount)
                {
                    // insert any defaults which are missing.
                    realm.Add(defaultsForAction.Skip(existingCount).Select(k => new RealmKeyBinding
                    {
                        KeyCombinationString = k.KeyCombination.ToString(),
                        ActionInt = (int)k.Action,
                        RulesetName = rulesetName,
                        Variant = variant
                    }));
                }
                else if (defaultsCount < existingCount)
                {
                    // generally this shouldn't happen, but if the user has more key bindings for an action than we expect,
                    // remove the last entries until the count matches for sanity.
                    foreach (var k in existing.TakeLast(existingCount - defaultsCount).ToArray())
                    {
                        realm.Remove(k);

                        // Remove from the local flattened/cached list so future lookups don't query now deleted rows.
                        existingBindings.Remove(k);
                    }
                }
            }
        }

        /// <summary>
        /// Keys which should not be allowed for gameplay input purposes.
        /// </summary>
        private static readonly IEnumerable<InputKey> banned_keys = new[]
        {
            InputKey.MouseWheelDown,
            InputKey.MouseWheelLeft,
            InputKey.MouseWheelUp,
            InputKey.MouseWheelRight
        };

        public static bool CheckValidForGameplay(KeyCombination combination)
        {
            foreach (var key in banned_keys)
            {
                if (combination.Keys.Contains(key))
                    return false;
            }

            return true;
        }
    }
}
