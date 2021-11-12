// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens.OnlinePlay.Multiplayer.Participants;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMultiplayerParticipantsList : MultiplayerTestScene
    {
        [SetUpSteps]
        public void SetupSteps()
        {
            createNewParticipantsList();
        }

        [Test]
        public void TestAddUser()
        {
            AddAssert("one unique panel", () => this.ChildrenOfType<ParticipantPanel>().Select(p => p.User).Distinct().Count() == 1);

            AddStep("add user", () => Client.AddUser(new APIUser
            {
                Id = 3,
                Username = "Second",
                CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
            }));

            AddAssert("two unique panels", () => this.ChildrenOfType<ParticipantPanel>().Select(p => p.User).Distinct().Count() == 2);
        }

        [Test]
        public void TestAddUnresolvedUser()
        {
            AddAssert("one unique panel", () => this.ChildrenOfType<ParticipantPanel>().Select(p => p.User).Distinct().Count() == 1);

            AddStep("add non-resolvable user", () => Client.TestAddUnresolvedUser());
            AddAssert("null user added", () => Client.Room.AsNonNull().Users.Count(u => u.User == null) == 1);

            AddUntilStep("two unique panels", () => this.ChildrenOfType<ParticipantPanel>().Select(p => p.User).Distinct().Count() == 2);

            AddStep("kick null user", () => this.ChildrenOfType<ParticipantPanel>().Single(p => p.User.User == null)
                                                .ChildrenOfType<ParticipantPanel.KickButton>().Single().TriggerClick());

            AddAssert("null user kicked", () => Client.Room.AsNonNull().Users.Count == 1);
        }

        [Test]
        public void TestRemoveUser()
        {
            APIUser secondUser = null;

            AddStep("add a user", () =>
            {
                Client.AddUser(secondUser = new APIUser
                {
                    Id = 3,
                    Username = "Second",
                    CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
                });
            });

            AddStep("remove host", () => Client.RemoveUser(API.LocalUser.Value));

            AddAssert("single panel is for second user", () => this.ChildrenOfType<ParticipantPanel>().Single().User.User == secondUser);
        }

        [Test]
        public void TestGameStateHasPriorityOverDownloadState()
        {
            AddStep("set to downloading map", () => Client.ChangeBeatmapAvailability(BeatmapAvailability.Downloading(0)));
            checkProgressBarVisibility(true);

            AddStep("make user ready", () => Client.ChangeState(MultiplayerUserState.Results));
            checkProgressBarVisibility(false);
            AddUntilStep("ready mark visible", () => this.ChildrenOfType<StateDisplay>().Single().IsPresent);

            AddStep("make user ready", () => Client.ChangeState(MultiplayerUserState.Idle));
            checkProgressBarVisibility(true);
        }

        [Test]
        public void TestCorrectInitialState()
        {
            AddStep("set to downloading map", () => Client.ChangeBeatmapAvailability(BeatmapAvailability.Downloading(0)));
            createNewParticipantsList();
            checkProgressBarVisibility(true);
        }

        [Test]
        public void TestBeatmapDownloadingStates()
        {
            AddStep("set to no map", () => Client.ChangeBeatmapAvailability(BeatmapAvailability.NotDownloaded()));
            AddStep("set to downloading map", () => Client.ChangeBeatmapAvailability(BeatmapAvailability.Downloading(0)));

            checkProgressBarVisibility(true);

            AddRepeatStep("increment progress", () =>
            {
                float progress = this.ChildrenOfType<ParticipantPanel>().Single().User.BeatmapAvailability.DownloadProgress ?? 0;
                Client.ChangeBeatmapAvailability(BeatmapAvailability.Downloading(progress + RNG.NextSingle(0.1f)));
            }, 25);

            AddAssert("progress bar increased", () => this.ChildrenOfType<ProgressBar>().Single().Current.Value > 0);

            AddStep("set to importing map", () => Client.ChangeBeatmapAvailability(BeatmapAvailability.Importing()));
            checkProgressBarVisibility(false);

            AddStep("set to available", () => Client.ChangeBeatmapAvailability(BeatmapAvailability.LocallyAvailable()));
        }

        [Test]
        public void TestToggleReadyState()
        {
            AddAssert("ready mark invisible", () => !this.ChildrenOfType<StateDisplay>().Single().IsPresent);

            AddStep("make user ready", () => Client.ChangeState(MultiplayerUserState.Ready));
            AddUntilStep("ready mark visible", () => this.ChildrenOfType<StateDisplay>().Single().IsPresent);

            AddStep("make user idle", () => Client.ChangeState(MultiplayerUserState.Idle));
            AddUntilStep("ready mark invisible", () => !this.ChildrenOfType<StateDisplay>().Single().IsPresent);
        }

        [Test]
        public void TestToggleSpectateState()
        {
            AddStep("make user spectating", () => Client.ChangeState(MultiplayerUserState.Spectating));
            AddStep("make user idle", () => Client.ChangeState(MultiplayerUserState.Idle));
        }

        [Test]
        public void TestCrownChangesStateWhenHostTransferred()
        {
            AddStep("add user", () => Client.AddUser(new APIUser
            {
                Id = 3,
                Username = "Second",
                CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
            }));

            AddUntilStep("first user crown visible", () => this.ChildrenOfType<ParticipantPanel>().ElementAt(0).ChildrenOfType<SpriteIcon>().First().Alpha == 1);
            AddUntilStep("second user crown hidden", () => this.ChildrenOfType<ParticipantPanel>().ElementAt(1).ChildrenOfType<SpriteIcon>().First().Alpha == 0);

            AddStep("make second user host", () => Client.TransferHost(3));

            AddUntilStep("first user crown hidden", () => this.ChildrenOfType<ParticipantPanel>().ElementAt(0).ChildrenOfType<SpriteIcon>().First().Alpha == 0);
            AddUntilStep("second user crown visible", () => this.ChildrenOfType<ParticipantPanel>().ElementAt(1).ChildrenOfType<SpriteIcon>().First().Alpha == 1);
        }

        [Test]
        public void TestKickButtonOnlyPresentWhenHost()
        {
            AddStep("add user", () => Client.AddUser(new APIUser
            {
                Id = 3,
                Username = "Second",
                CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
            }));

            AddUntilStep("kick buttons visible", () => this.ChildrenOfType<ParticipantPanel.KickButton>().Count(d => d.IsPresent) == 1);

            AddStep("make second user host", () => Client.TransferHost(3));

            AddUntilStep("kick buttons not visible", () => this.ChildrenOfType<ParticipantPanel.KickButton>().Count(d => d.IsPresent) == 0);

            AddStep("make local user host again", () => Client.TransferHost(API.LocalUser.Value.Id));

            AddUntilStep("kick buttons visible", () => this.ChildrenOfType<ParticipantPanel.KickButton>().Count(d => d.IsPresent) == 1);
        }

        [Test]
        public void TestKickButtonKicks()
        {
            AddStep("add user", () => Client.AddUser(new APIUser
            {
                Id = 3,
                Username = "Second",
                CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
            }));

            AddStep("kick second user", () => this.ChildrenOfType<ParticipantPanel.KickButton>().Single(d => d.IsPresent).TriggerClick());

            AddAssert("second user kicked", () => Client.Room?.Users.Single().UserID == API.LocalUser.Value.Id);
        }

        [Test]
        public void TestManyUsers()
        {
            AddStep("add many users", () =>
            {
                for (int i = 0; i < 20; i++)
                {
                    Client.AddUser(new APIUser
                    {
                        Id = i,
                        Username = $"User {i}",
                        RulesetsStatistics = new Dictionary<string, UserStatistics>
                        {
                            {
                                Ruleset.Value.ShortName,
                                new UserStatistics { GlobalRank = RNG.Next(1, 100000), }
                            }
                        },
                        CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
                    });

                    Client.ChangeUserState(i, (MultiplayerUserState)RNG.Next(0, (int)MultiplayerUserState.Results + 1));

                    if (RNG.NextBool())
                    {
                        var beatmapState = (DownloadState)RNG.Next(0, (int)DownloadState.LocallyAvailable + 1);

                        switch (beatmapState)
                        {
                            case DownloadState.NotDownloaded:
                                Client.ChangeUserBeatmapAvailability(i, BeatmapAvailability.NotDownloaded());
                                break;

                            case DownloadState.Downloading:
                                Client.ChangeUserBeatmapAvailability(i, BeatmapAvailability.Downloading(RNG.NextSingle()));
                                break;

                            case DownloadState.Importing:
                                Client.ChangeUserBeatmapAvailability(i, BeatmapAvailability.Importing());
                                break;
                        }
                    }
                }
            });
        }

        [Test]
        public void TestUserWithMods()
        {
            AddStep("add user", () =>
            {
                Client.AddUser(new APIUser
                {
                    Id = 0,
                    Username = "User 0",
                    RulesetsStatistics = new Dictionary<string, UserStatistics>
                    {
                        {
                            Ruleset.Value.ShortName,
                            new UserStatistics { GlobalRank = RNG.Next(1, 100000), }
                        }
                    },
                    CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
                });

                Client.ChangeUserMods(0, new Mod[]
                {
                    new OsuModHardRock(),
                    new OsuModDifficultyAdjust { ApproachRate = { Value = 1 } }
                });
            });

            for (var i = MultiplayerUserState.Idle; i < MultiplayerUserState.Results; i++)
            {
                var state = i;
                AddStep($"set state: {state}", () => Client.ChangeUserState(0, state));
            }

            AddStep("set state: downloading", () => Client.ChangeUserBeatmapAvailability(0, BeatmapAvailability.Downloading(0)));

            AddStep("set state: locally available", () => Client.ChangeUserBeatmapAvailability(0, BeatmapAvailability.LocallyAvailable()));
        }

        [Test]
        public void TestModOverlap()
        {
            AddStep("add dummy mods", () =>
            {
                Client.ChangeUserMods(new Mod[]
                {
                    new OsuModNoFail(),
                    new OsuModDoubleTime()
                });
            });

            AddStep("add user with mods", () =>
            {
                Client.AddUser(new APIUser
                {
                    Id = 0,
                    Username = "Baka",
                    RulesetsStatistics = new Dictionary<string, UserStatistics>
                    {
                        {
                            Ruleset.Value.ShortName,
                            new UserStatistics { GlobalRank = RNG.Next(1, 100000), }
                        }
                    },
                    CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
                });
                Client.ChangeUserMods(0, new Mod[]
                {
                    new OsuModHardRock(),
                    new OsuModDoubleTime()
                });
            });

            AddStep("set 0 ready", () => Client.ChangeState(MultiplayerUserState.Ready));

            AddStep("set 1 spectate", () => Client.ChangeUserState(0, MultiplayerUserState.Spectating));

            // Have to set back to idle due to status priority.
            AddStep("set 0 no map, 1 ready", () =>
            {
                Client.ChangeState(MultiplayerUserState.Idle);
                Client.ChangeBeatmapAvailability(BeatmapAvailability.NotDownloaded());
                Client.ChangeUserState(0, MultiplayerUserState.Ready);
            });

            AddStep("set 0 downloading", () => Client.ChangeBeatmapAvailability(BeatmapAvailability.Downloading(0)));

            AddStep("set 0 spectate", () => Client.ChangeUserState(0, MultiplayerUserState.Spectating));

            AddStep("make both default", () =>
            {
                Client.ChangeBeatmapAvailability(BeatmapAvailability.LocallyAvailable());
                Client.ChangeUserState(0, MultiplayerUserState.Idle);
                Client.ChangeState(MultiplayerUserState.Idle);
            });
        }

        private void createNewParticipantsList()
        {
            ParticipantsList participantsList = null;

            AddStep("create new list", () => Child = participantsList = new ParticipantsList
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Y,
                Size = new Vector2(380, 0.7f)
            });

            AddUntilStep("wait for list to load", () => participantsList.IsLoaded);
        }

        private void checkProgressBarVisibility(bool visible) =>
            AddUntilStep($"progress bar {(visible ? "is" : "is not")}visible", () =>
                this.ChildrenOfType<ProgressBar>().Single().IsPresent == visible);
    }
}
