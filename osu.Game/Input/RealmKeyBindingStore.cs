// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Input.Bindings;
using osu.Game.Database;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets;

#nullable enable

namespace osu.Game.Input
{
    public class RealmKeyBindingStore
    {
        private readonly RealmContextFactory realmFactory;

        public RealmKeyBindingStore(RealmContextFactory realmFactory)
        {
            this.realmFactory = realmFactory;
        }

        /// <summary>
        /// Retrieve all user-defined key combinations (in a format that can be displayed) for a specific action.
        /// </summary>
        /// <param name="globalAction">The action to lookup.</param>
        /// <returns>A set of display strings for all the user's key configuration for the action.</returns>
        public IReadOnlyList<string> GetReadableKeyCombinationsFor(GlobalAction globalAction)
        {
            List<string> combinations = new List<string>();

            using (var context = realmFactory.GetForRead())
            {
                foreach (var action in context.Realm.All<RealmKeyBinding>().Where(b => b.RulesetID == null && (GlobalAction)b.ActionInt == globalAction))
                {
                    string str = action.KeyCombination.ReadableString();

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
            using (var usage = realmFactory.GetForWrite())
            {
                // intentionally flattened to a list rather than querying against the IQueryable, as nullable fields being queried against aren't indexed.
                // this is much faster as a result.
                var existingBindings = usage.Realm.All<RealmKeyBinding>().ToList();

                insertDefaults(usage, existingBindings, container.DefaultKeyBindings);

                foreach (var ruleset in rulesets)
                {
                    var instance = ruleset.CreateInstance();
                    foreach (var variant in instance.AvailableVariants)
                        insertDefaults(usage, existingBindings, instance.GetDefaultKeyBindings(variant), ruleset.ID, variant);
                }

                usage.Commit();
            }
        }

        private void insertDefaults(RealmContextFactory.RealmUsage usage, List<RealmKeyBinding> existingBindings, IEnumerable<IKeyBinding> defaults, int? rulesetId = null, int? variant = null)
        {
            // compare counts in database vs defaults for each action type.
            foreach (var defaultsForAction in defaults.GroupBy(k => k.Action))
            {
                // avoid performing redundant queries when the database is empty and needs to be re-filled.
                int existingCount = existingBindings.Count(k => k.RulesetID == rulesetId && k.Variant == variant && k.ActionInt == (int)defaultsForAction.Key);

                if (defaultsForAction.Count() <= existingCount)
                    continue;

                // insert any defaults which are missing.
                usage.Realm.Add(defaultsForAction.Skip(existingCount).Select(k => new RealmKeyBinding
                {
                    KeyCombinationString = k.KeyCombination.ToString(),
                    ActionInt = (int)k.Action,
                    RulesetID = rulesetId,
                    Variant = variant
                }));
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
