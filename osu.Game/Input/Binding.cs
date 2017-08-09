using osu.Game.Rulesets;
using OpenTK.Input;
using SQLite.Net.Attributes;
using SQLiteNetExtensions.Attributes;

namespace osu.Game.Input
{
    public class Binding
    {
        [ForeignKey(typeof(RulesetInfo))]
        public int? RulesetID { get; set; }

        [Indexed]
        public int? Variant { get; set; }

        public Key Key { get; set; }

        public int Action { get; set; }
    }
}