// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Input.Bindings;
using osu.Game.Database;
using osu.Game.Rulesets;

namespace osu.Game.Input.Bindings
{
    /// <summary>
    /// A KeyBindingInputManager with a database backing for custom overrides.
    /// </summary>
    /// <typeparam name="T">The type of the custom action.</typeparam>
    public class DatabasedKeyBindingContainer<T> : KeyBindingContainer<T>
        where T : struct
    {
        private readonly RulesetInfo ruleset;

        private readonly int? variant;

        private IDisposable realmSubscription;
        private IQueryable<RealmKeyBinding> realmKeyBindings;

        [Resolved]
        private RealmContextFactory realmFactory { get; set; }

        public override IEnumerable<IKeyBinding> DefaultKeyBindings => ruleset.CreateInstance().GetDefaultKeyBindings(variant ?? 0);

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="ruleset">A reference to identify the current <see cref="Ruleset"/>. Used to lookup mappings. Null for global mappings.</param>
        /// <param name="variant">An optional variant for the specified <see cref="Ruleset"/>. Used when a ruleset has more than one possible keyboard layouts.</param>
        /// <param name="simultaneousMode">Specify how to deal with multiple matches of <see cref="KeyCombination"/>s and <typeparamref name="T"/>s.</param>
        /// <param name="matchingMode">Specify how to deal with exact <see cref="KeyCombination"/> matches.</param>
        public DatabasedKeyBindingContainer(RulesetInfo ruleset = null, int? variant = null, SimultaneousBindingMode simultaneousMode = SimultaneousBindingMode.None, KeyCombinationMatchingMode matchingMode = KeyCombinationMatchingMode.Any)
            : base(simultaneousMode, matchingMode)
        {
            this.ruleset = ruleset;
            this.variant = variant;

            if (ruleset != null && variant == null)
                throw new InvalidOperationException($"{nameof(variant)} can not be null when a non-null {nameof(ruleset)} is provided.");
        }

        protected override void LoadComplete()
        {
            string rulesetName = ruleset?.ShortName;

            realmKeyBindings = realmFactory.Context.All<RealmKeyBinding>()
                                           .Where(b => b.RulesetName == rulesetName && b.Variant == variant);

            realmSubscription = realmKeyBindings
                .QueryAsyncWithNotifications((sender, changes, error) =>
                {
                    // first subscription ignored as we are handling this in LoadComplete.
                    if (changes == null)
                        return;

                    ReloadMappings();
                });

            base.LoadComplete();
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            realmSubscription?.Dispose();
        }

        protected override void ReloadMappings()
        {
            var defaults = DefaultKeyBindings.ToList();

            List<RealmKeyBinding> newBindings = realmKeyBindings.Detach()
                                                                // this ordering is important to ensure that we read entries from the database in the order
                                                                // enforced by DefaultKeyBindings. allow for song select to handle actions that may otherwise
                                                                // have been eaten by the music controller due to query order.
                                                                .OrderBy(b => defaults.FindIndex(d => (int)d.Action == b.ActionInt)).ToList();

            // In the case no bindings were found in the database, presume this usage is for a non-databased ruleset.
            // This actually should never be required and can be removed if it is ever deemed to cause a problem.
            // See https://github.com/ppy/osu/issues/8805 for original reasoning, which is no longer valid as we use ShortName
            // for lookups these days.
            if (newBindings.Count == 0)
                KeyBindings = defaults;
            else
                KeyBindings = newBindings;
        }
    }
}
