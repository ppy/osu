// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Database;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Spectator;
using osu.Game.Overlays.Dashboard;
using osu.Game.Tests.Visual.Spectator;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneCurrentlyPlayingDisplay : OsuTestScene
    {
        private readonly APIUser streamingUser = new APIUser { Id = 2, Username = "Test user" };

        private TestSpectatorClient spectatorClient;
        private CurrentlyPlayingDisplay currentlyPlaying;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("add streaming client", () =>
            {
                spectatorClient = new TestSpectatorClient();
                var lookupCache = new TestUserLookupCache();

                Children = new Drawable[]
                {
                    lookupCache,
                    spectatorClient,
                    new DependencyProvidingContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        CachedDependencies = new (Type, object)[]
                        {
                            (typeof(SpectatorClient), spectatorClient),
                            (typeof(UserLookupCache), lookupCache)
                        },
                        Child = currentlyPlaying = new CurrentlyPlayingDisplay
                        {
                            RelativeSizeAxes = Axes.Both,
                        }
                    },
                };
            });
        }

        [Test]
        public void TestBasicDisplay()
        {
            AddStep("Add playing user", () => spectatorClient.StartPlay(streamingUser.Id, 0));
            AddUntilStep("Panel loaded", () => currentlyPlaying.ChildrenOfType<UserGridPanel>()?.FirstOrDefault()?.User.Id == 2);
            AddStep("Remove playing user", () => spectatorClient.EndPlay(streamingUser.Id));
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

            protected override Task<APIUser> ComputeValueAsync(int lookup, CancellationToken token = default)
            {
                // tests against failed lookups
                if (lookup == 13)
                    return Task.FromResult<APIUser>(null);

                return Task.FromResult(new APIUser
                {
                    Id = lookup,
                    Username = usernames[lookup % usernames.Length],
                });
            }
        }
    }
}
