// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace osu.Game.Modes
{
    public static class RulesetCollection
    {
        private static readonly ConcurrentDictionary<int, Ruleset> available_rulesets = new ConcurrentDictionary<int, Ruleset>();

        public static void Register(Type type)
        {
            Ruleset ruleset = Activator.CreateInstance(type) as Ruleset;

            if (ruleset == null)
                return;

            available_rulesets.TryAdd(available_rulesets.Count, ruleset);
        }

        public static Ruleset GetRuleset(PlayMode playMode)
        {
            return GetRuleset((int)playMode);
        }

        public static Ruleset GetRuleset(int rulesetId)
        {
            Ruleset ruleset;
            
            if (!available_rulesets.TryGetValue(rulesetId, out ruleset))
                throw new InvalidOperationException($"Ruleset id {rulesetId} doesn't exist. How did you trigger this?");

            return ruleset;
        }

        public static IEnumerable<Ruleset> AllRulesets => available_rulesets.Values;
    }
}
