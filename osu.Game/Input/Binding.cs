// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets;
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

        [Column("Keys")]
        public string KeysString
        {
            get { return Keys.ToString(); }
            set { Keys = value; }
        }

        [Ignore]
        public KeyCombination Keys { get; private set; }

        public int Action { get; private set; }

        public Binding()
        {

        }

        public Binding(KeyCombination keys, object action)
        {
            Keys = keys;
            Action = (int)action;
        }

        public virtual T GetAction<T>() => (T)(object)Action;

        public override string ToString() => $"{KeysString}=>{Action}";
    }
}