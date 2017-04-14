// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Modes
{
    /// <summary>
    /// Todo: All of this needs to be moved to a RulesetDatabase.
    /// </summary>
    public static class RulesetCollection
    {
        private static readonly ConcurrentDictionary<int, Ruleset> available_rulesets = new ConcurrentDictionary<int, Ruleset>();

        public static void Register(Type type)
        {
            var ruleset = Activator.CreateInstance(type) as Ruleset;

            if (ruleset == null)
                return;

            available_rulesets.TryAdd(available_rulesets.Count, ruleset);
        }

        public static Ruleset GetRuleset(int rulesetId)
        {
            Ruleset ruleset;

            if (!available_rulesets.TryGetValue(rulesetId, out ruleset))
                return null;

            return ruleset;
        }

        public static int GetId(Ruleset ruleset) => available_rulesets.First(kvp => kvp.Value == ruleset).Key;

        public static IEnumerable<Ruleset> AllRulesets => available_rulesets.Values;
    }
}
