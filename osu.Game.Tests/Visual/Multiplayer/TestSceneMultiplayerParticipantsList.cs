// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online;
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
        [SetUp]
        public new void Setup() => Schedule(createNewParticipantsList);

        [Test]
        public void TestAddUser()
        {
            AddAssert("one unique panel", () => this.ChildrenOfType<ParticipantPanel>().Select(p => p.User).Distinct().Count() == 1);

            AddStep("add user", () => Client.AddUser(new User
            {
                Id = 3,
                Username = "Second",
                CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
            }));

            AddAssert("two unique panels", () => this.ChildrenOfType<ParticipantPanel>().Select(p => p.User).Distinct().Count() == 2);
        }

        [Test]
        public void TestAddNullUser()
        {
            AddAssert("one unique panel", () => this.ChildrenOfType<ParticipantPanel>().Select(p => p.User).Distinct().Count() == 1);

            AddStep("add non-resolvable user", () => Client.AddNullUser(-3));

            AddUntilStep("two unique panels", () => this.ChildrenOfType<ParticipantPanel>().Select(p => p.User).Distinct().Count() == 2);
        }

        [Test]
        public void TestRemoveUser()
        {
            User secondUser = null;

            AddStep("add a user", () =>
            {
                Client.AddUser(secondUser = new User
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
            AddStep("recreate list", createNewParticipantsList);
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
                var progress = this.ChildrenOfType<ParticipantPanel>().Single().User.BeatmapAvailability.DownloadProgress ?? 0;
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
        public void TestCrownChangesStateWhenHostTransferred()
        {
            AddStep("add user", () => Client.AddUser(new User
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
        public void TestManyUsers()
        {
            AddStep("add many users", () =>
            {
                for (int i = 0; i < 20; i++)
                {
                    Client.AddUser(new User
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
                Client.AddUser(new User
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
        }

        private void createNewParticipantsList()
        {
            Child = new ParticipantsList { Anchor = Anchor.Centre, Origin = Anchor.Centre, RelativeSizeAxes = Axes.Y, Size = new Vector2(380, 0.7f) };
        }

        private void checkProgressBarVisibility(bool visible) =>
            AddUntilStep($"progress bar {(visible ? "is" : "is not")}visible", () =>
                this.ChildrenOfType<ProgressBar>().Single().IsPresent == visible);
    }
}
