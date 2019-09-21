// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
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
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(PreviousUsernames)
        };

        [Resolved]
        private IAPIProvider api { get; set; }

        private readonly Bindable<User> user = new Bindable<User>();

        public TestSceneUserProfilePreviousUsernames()
        {
            Child = new PreviousUsernames
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                User = { BindTarget = user },
            };

            User[] users =
            {
                new User { PreviousUsernames = new[] { "username1" } },
                new User { PreviousUsernames = new[] { "longusername", "longerusername" } },
                new User { PreviousUsernames = new[] { "test", "angelsim", "verylongusername" } },
                new User { PreviousUsernames = new[] { "ihavenoidea", "howcani", "makethistext", "anylonger" } },
                new User { PreviousUsernames = new string[0] },
                null
            };

            AddStep("single username", () => user.Value = users[0]);
            AddStep("two usernames", () => user.Value = users[1]);
            AddStep("three usernames", () => user.Value = users[2]);
            AddStep("four usernames", () => user.Value = users[3]);
            AddStep("no username", () => user.Value = users[4]);
            AddStep("null user", () => user.Value = users[5]);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddStep("online user (Angelsim)", () =>
            {
                var request = new GetUserRequest(1777162);
                request.Success += user => this.user.Value = user;
                api.Queue(request);
            });
        }
    }
}
