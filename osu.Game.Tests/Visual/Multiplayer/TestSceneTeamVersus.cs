// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Extensions;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.TeamVersus;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.OnlinePlay.Lounge;
using osu.Game.Screens.OnlinePlay.Multiplayer;
using osu.Game.Screens.OnlinePlay.Multiplayer.Match;
using osu.Game.Screens.OnlinePlay.Multiplayer.Participants;
using osu.Game.Tests.Resources;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public partial class TestSceneTeamVersus : ScreenTestScene
    {
        private BeatmapManager beatmaps;
        private BeatmapSetInfo importedSet;

        private TestMultiplayerComponents multiplayerComponents;

        private TestMultiplayerClient multiplayerClient => multiplayerComponents.MultiplayerClient;

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            Dependencies.Cache(new RealmRulesetStore(Realm));
            Dependencies.Cache(beatmaps = new BeatmapManager(LocalStorage, Realm, null, audio, Resources, host, Beatmap.Default));
            Dependencies.Cache(Realm);
        }

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("import beatmap", () =>
            {
                beatmaps.Import(TestResources.GetQuickTestBeatmapForImport()).WaitSafely();
                importedSet = beatmaps.GetAllUsableBeatmapSets().First();
            });

            AddStep("load multiplayer", () => LoadScreen(multiplayerComponents = new TestMultiplayerComponents()));
            AddUntilStep("wait for multiplayer to load", () => multiplayerComponents.IsLoaded);
            AddUntilStep("wait for lounge to load", () => this.ChildrenOfType<MultiplayerLoungeSubScreen>().FirstOrDefault()?.IsLoaded == true);
        }

        [Test]
        public void TestCreateWithType()
        {
            createRoom(() => new Room
            {
                Name = { Value = "Test Room" },
                Type = { Value = MatchType.TeamVersus },
                Playlist =
                {
                    new PlaylistItem(beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo)
                    {
                        RulesetID = new OsuRuleset().RulesetInfo.OnlineID
                    }
                }
            });

            AddUntilStep("room type is team vs", () => multiplayerClient.ClientRoom?.Settings.MatchType == MatchType.TeamVersus);
            AddUntilStep("user state arrived", () => multiplayerClient.ClientRoom?.Users.FirstOrDefault()?.MatchState is TeamVersusUserState);
        }

        [Test]
        public void TestChangeTeamsViaButton()
        {
            createRoom(() => new Room
            {
                Name = { Value = "Test Room" },
                Type = { Value = MatchType.TeamVersus },
                Playlist =
                {
                    new PlaylistItem(beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo)
                    {
                        RulesetID = new OsuRuleset().RulesetInfo.OnlineID
                    }
                }
            });

            AddUntilStep("user on team 0", () => (multiplayerClient.ClientRoom?.Users.FirstOrDefault()?.MatchState as TeamVersusUserState)?.TeamID == 0);
            AddStep("add another user", () => multiplayerClient.AddUser(new APIUser { Username = "otheruser", Id = 44 }));

            AddStep("press own button", () =>
            {
                InputManager.MoveMouseTo(multiplayerComponents.ChildrenOfType<TeamDisplay>().First());
                InputManager.Click(MouseButton.Left);
            });
            AddUntilStep("user on team 1", () => (multiplayerClient.ClientRoom?.Users.FirstOrDefault()?.MatchState as TeamVersusUserState)?.TeamID == 1);

            AddStep("press own button again", () => InputManager.Click(MouseButton.Left));
            AddUntilStep("user on team 0", () => (multiplayerClient.ClientRoom?.Users.FirstOrDefault()?.MatchState as TeamVersusUserState)?.TeamID == 0);

            AddStep("press other user's button", () =>
            {
                InputManager.MoveMouseTo(multiplayerComponents.ChildrenOfType<TeamDisplay>().ElementAt(1));
                InputManager.Click(MouseButton.Left);
            });
            AddUntilStep("user still on team 0", () => (multiplayerClient.ClientRoom?.Users.FirstOrDefault()?.MatchState as TeamVersusUserState)?.TeamID == 0);
        }

        [Test]
        public void TestSettingsUpdatedWhenChangingMatchType()
        {
            createRoom(() => new Room
            {
                Name = { Value = "Test Room" },
                Type = { Value = MatchType.HeadToHead },
                Playlist =
                {
                    new PlaylistItem(beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo)
                    {
                        RulesetID = new OsuRuleset().RulesetInfo.OnlineID,
                    }
                }
            });

            AddUntilStep("match type head to head", () => multiplayerClient.ClientAPIRoom?.Type.Value == MatchType.HeadToHead);

            AddStep("change match type", () => multiplayerClient.ChangeSettings(new MultiplayerRoomSettings
            {
                MatchType = MatchType.TeamVersus
            }).WaitSafely());

            AddUntilStep("api room updated to team versus", () => multiplayerClient.ClientAPIRoom?.Type.Value == MatchType.TeamVersus);
        }

        [Test]
        public void TestChangeTypeViaMatchSettings()
        {
            createRoom(() => new Room
            {
                Name = { Value = "Test Room" },
                Playlist =
                {
                    new PlaylistItem(beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo)
                    {
                        RulesetID = new OsuRuleset().RulesetInfo.OnlineID,
                    }
                }
            });

            AddUntilStep("room type is head to head", () => multiplayerClient.ClientRoom?.Settings.MatchType == MatchType.HeadToHead);

            AddUntilStep("team displays are not displaying teams", () => multiplayerComponents.ChildrenOfType<TeamDisplay>().All(d => d.DisplayedTeam == null));

            AddStep("change to team vs", () => multiplayerClient.ChangeSettings(matchType: MatchType.TeamVersus));

            AddUntilStep("room type is team vs", () => multiplayerClient.ClientRoom?.Settings.MatchType == MatchType.TeamVersus);

            AddUntilStep("team displays are displaying teams", () => multiplayerComponents.ChildrenOfType<TeamDisplay>().All(d => d.DisplayedTeam != null));
        }

        private void createRoom(Func<Room> room)
        {
            AddStep("open room", () => multiplayerComponents.ChildrenOfType<LoungeSubScreen>().Single().Open(room()));

            AddUntilStep("wait for room open", () => this.ChildrenOfType<MultiplayerMatchSubScreen>().FirstOrDefault()?.IsLoaded == true);
            AddWaitStep("wait for transition", 2);

            AddUntilStep("create room button enabled", () => this.ChildrenOfType<MultiplayerMatchSettingsOverlay.CreateOrUpdateButton>().Single().Enabled.Value);
            AddStep("create room", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<MultiplayerMatchSettingsOverlay.CreateOrUpdateButton>().Single());
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("wait for join", () => multiplayerClient.RoomJoined);
        }
    }
}
