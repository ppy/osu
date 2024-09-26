// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Setup;
using osuTK.Input;

namespace osu.Game.Tournament.Tests.Screens
{
    public partial class TestSceneBoardImportScreen : TournamentTestScene
    {
        private GridContainer testGrid = null!;

        private readonly Channel testChannel = new Channel();
        private readonly TournamentMatchChatDisplay chatDisplay;

        private TournamentRound testRound = null!;

        private BoardImportScreen importScreen;

        private TournamentUser refereeBot = new TournamentUser
        {
            Username = "RealJuroeBot",
            OnlineID = 114,
        };

        private APIUser oneUser = new APIUser
        {
            Username = "catchDolly",
            Id = 514,
        };

        public TestSceneBoardImportScreen()
        {
            Child = testGrid = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                RowDimensions = new[]
                {
                    new Dimension(GridSizeMode.AutoSize),
                    new Dimension(GridSizeMode.Relative, 0.6f),
                    new Dimension(GridSizeMode.AutoSize),
                    new Dimension(GridSizeMode.Relative, 0.3f),
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        new TournamentSpriteText { Text = "Import screen", Font = OsuFont.Torus.With(size: 20) },
                    },
                    new Drawable[]
                    {
                        importScreen = new BoardImportScreen()
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                    },
                    new Drawable[]
                    {
                        new TournamentSpriteText { Text = "Chat display", Font = OsuFont.Torus.With(size: 20) },
                    },
                    new Drawable[]
                    {
                        chatDisplay = new TournamentMatchChatDisplay
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 200,
                        },
                    },
                },
            };
        }

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("initialize round data", () => Ladder.CurrentMatch.Value.Round.Value = testRound = new TournamentRound
            {
                Referees =
                {
                    refereeBot
                },
            });
        }

        [Test]
        public void TestBoardDisplay()
        {
            AddStep("enable board display", () => testRound.UseBoard.Value = true);

            AddStep("add 16 beatmaps", () => testRound.Beatmaps.AddRange(new[]
            {
                new RoundBeatmap
                {
                    ID = 114514,
                    Mods = "HR",
                    ModIndex = "1",
                    BoardX = 1,
                    BoardY = 1,
                    Beatmap = new TournamentBeatmap
                    {
                        OnlineID = 114514,
                    },
                },
                new RoundBeatmap
                {
                    ID = 1919810,
                    Mods = "HR",
                    ModIndex = "2",
                    BoardX = 2,
                    BoardY = 1,
                    Beatmap = new TournamentBeatmap
                    {
                        OnlineID = 1919810,
                    },
                },
                new RoundBeatmap
                {
                    ID = 1111231,
                    Mods = "HR",
                    ModIndex = "3",
                    BoardX = 3,
                    BoardY = 1,
                    Beatmap = new TournamentBeatmap
                    {
                        OnlineID = 1111231,
                    },
                },
                new RoundBeatmap
                {
                    ID = 114,
                    Mods = "HR",
                    ModIndex = "4",
                    BoardX = 4,
                    BoardY = 1,
                    Beatmap = new TournamentBeatmap
                    {
                        OnlineID = 114,
                    },
                },
                new RoundBeatmap
                {
                    ID = 514,
                    Mods = "FM",
                    ModIndex = "1",
                    BoardX = 1,
                    BoardY = 2,
                    Beatmap = new TournamentBeatmap
                    {
                        OnlineID = 514,
                    },
                },
                new RoundBeatmap
                {
                    ID = 1,
                    Mods = "FM",
                    ModIndex = "2",
                    BoardX = 2,
                    BoardY = 2,
                    Beatmap = new TournamentBeatmap
                    {
                        OnlineID = 1,
                    },
                },
                new RoundBeatmap
                {
                    ID = 2,
                    Mods = "FM",
                    ModIndex = "3",
                    BoardX = 3,
                    BoardY = 2,
                    Beatmap = new TournamentBeatmap
                    {
                        OnlineID = 2,
                    },
                },
                new RoundBeatmap
                {
                    ID = 3,
                    Mods = "FM",
                    ModIndex = "4",
                    BoardX = 4,
                    BoardY = 2,
                    Beatmap = new TournamentBeatmap
                    {
                        OnlineID = 3,
                    },
                },
                new RoundBeatmap
                {
                    ID = 4,
                    Mods = "NM",
                    ModIndex = "1",
                    BoardX = 1,
                    BoardY = 3,
                    Beatmap = new TournamentBeatmap
                    {
                        OnlineID = 4,
                    },
                },
                new RoundBeatmap
                {
                    ID = 5,
                    Mods = "NM",
                    ModIndex = "2",
                    BoardX = 2,
                    BoardY = 3,
                    Beatmap = new TournamentBeatmap
                    {
                        OnlineID = 5,
                    },
                },
                new RoundBeatmap
                {
                    ID = 6,
                    Mods = "NM",
                    ModIndex = "3",
                    BoardX = 3,
                    BoardY = 3,
                    Beatmap = new TournamentBeatmap
                    {
                        OnlineID = 6,
                    },
                },
                new RoundBeatmap
                {
                    ID = 7,
                    Mods = "NM",
                    ModIndex = "4",
                    BoardX = 4,
                    BoardY = 3,
                    Beatmap = new TournamentBeatmap
                    {
                        OnlineID = 7,
                    },
                },
                new RoundBeatmap
                {
                    ID = 8,
                    Mods = "HD",
                    ModIndex = "1",
                    BoardX = 1,
                    BoardY = 4,
                    Beatmap = new TournamentBeatmap
                    {
                        OnlineID = 8,
                    },
                },
                new RoundBeatmap
                {
                    ID = 9,
                    Mods = "HD",
                    ModIndex = "2",
                    BoardX = 2,
                    BoardY = 4,
                    Beatmap = new TournamentBeatmap
                    {
                        OnlineID = 9,
                    },
                },
                new RoundBeatmap
                {
                    ID = 10,
                    Mods = "DT",
                    ModIndex = "1",
                    BoardX = 3,
                    BoardY = 4,
                    Beatmap = new TournamentBeatmap
                    {
                        OnlineID = 10,
                    },
                },
                new RoundBeatmap
                {
                    ID = 11,
                    Mods = "DT",
                    ModIndex = "2",
                    BoardX = 4,
                    BoardY = 4,
                    Beatmap = new TournamentBeatmap
                    {
                        OnlineID = 11,
                    },
                },
            }));

            AddStep("reload import screen", () => testGrid.Content[1] = new Drawable[]
            {
                importScreen = new BoardImportScreen
                {
                    RelativeSizeAxes = Axes.Both,
                }
            });
        }

        [Test]
        public void TestCommandReading()
        {
            AddStep("add messages in advance", () =>
            {
                testChannel.AddNewMessages(new Message(nextMessageId())
                {
                    Sender = refereeBot.ToAPIUser(),
                    Content = "[*] 当前棋盘：",
                });

                testChannel.AddNewMessages(new Message(nextMessageId())
                {
                    Sender = oneUser,
                    Content = "So whats happening???",
                });

                testChannel.AddNewMessages(new Message(nextMessageId())
                {
                    Sender = refereeBot.ToAPIUser(),
                    Content = "[*] 当前棋盘: FM1 (?) FM2 (?) FM3 (?) FM4 (?)",
                });
            });

            AddStep("bind to test channel", () => chatDisplay.Channel.Value = testChannel);

            AddStep("enable fetching from chat", () =>
            {
                InputManager.MoveMouseTo(importScreen.ChildrenOfType<SwitchButton>().ElementAt(0));
                InputManager.Click(MouseButton.Left);
            });

            AddStep("send partial messages", () => testChannel.AddNewMessages(new Message(nextMessageId())
            {
                Sender = refereeBot.ToAPIUser(),
                Content = "[*] 当前棋盘: HR1 (?) HR2 (?) HR3 (?) HR4 (?)"
            }));

            AddStep("try update", () =>
            {
                InputManager.MoveMouseTo(importScreen.ChildrenOfType<RoundedButton>().First(b => b.Text == "Save..."));
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("check board state", () =>
            {
                // TODO
                return true;
            });
        }

        private int messageId;

        private long? nextMessageId() => messageId++;
    }
}
