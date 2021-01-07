// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Input.Bindings;
using osu.Game.Input.Bindings;

namespace osu.Game.Input
{
    public interface IKeyBindingStore
    {
        event Action KeyBindingChanged;

        void Register(KeyBindingContainer manager);

        /// <summary>
        /// Retrieve all user-defined key combinations (in a format that can be displayed) for a specific action.
        /// </summary>
        /// <param name="globalAction">The action to lookup.</param>
        /// <returns>A set of display strings for all the user's key configuration for the action.</returns>
        IEnumerable<string> GetReadableKeyCombinationsFor(GlobalAction globalAction);

        /// <summary>
        /// Retrieve <see cref="DatabasedKeyBinding"/>s for a specified ruleset/variant content.
        /// </summary>
        /// <param name="rulesetId">The ruleset's internal ID.</param>
        /// <param name="variant">An optional variant.</param>
        /// <returns></returns>
        List<KeyBinding> Query(int? rulesetId = null, int? variant = null);

        /// <summary>
        /// Retrieve <see cref="KeyBinding"/>s for the specified action.
        /// </summary>
        /// <param name="action">The action to lookup.</param>
        List<KeyBinding> Query<T>(T action) where T : Enum;

        public void Update(KeyBinding buttonKeyBinding) => throw new NotImplementedException();
    }
}
