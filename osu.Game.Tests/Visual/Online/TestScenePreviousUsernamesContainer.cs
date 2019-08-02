// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Overlays.Profile.Header;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public class TestScenePreviousUsernamesContainer : OsuTestScene
    {
        public TestScenePreviousUsernamesContainer()
        {
            Bindable<User> user = new Bindable<User>();

            Child = new PreviousUsernamesContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                User = { BindTarget = user },
            };

            AddStep("single username", () => user.Value = new User
            {
                PreviousUsernames = new[] { "username1" },
            });

            AddStep("two usernames", () => user.Value = new User
            {
                PreviousUsernames = new[] { "longusername", "longerusername" },
            });

            AddStep("three usernames", () => user.Value = new User
            {
                PreviousUsernames = new[] { "test", "angelsim", "verylongusername" },
            });

            AddStep("four usernames", () => user.Value = new User
            {
                PreviousUsernames = new[] { "ihavenoidea", "howcani", "makethistext", "anylonger" },
            });

            AddStep("many usernames", () => user.Value = new User
            {
                PreviousUsernames = new[] { "ihavenoidea", "howcani", "makethistext", "anylonger", "but", "ican", "try", "tomake", "this" },
            });

            AddStep("no username", () => user.Value = new User
            {
                PreviousUsernames = new string[0],
            });

            AddStep("null user", () => user.Value = null);
        }
    }
}
