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
            get { return Keys.ToString(); }
            private set { Keys = value; }
        }

        [Column("Action")]
        public new int Action
        {
            get { return base.Action; }
            private set { base.Action = value; }
        }
    }
}