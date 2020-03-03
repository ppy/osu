// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Overlays;
using osu.Game.Overlays.Home.Friends;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneFriendsOnlineStatusControl : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(FriendsOnlineStatusControl),
            typeof(FriendsOnlineStatusItem),
            typeof(OverlayUpdateStreamControl<>),
            typeof(OverlayUpdateStreamItem<>),
            typeof(FriendsBundle)
        };

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        private FriendsOnlineStatusControl control;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Clear();
            Add(control = new FriendsOnlineStatusControl
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });
        });

        [Test]
        public void Populate()
        {
            AddStep(@"Populate", () => control.Populate(new List<FriendsBundle>
            {
                new FriendsBundle(FriendsOnlineStatus.All, 100),
                new FriendsBundle(FriendsOnlineStatus.Online, 50),
                new FriendsBundle(FriendsOnlineStatus.Offline, 50),
            }));
        }
    }
}
