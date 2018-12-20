// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Multi.Match.Components;
using osu.Game.Users;

namespace osu.Game.Tests.Visual
{
    public class TestCaseMatchHostInfo : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(HostInfo)
        };

        public TestCaseMatchHostInfo()
        {
            Child = new HostInfo(new Room { Host = { Value = new User { Username = "ImAHost" }}})
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            };
        }
    }
}
