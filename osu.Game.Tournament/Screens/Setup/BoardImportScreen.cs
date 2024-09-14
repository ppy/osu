// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Platform;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.IPC;
using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Tournament.Screens.Setup
{
    public partial class BoardImportScreen : TournamentScreen
    {
        [Resolved]
        private TournamentGame tournamentGame { get; set; } = null!;

        [Resolved]
        private TournamentSceneManager? sceneManager { get; set; }

        [Resolved]
        private MatchIPCInfo ipc { get; set; } = null!;

        private TournamentRound? round = null!;

        private Container boardContainer = null!;
        private DialogOverlay? overlay;
        private RoundedButton saveButton = null!;

        private bool isUpdateDone = false;

        private List<List<RoundBeatmap>> defCommandList;
        private BindableBool useChat = new BindableBool(false);

        [BackgroundDependencyLoader(true)]
        private void load(Storage storage, OsuColour colours)
        {
            round = LadderInfo.CurrentMatch.Value?.Round.Value;
            defCommandList = new List<List<RoundBeatmap>>();
            LadderInfo.CurrentMatch.Value.PendingMsgs.CollectionChanged += msgOnCollectionChanged;
            useChat.BindValueChanged(_ => fetchAndUpdate());

            AddRangeInternal(new Drawable[]
            {
                new Container
                {
                    Masking = true,
                    CornerRadius = 10,
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(0.8f, 0.9f),
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = colours.GreySeaFoamDark,
                            RelativeSizeAxes = Axes.Both,
                        },
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.4f,
                            RowDimensions = new[]
                            {
                                new Dimension(GridSizeMode.Relative, 0.08f),
                                new Dimension(GridSizeMode.Relative, 0.08f),
                                new Dimension(),
                                new Dimension(GridSizeMode.Relative, 0.2f),
                            },
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    new OsuSpriteText
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Text = "Update your board!",
                                        Font = OsuFont.Default.With(size: 35)
                                    },
                                },
                                new Drawable[]
                                {
                                    new LabelledSwitchButton
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Width = 0.8f,
                                        Label = "Use chat for update",
                                        Current = useChat,
                                    }
                                },
                                new Drawable[]
                                {
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                    },
                                },
                                new Drawable[]
                                {
                                    new FillFlowContainer
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Direction = FillDirection.Vertical,
                                        Spacing = new Vector2(15),
                                        Children = new Drawable[]
                                        {
                                            new RoundedButton
                                            {
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                                Width = 350,
                                                Text = "Import from local file (Unsupported)",
                                                // Action = () => sceneManager?.SetScreen(typeof(BoardFileSelectScreen)),
                                            },
                                            saveButton = new RoundedButton
                                            {
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                                Width = 350,
                                                Text = "Save...",
                                                BackgroundColour = new OsuColour().Pink,
                                                Action = PromptSave
                                            },
                                        }
                                    }
                                }
                            },
                        },
                        boardContainer = new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                    },
                },
                new BackButton
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    State = { Value = Visibility.Visible },
                    Action = () => sceneManager?.SetScreen(typeof(SetupScreen))
                }
            });

            if (round == null)
            {
                overlay = new DialogOverlay();
                overlay.Push(new IPCErrorDialog("Failed to auto detect", "An osu! stable cutting-edge installation could not be auto detected.\nPlease try and manually point to the directory."));
                AddInternal(overlay);
                return;
            }
            updateBoardDisplay();
        }

        private bool parseCommands()
        {
            if (LadderInfo.CurrentMatch.Value == null)
                return false;

            var msg = LadderInfo.CurrentMatch.Value.PendingMsgs;

            foreach (var item in msg)
            {
                BotCommand command = new BotCommand().ParseFromText(item.Content);

                switch (command.Command)
                {
                    case Commands.BoardDefinition:
                        defCommandList.Add(command.DefList);
                        break;

                    default:
                        break;
                }
            }
            // Clear here for refreshing and further follow-up updates.
            msg.Clear();

            return defCommandList.Count >= 4;
        }

        private void msgOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
            => Scheduler.AddOnce(fetchAndUpdate);

        private void fetchAndUpdate()
        {
            if (parseCommands())
            {
                updateBoard();
                updateBoardDisplay();
                saveButton.FlashColour(new OsuColour().Sky, 1000);
            }
        }

        private void updateBoard()
        {
            // No enough commands.
            if (defCommandList.Count < 4)
                return;

            // Y increases from one
            for (int i = 0; i <= 3; i++)
            {
                var item = defCommandList[i];
                for (int j = 0; j <= 3; j++)
                {
                    var map = item[j];
                    var target = LadderInfo.CurrentMatch.Value?.Round.Value?.Beatmaps.FirstOrDefault(p => p.Mods == map.Mods && p.ModIndex == map.ModIndex);
                    if (target != null)
                    {
                        target.BoardX = map.BoardX;
                        target.BoardY = i + 1;
                    }
                }
            }
            isUpdateDone = true;
            defCommandList.Clear();
        }

        protected virtual void PromptSave()
        {
            overlay = new DialogOverlay();

            if (!isUpdateDone)
            {
                if (defCommandList.Count == 0)
                    overlay.Push(new BoardNoUpdateDialog("No need to update", "Your board is unchanged, still up to date!"));
                else
                    overlay.Push(new BoardUpdateWaitingDialog("Some messages are missing", $"We're still waiting for the remaining {4 - defCommandList.Count} messages!"));
                AddInternal(overlay);
            }
            else
            {
                tournamentGame.SaveChanges();
                overlay.Push(new BoardUpdateSuccessDialog("Done!", "Your board is updated successfully. Remember to refresh your board view!"));
                AddInternal(overlay);
            }
        }

        private void updateBoardDisplay()
        {
            for (int i = 1; i <= 4; i++)
            {
                for (int j = 1; j <= 4; j++)
                {
                    var nextMap = round.Beatmaps.FirstOrDefault(p => (p.Mods != "EX" && p.BoardX == j && p.BoardY == i));
                    if (nextMap != null)
                    {
                        boardContainer.Add(new BoardBeatmapPanel(nextMap.Beatmap, nextMap.Mods, nextMap.ModIndex)
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            X = -200 + j * 160,
                            Y = -400 + i * 160,
                        });
                    }
                    else
                    {
                        // TODO: Do we need to add a placeholder here?
                    }
                }
            }
        }
    }
}
