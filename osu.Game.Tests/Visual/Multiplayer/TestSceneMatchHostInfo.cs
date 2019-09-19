// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Screens.Multi.Match.Components;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMatchHostInfo : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(HostInfo)
        };

        private readonly Bindable<User> host = new Bindable<User>(new User { Username = "SomeHost" });

        public TestSceneMatchHostInfo()
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
