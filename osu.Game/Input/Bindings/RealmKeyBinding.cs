// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Input.Bindings;
using osu.Game.Database;
using Realms;

namespace osu.Game.Input.Bindings
{
    [MapTo("KeyBinding")]
    public class RealmKeyBinding : RealmObject, IHasGuidPrimaryKey, IKeyBinding
    {
        [PrimaryKey]
        public string ID { get; set; }

        public int? RulesetID { get; set; }

        public int? Variant { get; set; }

        KeyCombination IKeyBinding.KeyCombination
        {
            get => KeyCombination;
            set => KeyCombination = value.ToString();
        }

        object IKeyBinding.Action
        {
            get => Action;
            set => Action = (int)value;
        }

        public int Action { get; set; }

        public string KeyCombination { get; set; }

        [Ignored]
        public KeyBinding KeyBinding
        {
            get => new KeyBinding(KeyCombination, Action);
            set
            {
                KeyCombination = value.KeyCombination.ToString();
                Action = (int)value.Action;
            }
        }
    }
}
