// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Game.Screens.Multi;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseMultiScreen : OsuTestCase
    {
        public TestCaseMultiScreen()
        {
            Multiplayer multi = new Multiplayer();

            AddStep(@"show", () => Add(multi));
            AddWaitStep(5);
            AddStep(@"exit", multi.Exit);
        }
    }
}
