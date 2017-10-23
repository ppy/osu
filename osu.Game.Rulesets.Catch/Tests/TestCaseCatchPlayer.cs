// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    [Ignore("getting CI working")]
    public class TestCaseCatchPlayer : Game.Tests.Visual.TestCasePlayer
    {
        public TestCaseCatchPlayer() : base(typeof(CatchRuleset))
        {
        }
    }
}
