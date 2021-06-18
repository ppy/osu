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

            foreach (var variant in instance.AvailableVariants)
                insertDefaults(instance.GetDefaultKeyBindings(variant), ruleset.ID, variant);
        }

        private void insertDefaults(IEnumerable<IKeyBinding> defaults, int? rulesetId = null, int? variant = null)
        {
            using (var usage = realmFactory.GetForWrite())
            {
                // compare counts in database vs defaults
                foreach (var defaultsForAction in defaults.GroupBy(k => k.Action))
                {
                    int existingCount = usage.Realm.All<RealmKeyBinding>().Count(k => k.RulesetID == rulesetId && k.Variant == variant && k.ActionInt == (int)defaultsForAction.Key);

                    if (defaultsForAction.Count() <= existingCount)
                        continue;

                    foreach (var k in defaultsForAction.Skip(existingCount))
                    {
                        // insert any defaults which are missing.
                        usage.Realm.Add(new RealmKeyBinding
                        {
                            KeyCombinationString = k.KeyCombination.ToString(),
                            ActionInt = (int)k.Action,
                            RulesetID = rulesetId,
                            Variant = variant
                        });
                    }
                }

                usage.Commit();
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
