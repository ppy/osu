// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.Profile.Header.Components;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public partial class TestSceneUserProfilePreviousUsernamesDisplay : OsuTestScene
    {
        private PreviousUsernamesDisplay container = null!;
        private OverlayColourProvider colourProvider = null!;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            colourProvider = new OverlayColourProvider(OverlayColourScheme.Pink);
            Child = new DependencyProvidingContainer
            {
                Child = container = new PreviousUsernamesDisplay
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                CachedDependencies = new (Type, object)[] { (typeof(OverlayColourProvider), colourProvider) },
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };
        });

        [Test]
        public void TestVisibility()
        {
            AddAssert("Is Hidden", () => container.Alpha == 0);

            AddStep("1 username", () => container.User.Value = users[0]);
            AddUntilStep("Is visible", () => container.Alpha == 1);

            AddStep("2 usernames", () => container.User.Value = users[1]);
            AddUntilStep("Is visible", () => container.Alpha == 1);

            AddStep("3 usernames", () => container.User.Value = users[2]);
            AddUntilStep("Is visible", () => container.Alpha == 1);

            AddStep("4 usernames", () => container.User.Value = users[3]);
            AddUntilStep("Is visible", () => container.Alpha == 1);

            AddStep("No username", () => container.User.Value = users[4]);
            AddUntilStep("Is hidden", () => container.Alpha == 0);

            AddStep("Null user", () => container.User.Value = users[5]);
            AddUntilStep("Is hidden", () => container.Alpha == 0);
        }

        private static readonly APIUser?[] users =
        {
            new APIUser { Id = 1, PreviousUsernames = new[] { "username1" } },
            new APIUser { Id = 2, PreviousUsernames = new[] { "longusername", "longerusername" } },
            new APIUser { Id = 3, PreviousUsernames = new[] { "test", "angelsim", "verylongusername" } },
            new APIUser { Id = 4, PreviousUsernames = new[] { "ihavenoidea", "howcani", "makethistext", "anylonger" } },
            new APIUser { Id = 5, PreviousUsernames = Array.Empty<string>() },
            null
        };
    }
}
