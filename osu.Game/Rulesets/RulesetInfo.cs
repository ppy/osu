// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using SQLite.Net.Attributes;

namespace osu.Game.Rulesets
{
    public class RulesetInfo : IEquatable<RulesetInfo>
    {
        [PrimaryKey, AutoIncrement]
        public int? ID { get; set; }

        [Indexed(Unique = true)]
        public string Name { get; set; }

        [Indexed(Unique = true)]
        public string InstantiationInfo { get; set; }

        [Indexed]
        public bool Available { get; set; }

        public virtual Ruleset CreateInstance() => (Ruleset)Activator.CreateInstance(Type.GetType(InstantiationInfo), this);

        public bool Equals(RulesetInfo other) => other != null && ID == other.ID && Available == other.Available && Name == other.Name && InstantiationInfo == other.InstantiationInfo;
    }
}
