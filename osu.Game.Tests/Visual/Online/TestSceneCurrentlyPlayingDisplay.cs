// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Database;
using osu.Game.Online.Spectator;
using osu.Game.Overlays.Dashboard;
using osu.Game.Tests.Visual.Gameplay;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneCurrentlyPlayingDisplay : OsuTestScene
    {
        [Cached(typeof(SpectatorStreamingClient))]
        private TestSceneSpectator.TestSpectatorStreamingClient testSpectatorStreamingClient = new TestSceneSpectator.TestSpectatorStreamingClient();

        private CurrentlyPlayingDisplay currentlyPlaying;

        [Cached(typeof(UserLookupCache))]
        private UserLookupCache lookupCache = new TestUserLookupCache();

        private Container nestedContainer;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("add streaming client", () =>
            {
                nestedContainer?.Remove(testSpectatorStreamingClient);
                Remove(lookupCache);

                Children = new Drawable[]
                {
                    lookupCache,
                    nestedContainer = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            testSpectatorStreamingClient,
                            currentlyPlaying = new CurrentlyPlayingDisplay
                            {
                                RelativeSizeAxes = Axes.Both,
                            }
                        }
                    },
                };
            });

            AddStep("Reset players", () => testSpectatorStreamingClient.PlayingUsers.Clear());
        }

        [Test]
        public void TestBasicDisplay()
        {
            AddStep("Add playing user", () => testSpectatorStreamingClient.PlayingUsers.Add(2));
            AddUntilStep("Panel loaded", () => currentlyPlaying.ChildrenOfType<UserGridPanel>()?.FirstOrDefault()?.User.Id == 2);
            AddStep("Remove playing user", () => testSpectatorStreamingClient.PlayingUsers.Remove(2));
            AddUntilStep("Panel no longer present", () => !currentlyPlaying.ChildrenOfType<UserGridPanel>().Any());
        }

        internal class TestUserLookupCache : UserLookupCache
        {
            private static readonly string[] usernames =
            {
                "fieryrage",
                "Kerensa",
                "MillhioreF",
                "Player01",
                "smoogipoo",
                "Ephemeral",
                "BTMC",
                "Cilvery",
                "m980",
                "HappyStick",
                "LittleEndu",
                "frenzibyte",
                "Zallius",
                "BanchoBot",
                "rocketminer210",
                "pishifat"
            };

            protected override Task<User> ComputeValueAsync(int lookup, CancellationToken token = default)
                => Task.FromResult(new User
                {
                    Id = lookup,
                    Username = usernames[lookup % usernames.Length],
                });
        }
    }
}
