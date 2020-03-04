// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Overlays;
using osu.Game.Overlays.Home.Friends;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneFriendsOnlineStatusControl : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(FriendsOnlineStatusControl),
            typeof(FriendsOnlineStatusItem),
            typeof(OverlayStreamControl<>),
            typeof(OverlayStreamItem<>),
            typeof(FriendsBundle)
        };

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        private FriendsOnlineStatusControl control;

        [SetUp]
        public void SetUp() => Schedule(() => Child = control = new FriendsOnlineStatusControl
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
        });

        [Test]
        public void Populate()
        {
            AddStep("Populate", () => control.Populate(new List<User>
            {
                new User
                {
                    IsOnline = true
                },
                new User
                {
                    IsOnline = false
                },
                new User
                {
                    IsOnline = false
                }
            }));

            AddAssert("3 users", () => control.Items.FirstOrDefault(item => item.Status == FriendsOnlineStatus.All)?.Count == 3);
            AddAssert("1 online user", () => control.Items.FirstOrDefault(item => item.Status == FriendsOnlineStatus.Online)?.Count == 1);
            AddAssert("2 offline users", () => control.Items.FirstOrDefault(item => item.Status == FriendsOnlineStatus.Offline)?.Count == 2);
        }
    }
}
