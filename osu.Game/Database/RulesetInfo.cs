// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Rulesets;
using SQLite.Net.Attributes;

namespace osu.Game.Database
{
    public class RulesetInfo
    {
        [PrimaryKey, AutoIncrement]
        public int? ID { get; set; }

        [Indexed(Unique = true)]
        public string Name { get; set; }

        [Indexed(Unique = true)]
        public string InstantiationInfo { get; set; }

        [Indexed]
        public bool Available { get; set; }

        public Ruleset CreateInstance() => (Ruleset)Activator.CreateInstance(Type.GetType(InstantiationInfo));
    }
}