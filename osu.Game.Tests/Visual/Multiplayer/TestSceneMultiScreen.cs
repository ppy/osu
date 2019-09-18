// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Screens.Multi.Lounge;
using osu.Game.Screens.Multi.Lounge.Components;

namespace osu.Game.Tests.Visual.Multiplayer
{
    [TestFixture]
    public class TestSceneMultiScreen : ScreenTestScene
    {
        protected override bool UseOnlineAPI => true;

        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(Screens.Multi.Multiplayer),
            typeof(LoungeSubScreen),
            typeof(FilterControl)
        };

        public TestSceneMultiScreen()
        {
            Screens.Multi.Multiplayer multi = new Screens.Multi.Multiplayer();

            AddStep(@"show", () => LoadScreen(multi));
        }
    }
}
