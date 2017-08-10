// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Input;
using osu.Game.Rulesets;

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

        private readonly bool allowConcurrentActions;

        private readonly List<Binding> mappings = new List<Binding>();

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="ruleset">A reference to identify the current <see cref="Ruleset"/>. Used to lookup mappings. Null for global mappings.</param>
        /// <param name="variant">An optional variant for the specified <see cref="Ruleset"/>. Used when a ruleset has more than one possible keyboard layouts.</param>
        /// <param name="allowConcurrentActions">Allow concurrent actions to be actuated at once. Note that this disables chord bindings.</param>
        protected ActionMappingInputManager(RulesetInfo ruleset = null, int? variant = null, bool allowConcurrentActions = false)
        {
            this.ruleset = ruleset;
            this.variant = variant;
            this.allowConcurrentActions = allowConcurrentActions;
        }

        protected abstract IDictionary<KeyCombination, T> CreateDefaultMappings();

        private BindingStore store;

        [BackgroundDependencyLoader]
        private void load(BindingStore bindings)
        {
            store = bindings;
            ReloadMappings();
        }

        protected void ReloadMappings()
        {
            var rulesetId = ruleset?.ID;

            mappings.Clear();

            foreach (var kvp in CreateDefaultMappings())
                mappings.Add(new Binding(kvp.Key, kvp.Value));

            if (store != null)
            {
                foreach (var b in store.Query<Binding>(b => b.RulesetID == rulesetId && b.Variant == variant))
                    mappings.Add(b);
            }

            if (allowConcurrentActions)
            {
                // ensure we have no overlapping bindings.
                foreach (var m in mappings)
                    foreach (var colliding in mappings.Where(k => !k.Keys.Equals(m.Keys) && k.Keys.CheckValid(m.Keys.Keys)))
                        throw new InvalidOperationException($"Multiple partially overlapping bindings are not supported ({m} and {colliding} are colliding)!");
            }
        }

        private readonly List<Binding> pressedBindings = new List<Binding>();

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (!args.Repeat && (allowConcurrentActions || pressedBindings.Count == 0))
            {
                Binding validBinding;

                if ((validBinding = mappings.Except(pressedBindings).LastOrDefault(m => m.Keys.CheckValid(state.Keyboard.Keys, !allowConcurrentActions))) != null)
                {
                    // store both the pressed combination and the resulting action, just in case the assignments change while we are actuated.
                    pressedBindings.Add(validBinding);
                    state.Data = validBinding.GetAction<T>();
                }
            }

            return base.OnKeyDown(state, args);
        }

        protected override bool OnKeyUp(InputState state, KeyUpEventArgs args)
        {
            foreach (var binding in pressedBindings.ToList())
            {
                if (!binding.Keys.CheckValid(state.Keyboard.Keys, !allowConcurrentActions))
                {
                    // set data as KeyUp.
                    state.Data = binding.GetAction<T>();

                    // and clear the no-longer-valid combination/action.
                    pressedBindings.Remove(binding);
                }
            }

            return base.OnKeyUp(state, args);
        }
    }
}
