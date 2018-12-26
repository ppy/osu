// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Screens.Multi;
using osu.Game.Screens.Multi.Lounge;
using osu.Game.Screens.Multi.Lounge.Components;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseMultiScreen : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(Multiplayer),
            typeof(LoungeSubScreen),
            typeof(FilterControl)
        };

        public TestCaseMultiScreen()
        {
            Multiplayer multi = new Multiplayer();

            AddStep(@"show", () => Add(multi));
            AddWaitStep(5);
            AddStep(@"exit", multi.Exit);
        }
    }
}
