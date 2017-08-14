// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Input.Bindings;
using osu.Game.Rulesets;
using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;

namespace osu.Game.Input.Bindings
{
    [Table("KeyBinding")]
    public class DatabasedKeyBinding : KeyBinding
    {
        [ForeignKey(typeof(RulesetInfo))]
        public int? RulesetID { get; set; }

        [Indexed]
        public int? Variant { get; set; }

        [Column("Keys")]
        public string KeysString
        {
            get { return KeyCombination.ToString(); }
            private set { KeyCombination = value; }
        }

        [Column("Action")]
        public new int Action
        {
            get { return (int)base.Action; }
            private set { base.Action = value; }
        }
    }
}