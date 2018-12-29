// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
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

        private readonly Bindable<User> host = new Bindable<User>(new User { Username = "SomeHost" });

        public TestCaseMatchHostInfo()
        {
            HostInfo hostInfo;

            Child = hostInfo = new HostInfo
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            };

            hostInfo.Host.BindTo(host);
        }
    }
}
