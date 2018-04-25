// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Osu.Tests
{
    [TestFixture]
    public class TestCaseEditor : EditorTestCase
    {
        public TestCaseEditor()
            : base(new OsuRuleset())
        {
        }
    }
}
