// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osu.Framework.Threading;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Board.Components;
using osu.Game.Tournament.Screens.Gameplay;
using osu.Game.Tournament.Screens.Gameplay.Components;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Tournament.Screens.Board
{
    public partial class BoardScreen : TournamentMatchScreen
    {
        private FillFlowContainer<FillFlowContainer<BoardBeatmapPanel>> mapFlows = null!;

        [Resolved]
        private TournamentSceneManager? sceneManager { get; set; }

        private WarningBox warning = null!;

        private readonly Bindable<TournamentMatch?> currentMatch = new Bindable<TournamentMatch?>();

        private TeamColour pickColour;
        private ChoiceType pickType;

        private bool havePendingSwap = false;
        private bool refEX = false;
        private bool refWin = false;
        private TeamColour refWinner = TeamColour.None;

        private TeamColour teamWinner = TeamColour.None;

        private OsuButton buttonRedBan = null!;
        private OsuButton buttonBlueBan = null!;
        private OsuButton buttonRedPick = null!;
        private OsuButton buttonBluePick = null!;

        private OsuButton buttonRedProtect = null!;
        private OsuButton buttonBlueProtect = null!;
        private OsuButton buttonRedWin = null!;
        private OsuButton buttonBlueWin = null!;
        private OsuButton buttonRedTrap = null!;
        private OsuButton buttonBlueTrap = null!;

        private OsuButton buttonTrapSwap = null!;

        private OsuButton buttonIndicator = null!;

        private bool useEX = false;
        private bool hasTrap = false;

        private TrapTypeDropdown trapTypeDropdown = null!;
        private Container informationDisplayContainer = null!;
        private Sprite additionalIcon = null!;

        private DrawableTeamPlayerList team1List = null!;
        private DrawableTeamPlayerList team2List = null!;
        private EmptyBox danmakuBox = null!;

        private readonly int sideListHeight = 660;

        private ScheduledDelegate? scheduledScreenChange;

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            currentMatch.BindValueChanged(matchChanged);
            currentMatch.BindTo(LadderInfo.CurrentMatch);

            LadderInfo.UseRefereeCommands.BindValueChanged(refereeChanged);
            LadderInfo.NeedRefereeResponse.BindValueChanged(refereeNeedChanged);

            InternalChildren = new Drawable[]
            {
                new TourneyVideo("mappool")
                {
                    Loop = true,
                    RelativeSizeAxes = Axes.Both,
                },
                new MatchHeader
                {
                    // For OFFC
                    ShowScores = false,
                    ShowRound = false,
                },

                // Box for trap type / display of other info.
                new EmptyBox(cornerRadius: 10)
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.None,
                    Width = 650,
                    Height = 100,
                    Margin = new MarginPadding { Bottom = 12 },
                    Colour = Color4.Black,
                    Alpha = 0.7f,
                },
                new FillFlowContainer
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    RelativeSizeAxes = Axes.None,
                    Position = new Vector2(30, 100),
                    Width = 320,
                    Height = sideListHeight,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        team1List = new DrawableTeamPlayerList(LadderInfo.CurrentMatch.Value?.Team1.Value)
                        {
                            RelativeSizeAxes = Axes.None,
                            Width = 300,
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                        },
                    },
                },
                new FillFlowContainer
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    RelativeSizeAxes = Axes.None,
                    Position = new Vector2(-30, 100),
                    Width = 320,
                    Height = sideListHeight,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        team2List = new DrawableTeamPlayerList(LadderInfo.CurrentMatch.Value?.Team2.Value)
                        {
                            RelativeSizeAxes = Axes.None,
                            Width = 300,
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                        },
                        // A single Box for livestream danmakus.
                        // Wrapped in a container for round corners.
                        danmakuBox = new EmptyBox(cornerRadius: 10)
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            RelativeSizeAxes = Axes.None,
                            Width = 300,
                            Height = sideListHeight - team2List.GetHeight() - 5,
                            Colour = Color4.Black,
                            Alpha = 0.7f,
                        },
                    },
                },
                mapFlows = new FillFlowContainer<FillFlowContainer<BoardBeatmapPanel>>
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    // X = 0,
                    Y = 15,
                    Height = 1f,
                    Width = 900,
                    Padding = new MarginPadding{ Left = 50 },
                    Spacing = new Vector2(10, 10),
                },
                informationDisplayContainer = new Container
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomLeft,
                    Position = new Vector2(-300, 7),
                    Height = 100,
                    Width = 500,
                    Child = new InstructionDisplay(),
                },
                additionalIcon = new Sprite
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomRight,
                    Position = new Vector2(300, -20),
                    Size = new Vector2(85),
                    Texture = textures.Get("Icons/additional-icon"),
                },
                new ControlPanel
                {
                    Children = new Drawable[]
                    {
                        new TournamentSpriteText
                        {
                            Text = "Current Mode"
                        },
                        new LabelledSwitchButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Label = "Ref Commands",
                            Current = LadderInfo.UseRefereeCommands,
                        },
                        new LabelledSwitchButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Label = "Enforce Response",
                            Current = LadderInfo.NeedRefereeResponse,
                        },
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 40,
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    buttonRedProtect = new TourneyButton
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Text = "Red Protect",
                                        BackgroundColour = TournamentGame.COLOUR_RED,
                                        Action = () => setMode(TeamColour.Red, ChoiceType.Protect)
                                    },
                                    buttonBlueProtect = new TourneyButton
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Text = "Blue Protect",
                                        BackgroundColour = TournamentGame.COLOUR_BLUE,
                                        Action = () => setMode(TeamColour.Blue, ChoiceType.Protect)
                                    },
                                }
                            },
                        },
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 40,
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    buttonRedBan = new TourneyButton
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Text = "Red Ban",
                                        BackgroundColour = TournamentGame.COLOUR_RED,
                                        Action = () => setMode(TeamColour.Red, ChoiceType.Ban)
                                    },
                                    buttonBlueBan = new TourneyButton
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Text = "Blue Ban",
                                        BackgroundColour = TournamentGame.COLOUR_BLUE,
                                        Action = () => setMode(TeamColour.Blue, ChoiceType.Ban)
                                    },
                                }
                            },
                        },
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 40,
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    buttonRedPick = new TourneyButton
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Text = "Red Pick",
                                        BackgroundColour = TournamentGame.COLOUR_RED,
                                        Action = () => setMode(TeamColour.Red, ChoiceType.Pick)
                                    },
                                    buttonBluePick = new TourneyButton
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Text = "Blue Pick",
                                        BackgroundColour = TournamentGame.COLOUR_BLUE,
                                        Action = () => setMode(TeamColour.Blue, ChoiceType.Pick)
                                    },
                                }
                            },
                        },
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 40,
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    buttonRedWin = new TourneyButton
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Text = "Red Win",
                                        BackgroundColour = TournamentGame.COLOUR_RED,
                                        Action = () => setMode(TeamColour.Red, ChoiceType.RedWin)
                                    },
                                    buttonBlueWin = new TourneyButton
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Text = "Blue Win",
                                        BackgroundColour = TournamentGame.COLOUR_BLUE,
                                        Action = () => setMode(TeamColour.Blue, ChoiceType.BlueWin)
                                    },
                                }
                            },
                        },
                        new ControlPanel.Spacer(),
                        trapTypeDropdown = new TrapTypeDropdown
                        {
                            LabelText = "Trap type"
                        },
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 40,
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    buttonRedTrap = new TourneyButton
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Text = "Red Trap",
                                        BackgroundColour = TournamentGame.COLOUR_RED,
                                        Action = () => setMode(TeamColour.Red, ChoiceType.Trap)
                                    },
                                    buttonBlueTrap = new TourneyButton
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Text = "Blue Trap",
                                        BackgroundColour = TournamentGame.COLOUR_BLUE,
                                        Action = () => setMode(TeamColour.Blue, ChoiceType.Trap)
                                    },
                                }
                            },
                        },
                        buttonTrapSwap = new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Free Swap",
                            BackgroundColour = Color4.Indigo,
                            Action = () => setMode(TeamColour.Neutral, ChoiceType.Swap)
                        },
                        new ControlPanel.Spacer(),
                        buttonIndicator = new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "EX Indicator",
                            BackgroundColour = Color4.Purple,
                            Colour = Color4.Gray,
                            Action = () => setMode(TeamColour.Neutral, ChoiceType.Neutral)
                        },
                        new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Refresh",
                            BackgroundColour = Color4.Orange,
                            Action = updateDisplay
                        },
                        new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Reset",
                            BackgroundColour = Color4.Orange,
                            Action = reset
                        },
                    },
                }
            };
        }

        private void beatmapChanged(ValueChangedEvent<TournamentBeatmap?> beatmap)
        {
            if (CurrentMatch.Value?.Round.Value == null)
                return;

            int totalBansRequired = CurrentMatch.Value.Round.Value.BanCount.Value * 2;

            if (CurrentMatch.Value.PicksBans.Count(p => p.Type == ChoiceType.Ban) < totalBansRequired)
                return;

            // if bans have already been placed, beatmap changes result in a selection being made automatically
            // if (beatmap.NewValue?.OnlineID > 0)
            //     addForBeatmap(beatmap.NewValue.OnlineID);
        }

        private void matchChanged(ValueChangedEvent<TournamentMatch?> match)
        {
            if (match.OldValue != null)
            {
                match.OldValue.PendingMsgs.CollectionChanged -= msgOnCollectionChanged;
            }
            if (match.NewValue != null)
            {
                match.NewValue.PendingMsgs.CollectionChanged += msgOnCollectionChanged;

                if (match.NewValue.Team1 != null) team1List?.ReloadWithTeam(match.NewValue.Team1.Value);
                if (match.NewValue.Team2 != null && team2List != null)
                {
                    team2List.ReloadWithTeam(match.NewValue.Team2.Value);
                    danmakuBox.ResizeHeightTo(Height = sideListHeight - team2List.GetHeight() - 5, 500, Easing.OutCubic);
                }
            }

            Scheduler.AddOnce(parseCommands);
        }

        private void msgOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
            => Scheduler.AddOnce(parseCommands);

        private void refereeChanged(ValueChangedEvent<bool> enableEvent)
        {
            parseCommands();
        }

        private void parseCommands()
        {
            if (CurrentMatch.Value == null)
                return;

            var msg = CurrentMatch.Value.PendingMsgs;

            foreach (var item in msg)
            {
                BotCommand command = new BotCommand().ParseFromText(item.Content);
                switch (command.Command)
                {
                    case Commands.Panic:
                        informationDisplayContainer.Child = new InstructionDisplay(step: Steps.Halt);
                        informationDisplayContainer.FadeInFromZero(duration: 200, easing: Easing.OutCubic);
                        break;

                    case Commands.EnterEX:
                        refEX = true;
                        updateBottomDisplay();
                        break;

                    case Commands.SetWin:
                        refWin = true;
                        refWinner = command.Team;
                        updateBottomDisplay(bottomOnly: false);
                        break;

                    case Commands.MarkWin:
                        pickColour = command.Team;
                        pickType = command.Team == TeamColour.Red ? ChoiceType.RedWin : ChoiceType.BlueWin;
                        addForBeatmap(command.MapMod);
                        updateBottomDisplay(bottomOnly: false);
                        break;

                    case Commands.Ban:
                        pickColour = command.Team;
                        pickType = ChoiceType.Ban;
                        addForBeatmap(command.MapMod);
                        updateBottomDisplay();
                        break;

                    case Commands.Protect:
                        pickColour = command.Team;
                        pickType = ChoiceType.Protect;
                        addForBeatmap(command.MapMod);
                        updateBottomDisplay();
                        break;

                    case Commands.Pick:
                        pickColour = command.Team;
                        pickType = ChoiceType.Pick;
                        addForBeatmap(command.MapMod);

                        if (CurrentMatch.Value.Round.Value != null)
                        {
                            var map = CurrentMatch.Value.Round.Value.Beatmaps.FirstOrDefault(b => b.Mods + b.ModIndex == command.MapMod);
                            if (map != null && map.Beatmap != null && CurrentMatch.Value.Traps.All(p => p.BeatmapID != map.Beatmap.OnlineID)) updateBottomDisplay();
                        }

                        break;

                    default:
                        break;
                }
            }
            msg.Clear();
        }

        private void setMode(TeamColour colour, ChoiceType choiceType)
        {
            pickColour = colour;
            pickType = choiceType;

            buttonRedBan.Colour = setColour(pickColour == TeamColour.Red && pickType == ChoiceType.Ban);
            buttonBlueBan.Colour = setColour(pickColour == TeamColour.Blue && pickType == ChoiceType.Ban);
            buttonRedPick.Colour = setColour(pickColour == TeamColour.Red && pickType == ChoiceType.Pick);
            buttonBluePick.Colour = setColour(pickColour == TeamColour.Blue && pickType == ChoiceType.Pick);
            buttonRedProtect.Colour = setColour(pickColour == TeamColour.Red && pickType == ChoiceType.Protect);
            buttonBlueProtect.Colour = setColour(pickColour == TeamColour.Blue && pickType == ChoiceType.Protect);
            buttonRedWin.Colour = setColour(pickColour == TeamColour.Red && pickType == ChoiceType.RedWin);
            buttonBlueWin.Colour = setColour(pickColour == TeamColour.Blue && pickType == ChoiceType.BlueWin);
            buttonRedTrap.Colour = setColour(pickColour == TeamColour.Red && pickType == ChoiceType.Trap);
            buttonBlueTrap.Colour = setColour(pickColour == TeamColour.Blue && pickType == ChoiceType.Trap);
            buttonTrapSwap.Colour = setColour(pickType == ChoiceType.Swap);

            buttonTrapSwap.Text = CurrentMatch.Value?.PendingSwaps.Any() ?? false ? @$"Free Swap (Target)" : @$"Free Swap";

            static Color4 setColour(bool active) => active ? Color4.White : Color4.Gray;
            updateBottomDisplay();
        }

        private void refereeNeedChanged(ValueChangedEvent<bool>? _ = null)
            => updateBottomDisplay(bottomOnly: false);

        private void updateBottomDisplay(ValueChangedEvent<bool>? _ = null, bool bottomOnly = true)
        {
            Drawable oldDisplay = informationDisplayContainer.Child;
            Drawable newDisplay;

            if (CurrentMatch.Value == null) return;

            havePendingSwap = CurrentMatch.Value.PendingSwaps.Any();

            var color = pickColour;
            Steps state = Steps.Default;

            if (DetectEX() && !havePendingSwap)
            {
                if (LadderInfo.UseRefereeCommands.Value && LadderInfo.NeedRefereeResponse.Value)
                {
                    state = refEX ? Steps.EX : Steps.Halt;
                }
                else
                {
                    state = Steps.EX;
                }
            }
            else if (DetectWin() && !havePendingSwap)
            {
                if (LadderInfo.UseRefereeCommands.Value && LadderInfo.NeedRefereeResponse.Value)
                {
                    state = refWin && teamWinner == refWinner && teamWinner != TeamColour.None ? Steps.FinalWin : Steps.Halt;
                    color = refWin && teamWinner == refWinner ? teamWinner : pickColour;

                    // Special cases for a draw
                    if (teamWinner == TeamColour.Neutral && refEX)
                    {
                        state = Steps.FinalWin;
                        color = teamWinner;
                    }
                }
                else
                {
                    state = Steps.FinalWin;
                    color = teamWinner;
                }
            }
            else
            {
                switch (pickType)
                {
                    case ChoiceType.Protect:
                        state = Steps.Protect;
                        break;

                    case ChoiceType.Pick:
                        state = Steps.Pick;
                        break;

                    case ChoiceType.Trap:
                        state = Steps.Trap;
                        break;

                    case ChoiceType.Ban:
                        state = Steps.Ban;
                        break;

                    case ChoiceType.RedWin or ChoiceType.BlueWin:
                        state = Steps.Win;
                        break;
                }
            }

            newDisplay = pickType == ChoiceType.Swap ? new TrapInfoDisplay(trap: TrapType.Swap) : new InstructionDisplay(team: color, step: state);

            if (oldDisplay != newDisplay)
            {
                informationDisplayContainer.Child = newDisplay;
                informationDisplayContainer.FadeInFromZero(duration: 200, easing: Easing.InCubic);
            }

            if (state == Steps.FinalWin)
            {
                CurrentMatch.Value.Round.Value?.IsFinalStage.BindTo(new BindableBool(color == TeamColour.Neutral));

                if (!bottomOnly)
                {
                    AddInternal(new RoundAnimation(teamWinner == TeamColour.Red
                        ? CurrentMatch.Value.Team1.Value
                        : teamWinner == TeamColour.Blue
                            ? CurrentMatch.Value.Team2.Value
                            : null, teamWinner));
                }
            }
            else
            {
                CurrentMatch.Value.Round.Value?.IsFinalStage.BindTo(new BindableBool(false));
            }
        }

        /*
        private void setNextMode()
        {
            if (CurrentMatch.Value?.Round.Value == null)
                return;

            int totalBansRequired = CurrentMatch.Value.Round.Value.BanCount.Value * 2;

            TeamColour lastPickColour = CurrentMatch.Value.PicksBans.LastOrDefault()?.Team ?? TeamColour.Red;

            TeamColour nextColour;

            bool hasAllBans = CurrentMatch.Value.PicksBans.Count(p => p.Type == ChoiceType.Ban) >= totalBansRequired;

            if (!hasAllBans)
            {
                // Ban phase: switch teams every second ban.
                nextColour = CurrentMatch.Value.PicksBans.Count % 2 == 1
                    ? getOppositeTeamColour(lastPickColour)
                    : lastPickColour;
            }
            else
            {
                // Pick phase : switch teams every pick, except for the first pick which generally goes to the team that placed the last ban.
                nextColour = pickType == ChoiceType.Pick
                    ? getOppositeTeamColour(lastPickColour)
                    : lastPickColour;
            }

            setMode(nextColour, hasAllBans ? ChoiceType.Pick : ChoiceType.Ban);

            TeamColour getOppositeTeamColour(TeamColour colour) => colour == TeamColour.Red ? TeamColour.Blue : TeamColour.Red;
        }
        */

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            var maps = mapFlows.Select(f => f.FirstOrDefault(m => m.ReceivePositionalInputAt(e.ScreenSpaceMousePosition)));
            var map = maps.FirstOrDefault(m => m != null);

            if (map != null)
            {
                if (e.Button == MouseButton.Left && map.Beatmap?.OnlineID > 0)
                {
                    // Handle updating status to Red/Blue Win
                    if (isPickWin)
                    {
                        updateWinStatusForBeatmap(map.Beatmap.OnlineID);
                    }
                    else
                    {
                        addForBeatmap(map.Beatmap.OnlineID);
                    }
                }
                else if (e.Button == MouseButton.Right)
                {
                    var existing = CurrentMatch.Value?.PicksBans.FirstOrDefault(p => p.BeatmapID == map.Beatmap?.OnlineID);

                    if (existing != null)
                    {
                        CurrentMatch.Value?.PicksBans.Remove(existing);
                        hasTrap = false;
                        // setNextMode();
                    }
                    else
                    {
                        var existingProtect = CurrentMatch.Value?.Protects.FirstOrDefault(p => p.BeatmapID == map.Beatmap?.OnlineID);
                        var existingTrap = CurrentMatch.Value?.Traps.FirstOrDefault(p => p.BeatmapID == map.Beatmap?.OnlineID);
                        var PBTrap = CurrentMatch.Value?.PicksBans.FirstOrDefault(p => p.BeatmapID == map.Beatmap?.OnlineID && p.Type == ChoiceType.Trap);
                        var existingPendingSwap = CurrentMatch.Value?.PendingSwaps.FirstOrDefault(p => p.BeatmapID == map.Beatmap?.OnlineID);
                        if (existingProtect != null) CurrentMatch.Value?.Protects.Remove(existingProtect);
                        if (existingTrap != null) CurrentMatch.Value?.Traps.Remove(existingTrap);
                        if (PBTrap != null) CurrentMatch.Value?.PicksBans.Remove(PBTrap);
                        if (existingPendingSwap != null) CurrentMatch.Value?.PendingSwaps.Remove(existingPendingSwap);
                    }
                }

                // Automatically detect EX & win conditions
                if (CurrentMatch.Value != null)
                {
                    buttonIndicator.Colour = DetectWin() ? Color4.Orange : (DetectEX() ? Color4.White : Color4.Gray);
                    havePendingSwap = CurrentMatch.Value.PendingSwaps.Any();

                    if (!hasTrap)
                    {
                        // Restore to the last state
                        updateBottomDisplay(bottomOnly: false);
                    }
                }

                return true;
            }

            return base.OnMouseDown(e);
        }

        private void updateWinStatusForBeatmap(int beatmapId)
        {
            var existing = CurrentMatch.Value?.PicksBans.FirstOrDefault(p => p.BeatmapID == beatmapId && (p.Type == ChoiceType.RedWin || p.Type == ChoiceType.BlueWin));

            if (existing != null)
            {
                CurrentMatch.Value?.PicksBans.Remove(existing);
            }

            CurrentMatch.Value?.PicksBans.Add(new BeatmapChoice
            {
                Team = pickType == ChoiceType.RedWin ? TeamColour.Red : TeamColour.Blue,
                Type = pickType,
                BeatmapID = beatmapId,
                Token = true,
            });
            // setNextMode(); // Uncomment if you still want to automatically set the next mode
        }

        private void reset()
        {
            // Clear map marking lists
            CurrentMatch.Value?.PicksBans.Clear();
            CurrentMatch.Value?.Protects.Clear();
            CurrentMatch.Value?.Traps.Clear();
            CurrentMatch.Value?.PendingSwaps.Clear();
            CurrentMatch.Value?.Round.Value?.IsFinalStage.BindTo(new BindableBool(false));

            // Reset bottom display
            informationDisplayContainer.Child = new InstructionDisplay();

            // Reset button group
            buttonBlueProtect.Colour = Color4.White;
            buttonBlueBan.Colour = Color4.White;
            buttonBluePick.Colour = Color4.White;
            buttonBlueWin.Colour = Color4.White;
            buttonBlueTrap.Colour = Color4.White;
            buttonRedProtect.Colour = Color4.White;
            buttonRedBan.Colour = Color4.White;
            buttonRedPick.Colour = Color4.White;
            buttonRedWin.Colour = Color4.White;
            buttonRedTrap.Colour = Color4.White;
            buttonTrapSwap.Colour = Color4.White;
            buttonIndicator.Colour = Color4.Gray;

            // setNextMode();
        }

        private bool isPickWin => pickType == ChoiceType.RedWin || pickType == ChoiceType.BlueWin;

        private void addForBeatmap(string modId)
        {
            if (CurrentMatch.Value?.Round.Value == null)
                return;

            var map = CurrentMatch.Value.Round.Value.Beatmaps.FirstOrDefault(b => b.Mods + b.ModIndex == modId);

            if (map != null)
                addForBeatmap(map.ID);
        }

        private void addForBeatmap(int beatmapId)
        {
            bool hasReversed = false;

            if (pickType == ChoiceType.Neutral)
                return;

            if (CurrentMatch.Value?.Round.Value == null)
                return;

            if (CurrentMatch.Value.Round.Value.Beatmaps.All(b => b.Beatmap?.OnlineID != beatmapId))
                // don't attempt to add if the beatmap isn't in our pool
                return;

            if (CurrentMatch.Value.PicksBans.Any(p => p.BeatmapID == beatmapId && ((p.Type == ChoiceType.Ban || p.Type == ChoiceType.RedWin || p.Type == ChoiceType.BlueWin) && !isPickWin && pickType != ChoiceType.Swap)))
                // don't attempt to add if already banned / winned and it's not a win type.
                return;

            if (CurrentMatch.Value.Protects.Any(p => p.BeatmapID == beatmapId && pickType == ChoiceType.Ban))
                // don't attempt to ban a protected map
                return;

            // Remove the latest win state for Reverse Trap
            if (CurrentMatch.Value.PicksBans.Any(p => p.BeatmapID == beatmapId && (p.Type == ChoiceType.RedWin || p.Type == ChoiceType.BlueWin) && pickType == ChoiceType.Pick))
            {
                var latestWin = CurrentMatch.Value.PicksBans.LastOrDefault(p => p.BeatmapID == beatmapId && (p.Type == ChoiceType.RedWin || p.Type == ChoiceType.BlueWin));
                if (latestWin != null) CurrentMatch.Value.PicksBans.Remove(latestWin);
            }

            // Show the trap description
            if (CurrentMatch.Value.Traps.Any(p => p.BeatmapID == beatmapId))
            {
                // One and another for both teams
                var matchTrap = CurrentMatch.Value.Traps.FirstOrDefault(p => p.BeatmapID == beatmapId);
                var anotherTrap = CurrentMatch.Value.Traps.LastOrDefault(p => p.BeatmapID == beatmapId);
                var PBTrap = CurrentMatch.Value.PicksBans.FirstOrDefault(p => p.BeatmapID == beatmapId && p.Type == ChoiceType.Trap);

                if (pickType == ChoiceType.Pick && matchTrap != null && anotherTrap != null)
                {
                    bool isSameTeam = matchTrap.Team == anotherTrap.Team;
                    var triggeredTrap = matchTrap.Team != pickColour ? matchTrap : anotherTrap;
                    if (!isSameTeam)
                    {
                        informationDisplayContainer.Child = new TrapInfoDisplay(
                            trap: triggeredTrap.Mode, team: triggeredTrap.Team, mapID: triggeredTrap.BeatmapID);
                        CurrentMatch.Value.Traps.Remove(matchTrap);
                        CurrentMatch.Value.Traps.Remove(anotherTrap);
                    }
                    else
                    {
                        informationDisplayContainer.Child = matchTrap.Team != pickColour
                        ? new TrapInfoDisplay(trap: matchTrap.Mode, team: matchTrap.Team, mapID: matchTrap.BeatmapID)
                        : new TrapInfoDisplay(trap: TrapType.Unused, team: matchTrap.Team, mapID: matchTrap.BeatmapID);
                        CurrentMatch.Value.Traps.Remove(matchTrap);
                        if (PBTrap != null) CurrentMatch.Value.PicksBans.Remove(PBTrap);
                    }

                    if (matchTrap.Mode == TrapType.Reverse)
                    {
                        // Add Win status first
                        hasReversed = true;
                        CurrentMatch.Value.PicksBans.Add(new BeatmapChoice
                        {
                            BeatmapID = beatmapId,
                            Team = matchTrap.Team == TeamColour.Red ? TeamColour.Red : TeamColour.Blue,
                            Type = matchTrap.Team == TeamColour.Red ? ChoiceType.RedWin : ChoiceType.BlueWin,
                            Token = true,
                        });
                    }
                    hasTrap = true;
                }
                else
                {
                    hasTrap = false;
                }
            }

            if (pickType == ChoiceType.Pick)
            {
                var introMap = CurrentMatch.Value.Round.Value.Beatmaps.FirstOrDefault(b => b.Beatmap?.OnlineID == beatmapId);
                if (introMap != null)
                {
                    AddInternal(new TournamentIntro(introMap)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    });
                }
            }

            // Perform a Swap with the latest untriggered Swap
            if (pickType == ChoiceType.Swap)
            {
                // Already have one: perform a Swap
                var source = CurrentMatch.Value.PendingSwaps.FirstOrDefault();

                if (source != null)
                {
                    CurrentMatch.Value.PendingSwaps.Add(new BeatmapChoice
                    {
                        Team = TeamColour.Neutral,
                        Type = ChoiceType.Neutral,
                        BeatmapID = beatmapId,
                        Token = true,
                    });
                    SwapMap(source.BeatmapID, beatmapId);
                }
                else
                {
                    // Add as a pending Swap operation
                    CurrentMatch.Value.PendingSwaps.Add(new BeatmapChoice
                    {
                        Team = TeamColour.Neutral,
                        Type = ChoiceType.Neutral,
                        BeatmapID = beatmapId,
                        Token = true,
                    });
                }
            }

            // Trap action specific
            if (pickType == ChoiceType.Trap)
            {
                CurrentMatch.Value.Traps.Add(new TrapInfo
                {
                    Team = pickColour,
                    Mode = new TrapInfo().GetReversedType(trapTypeDropdown.Current.Value),
                    BeatmapID = beatmapId,
                });
            }

            // Not to add a same map reference of the same type twice!
            if (pickType == ChoiceType.Protect && !CurrentMatch.Value.Protects.Any(p => p.BeatmapID == beatmapId))
            {
                CurrentMatch.Value.Protects.Add(new BeatmapChoice
                {
                    Team = pickColour,
                    Type = pickType,
                    BeatmapID = beatmapId,
                    Token = true,
                });
            }

            if (pickType != ChoiceType.Swap && !hasReversed && !CurrentMatch.Value.PicksBans.Any(p => p.BeatmapID == beatmapId && p.Type == pickType))
            {
                CurrentMatch.Value.PicksBans.Add(new BeatmapChoice
                {
                    Team = pickColour,
                    Type = pickType,
                    BeatmapID = beatmapId,
                    Token = true,
                });
            }

            // setNextMode(); // Uncomment if you still want to automatically set the next mode

            if (LadderInfo.AutoProgressScreens.Value)
            {
                if (pickType == ChoiceType.Pick && CurrentMatch.Value.PicksBans.Any(i => i.Type == ChoiceType.Pick))
                {
                    scheduledScreenChange?.Cancel();
                    scheduledScreenChange = Scheduler.AddDelayed(() => { sceneManager?.SetScreen(typeof(GameplayScreen)); }, 10000);
                }
            }
        }

        public override void Hide()
        {
            scheduledScreenChange?.Cancel();
            base.Hide();
        }

        protected override void CurrentMatchChanged(ValueChangedEvent<TournamentMatch?> match)
        {
            base.CurrentMatchChanged(match);
            updateDisplay();
        }

        protected void SwapMap(int sourceMapID, int targetMapID)
        {
            var source = CurrentMatch.Value?.Round.Value?.Beatmaps.FirstOrDefault(p => p.Beatmap?.OnlineID == sourceMapID);
            var target = CurrentMatch.Value?.Round.Value?.Beatmaps.FirstOrDefault(p => p.Beatmap?.OnlineID == targetMapID);

            // Already detected null here, no need to do again
            if (source != null && target != null)
            {
                if (CurrentMatch.Value?.Round.Value?.UseBoard.Value == false) return;

                int middleX = source.BoardX;
                int middleY = source.BoardY;

                source.BoardX = target.BoardX;
                source.BoardY = target.BoardY;

                target.BoardX = middleX;
                target.BoardY = middleY;


                // TODO: A better way to reload maps
                DetectWin();
                DetectEX();
                updateDisplay();
                // CurrentMatch.Value?.PendingSwaps.Clear();
            }
            else
            {
                // Rare, but may happen
                throw new InvalidOperationException("Cannot get the corresponding maps.");
            }
        }

        /// <summary>
        /// Detects if someone has won the match.
        /// </summary>
        /// <returns>true if has, otherwise false</returns>
        public bool DetectWin()
        {
            // Don't detect if not defining board coordinates
            if (CurrentMatch.Value?.Round.Value?.Beatmaps == null) return false;
            if (!CurrentMatch.Value.Round.Value.UseBoard.Value) return false;
            if (CurrentMatch.Value.PendingSwaps.Any()) return false;

            List<TeamColour> winColours = new List<TeamColour>();

            TeamColour winner;

            winColours.Add(isWin(1, 1, 1, 4));
            winColours.Add(isWin(2, 1, 2, 4));
            winColours.Add(isWin(3, 1, 3, 4));
            winColours.Add(isWin(4, 1, 4, 4));
            winColours.Add(isWin(1, 1, 4, 1));
            winColours.Add(isWin(1, 2, 4, 2));
            winColours.Add(isWin(1, 3, 4, 3));
            winColours.Add(isWin(1, 4, 4, 4));
            winColours.Add(isWin(1, 1, 4, 4));
            winColours.Add(isWin(1, 4, 4, 1));

            winner = winColours.Contains(TeamColour.Red)
                ? winColours.Contains(TeamColour.Blue)
                    ? TeamColour.Neutral
                    : TeamColour.Red
                : winColours.Contains(TeamColour.Blue)
                    ? TeamColour.Blue
                    : TeamColour.None;

            teamWinner = winner;

            if (winner == TeamColour.Neutral || winner == TeamColour.None)
            {
                // Reset team scores
                CurrentMatch.Value.Team1Score.Value = 0;
                CurrentMatch.Value.Team2Score.Value = 0;

                return winner == TeamColour.Neutral;
            }
            else
            {
                CurrentMatch.Value.Team1Score.Value = winner == TeamColour.Red ? 6 : 0;
                CurrentMatch.Value.Team2Score.Value = winner == TeamColour.Blue ? 6 : 0;

                return true;
            }
        }

        /// <summary>
        /// Get all beatmaps on a specified line.
        /// </summary>
        /// <param name="startX">The start point of the line, X value.</param>
        /// <param name="startY">The start point of the line, Y value.</param>
        /// <param name="endX">The end point of the line, X value.</param>
        /// <param name="endY">The end point of the line, Y value.</param>
        /// <returns>A <see langword="List"/> of <see cref="RoundBeatmap"/>.</returns>
        private List<RoundBeatmap> getMapLine(int startX, int startY, int endX, int endY)
        {
            List<RoundBeatmap> mapLine = new List<RoundBeatmap>();

            // Reject null matches
            if (CurrentMatch.Value == null) return mapLine;

            if (startX == endX)
            {
                for (int i = startY; i <= endY; i++)
                {
                    var map = getBoardMap(startX, i);
                    if (map != null) mapLine.Add(map);
                }
            }
            else if (startY == endY)
            {
                for (int i = startX; i <= endX; i++)
                {
                    var map = getBoardMap(i, startY);
                    if (map != null) mapLine.Add(map);
                }
            }
            else
            {
                int stepX = endX > startX ? 1 : -1;
                int stepY = endY > startY ? 1 : -1;
                for (int i = 0; i <= 3; i++)
                {
                    var map = getBoardMap(startX + i * stepX, startY + i * stepY);
                    if (map != null) mapLine.Add(map);
                }
            }
            return mapLine;
        }

        /// <summary>
        /// Detects if either team has won.
        ///
        /// <br></br>The given line should be either a straight line or a diagonal line.
        /// </summary>
        /// <param name="startX">The start point of the line, X value.</param>
        /// <param name="startY">The start point of the line, Y value.</param>
        /// <param name="endX">The end point of the line, X value.</param>
        /// <param name="endY">The end point of the line, Y value.</param>
        /// <returns>the winner team's colour, or <see cref="TeamColour.Neutral"/> if there isn't one</returns>
        private TeamColour isWin(int startY, int startX, int endY, int endX)
        {
            List<RoundBeatmap> mapLine = new List<RoundBeatmap>();
            const TeamColour colourfalse = TeamColour.Neutral;
            TeamColour thisColour = TeamColour.Neutral;

            // Currently limited to 4x4 use only
            if ((endX - startX) % 3 != 0 || (endY - startY) % 3 != 0) return colourfalse;

            // Reject null matches
            if (CurrentMatch.Value == null) return colourfalse;

            mapLine = getMapLine(startY, startX, endY, endX);

            foreach (RoundBeatmap b in mapLine)
            {
                // Get the coloured map
                var pickedMap = CurrentMatch.Value.PicksBans.FirstOrDefault(p => (p.BeatmapID == b.Beatmap?.OnlineID &&
                        (p.Type == ChoiceType.RedWin || p.Type == ChoiceType.BlueWin)));

                // Have banned maps: Cannot win
                if (CurrentMatch.Value.PicksBans.Any(p => (p.BeatmapID == b.Beatmap?.OnlineID && p.Type == ChoiceType.Ban))) return colourfalse;
                if (pickedMap != null)
                {
                    // Set the default colour
                    if (thisColour == TeamColour.Neutral) { thisColour = pickedMap.Team; }
                    // Different mark colour: Cannot win
                    else { if (thisColour != pickedMap.Team) return colourfalse; }
                }
                else return colourfalse;
            }

            // Finally: Can win
            return thisColour;
        }

        /// <summary>
        /// Detects if the board satisfies the conditions to enter the EX stage.
        /// </summary>
        /// <returns>true if satisfies, otherwise false.</returns>
        public bool DetectEX()
        {
            if (CurrentMatch.Value?.Round.Value?.Beatmaps == null) return false;
            if (!CurrentMatch.Value.Round.Value.UseBoard.Value) return false;
            if (CurrentMatch.Value.PendingSwaps.Any()) return false;

            // Manba out
            bool isRowAvailable = canWin(1, 1, 1, 4) || canWin(2, 1, 2, 4) || canWin(3, 1, 3, 4) || canWin(4, 1, 4, 4);
            bool isColumnAvailable = canWin(1, 1, 4, 1) || canWin(1, 2, 4, 2) || canWin(1, 3, 4, 3) || canWin(1, 4, 4, 4);
            bool isDiagonalAvailable = canWin(1, 1, 4, 4) || canWin(1, 4, 4, 1);

            useEX = !isDiagonalAvailable && !isRowAvailable && !isColumnAvailable;
            return useEX;
        }

        /// <summary>
        /// Detects if either team could use the given line to win.
        ///
        /// <br></br>The given line should be either a straight line or a diagonal line.
        /// </summary>
        /// <param name="startX">The start point of the line, X value.</param>
        /// <param name="startY">The start point of the line, Y value.</param>
        /// <param name="endX">The end point of the line, X value.</param>
        /// <param name="endY">The end point of the line, Y value.</param>
        /// <returns>true if can, otherwise false</returns>
        private bool canWin(int startY, int startX, int endY, int endX)
        {
            List<RoundBeatmap> mapLine = new List<RoundBeatmap>();
            TeamColour thisColour = TeamColour.Neutral;

            // Currently limited to 4x4 use only
            if ((endX - startX) % 3 != 0 || (endY - startY) % 3 != 0) return false;

            // Reject null matches
            if (CurrentMatch.Value == null) return false;

            mapLine = getMapLine(startX, startY, endX, endY);

            foreach (RoundBeatmap b in mapLine)
            {
                // Get the coloured map
                var pickedMap = CurrentMatch.Value.PicksBans.FirstOrDefault(p =>
                    (p.BeatmapID == b.Beatmap?.OnlineID &&
                     (p.Type == ChoiceType.RedWin
                      || p.Type == ChoiceType.BlueWin)));

                // Have banned maps: Cannot win
                if (CurrentMatch.Value.PicksBans.Any(p => (p.BeatmapID == b.Beatmap?.OnlineID && p.Type == ChoiceType.Ban))) return false;

                if (pickedMap != null)
                {
                    // Set the default colour
                    if (thisColour == TeamColour.Neutral) { thisColour = pickedMap.Team; }
                    // Different mark colour: Cannot win
                    else
                    {
                        if (thisColour != pickedMap.Team) return false;
                    }
                }
            }

            // Finally: Can win
            return true;
        }

        /// <summary>
        /// Get a beatmap placed on a specific point on the board.
        /// </summary>
        /// <param name="X">The X coordinate value of the beatmap.</param>
        /// <param name="Y">The Y coordinate value of the beatmap.</param>
        /// <returns>A <see cref="RoundBeatmap"/>, pointing to the corresponding beatmap.</returns>
        private RoundBeatmap? getBoardMap(int X, int Y)
            => CurrentMatch.Value?.Round.Value?.Beatmaps.FirstOrDefault(p => (p.BoardX == X && p.BoardY == Y && p.Mods != "EX")) ?? null;

        private void updateDisplay()
        {
            mapFlows.Clear();

            if (CurrentMatch.Value == null)
            {
                AddInternal(warning = new WarningBox("Cannot access current match, sorry ;w;"));
                return;
            }

            if (CurrentMatch.Value.Round.Value != null)
            {
                FillFlowContainer<BoardBeatmapPanel>? currentFlow = null;

                // Use predefined Board coodinate
                if (CurrentMatch.Value.Round.Value.UseBoard.Value)
                {
                    warning?.FadeOut(duration: 200, easing: Easing.OutCubic);

                    for (int i = 1; i <= 4; i++)
                    {
                        mapFlows.Add(currentFlow = new FillFlowContainer<BoardBeatmapPanel>
                        {
                            Spacing = new Vector2(10, 10),
                            Direction = FillDirection.Full,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y
                        });

                        for (int j = 1; j <= 4; j++)
                        {
                            var nextMap = CurrentMatch.Value.Round.Value.Beatmaps.FirstOrDefault(p => (p.Mods != "EX" && p.BoardX == j && p.BoardY == i));

                            if (nextMap != null)
                            {
                                var hasSwappedMap = CurrentMatch.Value.PendingSwaps.FirstOrDefault(p => p.BeatmapID == nextMap.Beatmap?.OnlineID);
                                currentFlow.Add(new BoardBeatmapPanel(nextMap.Beatmap, nextMap.Mods, nextMap.ModIndex, hasSwappedMap != null)
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    Height = 150,
                                });
                                if (hasSwappedMap != null) CurrentMatch.Value.PendingSwaps.Remove(hasSwappedMap);
                            }
                        }
                    }
                }
                else
                {
                    AddInternal(warning = new WarningBox("This round isn't set up for board view..."));
                    return;
                }
                mapFlows.Padding = new MarginPadding(5);
            }
        }
    }
}
