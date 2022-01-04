// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Input.Bindings;
using osu.Game.Database;
using Realms;

#nullable enable

namespace osu.Game.Input.Bindings
{
    [MapTo(nameof(KeyBinding))]
    public class RealmKeyBinding : RealmObject, IHasGuidPrimaryKey, IKeyBinding
    {
        [PrimaryKey]
        public Guid ID { get; set; } = Guid.NewGuid();

        public string? RulesetName { get; set; }

        public int? Variant { get; set; }

        public KeyCombination KeyCombination
        {
            get => KeyCombinationString;
            set => KeyCombinationString = value.ToString();
        }

        public object Action
        {
            get => ActionInt;
            set => ActionInt = (int)value;
        }

        [MapTo(nameof(Action))]
        public int ActionInt { get; set; }

        [MapTo(nameof(KeyCombination))]
        public string KeyCombinationString { get; set; } = string.Empty;
    }
}
