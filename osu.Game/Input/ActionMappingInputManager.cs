// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Input;
using osu.Game.Rulesets;
using OpenTK.Input;

namespace osu.Game.Input
{
    /// <summary>
    /// Maps custom action data of type <see cref="T"/> and stores to <see cref="InputState.Data"/>.
    /// </summary>
    /// <typeparam name="T">The type of the custom action.</typeparam>
    public abstract class ActionMappingInputManager<T> : PassThroughInputManager
        where T : struct
    {
        private readonly RulesetInfo ruleset;

        private readonly int? variant;

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="ruleset">A reference to identify the current <see cref="Ruleset"/>. Used to lookup mappings. Null for global mappings.</param>
        /// <param name="variant">An optional variant for the specified <see cref="Ruleset"/>. Used when a ruleset has more than one possible keyboard layouts.</param>
        protected ActionMappingInputManager(RulesetInfo ruleset = null, int? variant = null)
        {
            this.ruleset = ruleset;
            this.variant = variant;

            Mappings = CreateDefaultMappings();
        }

        protected IDictionary<Key, T> Mappings { get; private set; }

        protected abstract IDictionary<Key, T> CreateDefaultMappings();

        [BackgroundDependencyLoader]
        private void load(BindingStore bindings)
        {
            var rulesetId = ruleset?.ID;
            foreach (var b in bindings.Query<Binding>(b => b.RulesetID == rulesetId && b.Variant == variant))
                Mappings[b.Key] = (T)(object)b.Action;
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            mapKey(state, args.Key);
            return base.OnKeyDown(state, args);
        }

        protected override bool OnKeyUp(InputState state, KeyUpEventArgs args)
        {
            mapKey(state, args.Key);
            return base.OnKeyUp(state, args);
        }

        private void mapKey(InputState state, Key key)
        {
            T mappedData;
            if (Mappings.TryGetValue(key, out mappedData))
                state.Data = mappedData;
        }

        private T parseStringRepresentation(string str)
        {
            T res;

            if (Enum.TryParse(str, out res))
                return res;

            return default(T);
        }
    }
}
