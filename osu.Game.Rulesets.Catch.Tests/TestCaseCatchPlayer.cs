// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    public class TestCaseCatchPlayer : Game.Tests.Visual.TestCasePlayer
    {
        public TestCaseCatchPlayer() : base(new CatchRuleset())
        {
        }
    }
}
