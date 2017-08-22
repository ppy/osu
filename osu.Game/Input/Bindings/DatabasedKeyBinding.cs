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
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

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

        [Indexed]
        [Column("Action")]
        public int IntAction
        {
            get { return (int)Action; }
            set { Action = value; }
        }
    }
}