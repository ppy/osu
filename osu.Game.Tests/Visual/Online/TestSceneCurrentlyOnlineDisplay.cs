// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Metadata;
using osu.Game.Online.Spectator;
using osu.Game.Overlays;
using osu.Game.Overlays.Dashboard.CurrentlyOnline;
using osu.Game.Tests.Visual.Metadata;
using osu.Game.Tests.Visual.Spectator;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Online
{
    public partial class TestSceneCurrentlyOnlineDisplay : OsuTestScene
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

        private readonly APIUser streamingUser = new APIUser { Id = 2, Username = "Test user" };

        private TestSpectatorClient spectatorClient = null!;
        private TestMetadataClient metadataClient = null!;
        private CurrentlyOnlineDisplay currentlyOnline = null!;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("set up components", () =>
            {
                spectatorClient = new TestSpectatorClient();
                metadataClient = new TestMetadataClient();

                ((DummyAPIAccess)API).HandleRequest = req =>
                {
                    switch (req)
                    {
                        case LookupUsersRequest lookupUsersRequest:
                            var users = lookupUsersRequest.UserIds.Select(id =>
                            {
                                // tests against failed lookups
                                if (id == 13)
                                    return null;

                                return new APIUser
                                {
                                    Id = id,
                                    Username = usernames[id % usernames.Length],
                                };
                            }).ToList();
                            lookupUsersRequest.TriggerSuccess(new GetUsersResponse { Users = users });
                            return true;

                        default:
                            return false;
                    }
                };

                Children = new Drawable[]
                {
                    spectatorClient,
                    metadataClient,
                    new DependencyProvidingContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        CachedDependencies = new (Type, object)[]
                        {
                            (typeof(SpectatorClient), spectatorClient),
                            (typeof(MetadataClient), metadataClient),
                            (typeof(OverlayColourProvider), new OverlayColourProvider(OverlayColourScheme.Purple)),
                        },
                        Child = currentlyOnline = new CurrentlyOnlineDisplay()
                    },
                };
            });
        }

        [Test]
        public void TestBasicDisplay()
        {
            IDisposable token = null!;

            AddStep("Begin watching user presence", () => token = metadataClient.BeginWatchingUserPresence());
            AddStep("Add online user", () => metadataClient.UserPresenceUpdated(streamingUser.Id, new UserPresence { Status = UserStatus.Online, Activity = new UserActivity.ChoosingBeatmap() }));
            AddUntilStep("Panel loaded", () => currentlyOnline.ChildrenOfType<OnlineUserPanel>().FirstOrDefault()?.User.Id == 2);
            AddAssert("Spectate button disabled", () => currentlyOnline.ChildrenOfType<OnlineUserPanel>().First().CanSpectate.Value, () => Is.False);

            AddStep("User began playing", () => metadataClient.UserPresenceUpdated(streamingUser.Id, new UserPresence { Status = UserStatus.Online, Activity = new UserActivity.InSoloGame() }));
            AddAssert("Spectate button enabled", () => currentlyOnline.ChildrenOfType<OnlineUserPanel>().First().CanSpectate.Value, () => Is.True);

            AddStep("User finished playing",
                () => metadataClient.UserPresenceUpdated(streamingUser.Id, new UserPresence { Status = UserStatus.Online, Activity = new UserActivity.ChoosingBeatmap() }));
            AddAssert("Spectate button disabled", () => currentlyOnline.ChildrenOfType<OnlineUserPanel>().First().CanSpectate.Value, () => Is.False);

            AddStep("Remove playing user", () => metadataClient.UserPresenceUpdated(streamingUser.Id, null));
            AddUntilStep("Panel no longer present", () => !currentlyOnline.ChildrenOfType<OnlineUserPanel>().Any());
            AddStep("End watching user presence", () => token.Dispose());
        }

        [Test]
        public void TestUserWasPlayingBeforeWatchingUserPresence()
        {
            IDisposable token = null!;

            AddStep("Begin watching user presence", () => token = metadataClient.BeginWatchingUserPresence());
            AddStep("Add online user", () => metadataClient.UserPresenceUpdated(streamingUser.Id, new UserPresence { Status = UserStatus.Online, Activity = new UserActivity.InSoloGame() }));
            AddUntilStep("Panel loaded", () => currentlyOnline.ChildrenOfType<OnlineUserPanel>().FirstOrDefault()?.User.Id == streamingUser.Id);
            AddAssert("Spectate button enabled", () => currentlyOnline.ChildrenOfType<OnlineUserPanel>().First().CanSpectate.Value, () => Is.True);

            AddStep("User finished playing",
                () => metadataClient.UserPresenceUpdated(streamingUser.Id, new UserPresence { Status = UserStatus.Online, Activity = new UserActivity.ChoosingBeatmap() }));
            AddAssert("Spectate button disabled", () => currentlyOnline.ChildrenOfType<OnlineUserPanel>().First().CanSpectate.Value, () => Is.False);
            AddStep("Remove playing user", () => metadataClient.UserPresenceUpdated(streamingUser.Id, null));
            AddStep("End watching user presence", () => token.Dispose());
        }
    }
}
