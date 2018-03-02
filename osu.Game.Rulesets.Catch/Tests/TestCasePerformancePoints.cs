// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    public class TestCasePerformancePoints : Game.Tests.Visual.TestCasePerformancePoints
    {
        public TestCasePerformancePoints()
            : base(new CatchRuleset())
        {
        }
    }
}
