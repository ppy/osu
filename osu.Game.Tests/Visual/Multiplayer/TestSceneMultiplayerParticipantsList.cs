// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.OnlinePlay.Multiplayer.Participants;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMultiplayerParticipantsList : MultiplayerTestScene
    {
        [SetUp]
        public new void Setup() => Schedule(() =>
        {
            Child = new ParticipantsList
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Y,
                Size = new Vector2(380, 0.7f)
            };
        });

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
                        CurrentModeRank = RNG.Next(1, 100000),
                        CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
                    });

                    Client.ChangeUserState(i, (MultiplayerUserState)RNG.Next(0, (int)MultiplayerUserState.Results + 1));
                }
            });
        }
    }
}
