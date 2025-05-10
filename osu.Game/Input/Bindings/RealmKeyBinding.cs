// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Input.Bindings;
using osu.Game.Database;
using osu.Game.Rulesets;
using Realms;

namespace osu.Game.Input.Bindings
{
    [MapTo(nameof(KeyBinding))]
    public class RealmKeyBinding : RealmObject, IHasGuidPrimaryKey, IKeyBinding
    {
        [PrimaryKey]
        public Guid ID { get; set; }

        public string? RulesetName { get; set; }

        public int? Variant { get; set; }

        [Ignored]
        public KeyCombination KeyCombination
        {
            get => KeyCombinationString;
            set => KeyCombinationString = value.ToString();
        }

        /// <summary>
        /// The resultant action which is triggered by this binding.
        /// </summary>
        /// <remarks>
        /// This implementation always returns an integer.
        /// If wanting to get the actual enum-typed value, use <see cref="GetAction"/>.
        /// </remarks>
        [Ignored]
        public object Action
        {
            get => ActionInt;
            set => ActionInt = (int)value;
        }

        [MapTo(nameof(Action))]
        public int ActionInt { get; set; }

        [MapTo(nameof(KeyCombination))]
        public string KeyCombinationString { get; set; } = null!;

        public RealmKeyBinding(object action, KeyCombination keyCombination, string? rulesetName = null, int? variant = null)
        {
            Action = action;
            KeyCombination = keyCombination;

            RulesetName = rulesetName;
            Variant = variant;
            ID = Guid.NewGuid();
        }

        [UsedImplicitly] // Realm
        private RealmKeyBinding()
        {
        }

        public object GetAction(RulesetStore rulesets)
        {
            if (string.IsNullOrEmpty(RulesetName))
                return (GlobalAction)ActionInt;

            var ruleset = rulesets.GetRuleset(RulesetName);
            var actionType = ruleset!.CreateInstance()
                                     .GetDefaultKeyBindings(Variant ?? 0)
                                     .First() // let's just assume nobody does something stupid like mix multiple types...
                                     .Action
                                     .GetType();

            return Enum.ToObject(actionType, ActionInt);
        }
    }
}
