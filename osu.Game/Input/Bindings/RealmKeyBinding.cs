// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Input.Bindings;
using osu.Game.Database;
using Realms;

namespace osu.Game.Input.Bindings
{
    [MapTo("KeyBinding")]
    public class RealmKeyBinding : RealmObject, IHasGuidPrimaryKey
    {
        public string ID { get; set; }

        public int? RulesetID { get; set; }

        public int? Variant { get; set; }

        [Ignored]
        public KeyBinding KeyBinding
        {
            get
            {
                var split = KeyBindingString.Split(':');
                return new KeyBinding(split[0], int.Parse(split[1]));
            }
            set => KeyBindingString = $"{value.KeyCombination}:{(int)value.Action}";
        }

        public string KeyBindingString { get; set; }
    }
}
