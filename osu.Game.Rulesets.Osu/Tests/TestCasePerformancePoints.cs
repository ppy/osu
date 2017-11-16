// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestCasePerformancePoints : osu.Game.Tests.Visual.TestCasePerformancePoints
    {
        public TestCasePerformancePoints()
            : base(new OsuRuleset(new RulesetInfo()))
        {
        }
    }
}
