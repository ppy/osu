// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Profile.Header.Components;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public class TestSceneUserProfilePreviousUsernames : OsuTestScene
    {
        [Resolved]
        private IAPIProvider api { get; set; }

        private GetUserRequest request;
        private PreviousUsernames container;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            request?.Cancel();

            Child = container = new PreviousUsernames
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };
        });

        [Test]
        public void TestOffline()
        {
            AddAssert("Is Hidden", () => container?.Alpha == 0);

            AddStep("1 username", () => container.User.Value = users[0]);
            AddUntilStep("Is visible", () => container?.Alpha == 1);

            AddStep("2 usernames", () => container.User.Value = users[1]);
            AddUntilStep("Is visible", () => container?.Alpha == 1);

            AddStep("3 usernames", () => container.User.Value = users[2]);
            AddUntilStep("Is visible", () => container?.Alpha == 1);

            AddStep("4 usernames", () => container.User.Value = users[3]);
            AddUntilStep("Is visible", () => container?.Alpha == 1);

            AddStep("No username", () => container.User.Value = users[4]);
            AddUntilStep("Is hidden", () => container?.Alpha == 0);

            AddStep("Null user", () => container.User.Value = users[5]);
            AddUntilStep("Is hidden", () => container?.Alpha == 0);
        }

        [Test]
        public void TestOnline()
        {
            AddAssert("Is Hidden", () => container?.Alpha == 0);

            AddStep("Create request", () =>
            {
                request = new GetUserRequest(1777162);
                request.Success += u => container.User.Value = u;
                api?.Queue(request);
            });

            AddUntilStep("Is visible", () => container?.Alpha == 1);
        }

        private static readonly User[] users =
        {
            new User { Id = 1, PreviousUsernames = new[] { "username1" } },
            new User { Id = 2, PreviousUsernames = new[] { "longusername", "longerusername" } },
            new User { Id = 3, PreviousUsernames = new[] { "test", "angelsim", "verylongusername" } },
            new User { Id = 4, PreviousUsernames = new[] { "ihavenoidea", "howcani", "makethistext", "anylonger" } },
            new User { Id = 5, PreviousUsernames = Array.Empty<string>() },
            null
        };

        protected override void Dispose(bool isDisposing)
        {
            request?.Cancel();
            base.Dispose(isDisposing);
        }
    }
}
