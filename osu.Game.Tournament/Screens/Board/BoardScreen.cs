// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Threading;
using osu.Game.Graphics.UserInterface;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.IPC;
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

        private readonly Bindable<TournamentMatch?> currentMatch = new Bindable<TournamentMatch?>();

        [Resolved]
        private TournamentSceneManager? sceneManager { get; set; }

        private TeamColour pickColour;
        private ChoiceType pickType;

        private TeamColour teamWinner = TeamColour.Neutral;

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

        private TrapTypeDropdown trapTypeDropdown = null!;
        private Container trapInfoDisplayHolder = null!;
        private Container instructionDisplayHolder = null!;

        private DrawableTeamPlayerList team1List = null!;
        private DrawableTeamPlayerList team2List = null!;

        private readonly int sideListHeight = 660;

        private ScheduledDelegate? scheduledScreenChange;

        [BackgroundDependencyLoader]
        private void load(MatchIPCInfo ipc)
        {
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
                    Position = new Vector2(30, 110),
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
                        new TournamentMatchChatDisplay(cornerRadius: 10)
                        {
                            RelativeSizeAxes = Axes.None,
                            Height = sideListHeight - team1List.GetHeight() - 5,
                            Width = 300,
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            Margin = new MarginPadding { Top = 10 },
                        },
                    },
                },
                new FillFlowContainer
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    RelativeSizeAxes = Axes.None,
                    Position = new Vector2(-30, 110),
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
                        new EmptyBox(cornerRadius: 10)
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            RelativeSizeAxes = Axes.None,
                            Width = 300,
                            Height = sideListHeight - team2List.GetHeight() - 5,
                            Margin = new MarginPadding { Top = 10 },
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
                trapInfoDisplayHolder = new Container
                {
                    Alpha = 0,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Height = 100,
                    Width = 500,
                },
                instructionDisplayHolder = new Container
                {
                    Alpha = 1,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Height = 100,
                    Width = 500,
                    Child = new InstructionDisplay(),
                },
                new ControlPanel
                {
                    Children = new Drawable[]
                    {
                        new TournamentSpriteText
                        {
                            Text = "Current Mode"
                        },
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
                        new ControlPanel.Spacer(),
                        trapTypeDropdown = new TrapTypeDropdown
                        {
                            LabelText = "Trap type"
                        },
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
                        buttonTrapSwap = new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Perform Swap",
                            BackgroundColour = Color4.Indigo,
                            Action = () => setMode(TeamColour.Neutral, ChoiceType.TrapSwap)
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
                            Text = "Reset",
                            BackgroundColour = Color4.Orange,
                            Action = reset
                        },
                    },
                }
            };

            ipc.Beatmap.BindValueChanged(beatmapChanged);
        }

        private void beatmapChanged(ValueChangedEvent<TournamentBeatmap?> beatmap)
        {
            if (CurrentMatch.Value?.Round.Value == null)
                return;

            int totalBansRequired = CurrentMatch.Value.Round.Value.BanCount.Value * 2;

            if (CurrentMatch.Value.PicksBans.Count(p => p.Type == ChoiceType.Ban) < totalBansRequired)
                return;

            // if bans have already been placed, beatmap changes result in a selection being made automatically
            if (beatmap.NewValue?.OnlineID > 0)
                addForBeatmap(beatmap.NewValue.OnlineID);
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
            buttonTrapSwap.Colour = setColour(pickType == ChoiceType.TrapSwap);

            static Color4 setColour(bool active) => active ? Color4.White : Color4.Gray;

            switch (choiceType)
            {
                case ChoiceType.Protect:
                    instructionDisplayHolder.Child = new InstructionDisplay(team: pickColour, step: Steps.Protect);
                    break;

                case ChoiceType.Pick:
                    instructionDisplayHolder.Child = new InstructionDisplay(team: pickColour, step: Steps.Pick);
                    break;

                case ChoiceType.Trap:
                    instructionDisplayHolder.Child = new InstructionDisplay(team: pickColour, step: Steps.Trap);
                    break;

                case ChoiceType.Ban:
                    instructionDisplayHolder.Child = new InstructionDisplay(team: pickColour, step: Steps.Ban);
                    break;

                case ChoiceType.RedWin or ChoiceType.BlueWin:
                    instructionDisplayHolder.Child = new InstructionDisplay(team: pickColour, step: Steps.Win);
                    break;

                case ChoiceType.TrapSwap:
                    trapInfoDisplayHolder.Child = new TrapInfoDisplay(trap: TrapType.Swap);
                    instructionDisplayHolder.FadeOut(duration: 250, easing: Easing.OutCubic);
                    trapInfoDisplayHolder.FadeInFromZero(duration: 250, easing: Easing.InCubic);
                    break;

                default:
                    instructionDisplayHolder.Child = new InstructionDisplay(team: teamWinner, step: DetectWin() ? Steps.FinalWin : (useEX ? Steps.EX : Steps.Default));
                    break;
            }

            if (choiceType != ChoiceType.TrapSwap)
            {
                trapInfoDisplayHolder.FadeOut(duration: 250, easing: Easing.OutCubic);
                instructionDisplayHolder.FadeInFromZero(duration: 250, easing: Easing.InCubic);
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
                        var existing = CurrentMatch.Value?.PicksBans.FirstOrDefault(p => p.BeatmapID == map.Beatmap?.OnlineID);
                        if (existing != null) CurrentMatch.Value?.PicksBans.Remove(existing);
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
                        // setNextMode();
                    }
                    else
                    {
                        var existingProtect = CurrentMatch.Value?.Protects.FirstOrDefault(p => p.BeatmapID == map.Beatmap?.OnlineID);
                        var existingTrap = CurrentMatch.Value?.Traps.FirstOrDefault(p => p.BeatmapID == map.Beatmap?.OnlineID);
                        if (existingProtect != null) CurrentMatch.Value?.Protects.Remove(existingProtect);
                        if (existingTrap != null) CurrentMatch.Value?.Traps.Remove(existingTrap);
                    }
                }

                // Automatically detect EX & win conditions
                if (CurrentMatch.Value != null)
                {
                    buttonIndicator.Colour = DetectWin() ? Color4.Orange : (DetectEX() ? Color4.White : Color4.Gray);
                    if (teamWinner != TeamColour.Neutral)
                    {
                        instructionDisplayHolder.Child = new InstructionDisplay(team: teamWinner, step: Steps.FinalWin);
                        instructionDisplayHolder.FadeInFromZero(duration: 500, easing: Easing.InCubic);
                    }
                    else if (useEX)
                    {
                        instructionDisplayHolder.Child = new InstructionDisplay(step: Steps.EX);
                        instructionDisplayHolder.FadeInFromZero(duration: 200, easing: Easing.InCubic);
                    }
                }

                return true;
            }

            return base.OnMouseDown(e);
        }

        private void updateWinStatusForBeatmap(int beatmapId)
        {
            var existing = CurrentMatch.Value?.PicksBans.FirstOrDefault(p => p.BeatmapID == beatmapId);
            if (existing != null)
            {
                existing.Type = pickType;
                // Ensure the pickColour is set correctly for the win types.
                existing.Team = pickType == ChoiceType.RedWin ? TeamColour.Red : TeamColour.Blue;
            }
            else
            {
                CurrentMatch.Value?.PicksBans.Add(new BeatmapChoice
                {
                    Team = pickType == ChoiceType.RedWin ? TeamColour.Red : TeamColour.Blue,
                    Type = pickType,
                    BeatmapID = beatmapId
                });
            }
            // setNextMode(); // Uncomment if you still want to automatically set the next mode
        }
        private void reset()
        {
            // Clear map marking lists
            CurrentMatch.Value?.PicksBans.Clear();
            CurrentMatch.Value?.Protects.Clear();
            CurrentMatch.Value?.Traps.Clear();
            CurrentMatch.Value?.PendingSwaps.Clear();

            // Reset bottom display
            trapInfoDisplayHolder.FadeOut(duration: 200, easing: Easing.OutCubic);
            instructionDisplayHolder.FadeInFromZero(duration: 200, easing: Easing.InCubic);
            instructionDisplayHolder.Child = new InstructionDisplay();

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

        private void addForBeatmap(int beatmapId)
        {
            if (pickType == ChoiceType.Neutral)
                return;

            if (CurrentMatch.Value?.Round.Value == null)
                return;

            if (CurrentMatch.Value.Round.Value.Beatmaps.All(b => b.Beatmap?.OnlineID != beatmapId))
                // don't attempt to add if the beatmap isn't in our pool
                return;

            if (CurrentMatch.Value.PicksBans.Any(p => p.BeatmapID == beatmapId && (p.Type == ChoiceType.Ban && !isPickWin)))
                // don't attempt to add if already banned and it's not a win type.
                return;

            if (CurrentMatch.Value.Protects.Any(p => p.BeatmapID == beatmapId && (pickType == ChoiceType.Ban || pickType == ChoiceType.Trap)))
                // don't attempt to ban a protected map
                return;

            // Show the trap description
            if (CurrentMatch.Value.Traps.Any(p => p.BeatmapID == beatmapId && !p.IsTriggered))
            {
                var matchTrap = CurrentMatch.Value.Traps.First(p => p.BeatmapID == beatmapId);
                if (pickType == ChoiceType.Pick)
                {
                    // Add as a pending Swap operation
                    if (matchTrap.Mode == TrapType.Swap)
                    {
                        CurrentMatch.Value.PendingSwaps.Add(new BeatmapChoice
                        {
                            Team = TeamColour.Neutral,
                            Type = ChoiceType.Neutral,
                            BeatmapID = beatmapId,
                        });
                    }

                    if (matchTrap.Team != pickColour)
                    {
                        trapInfoDisplayHolder.Child = new TrapInfoDisplay(trap: matchTrap.Mode, team: matchTrap.Team, mapID: matchTrap.BeatmapID);
                    }
                    else
                    {
                        trapInfoDisplayHolder.Child = new TrapInfoDisplay(trap: TrapType.Unused, team: matchTrap.Team, mapID: matchTrap.BeatmapID);
                    }
                    instructionDisplayHolder.FadeOut(duration: 250, easing: Easing.OutCubic);
                    trapInfoDisplayHolder.FadeInFromZero(duration: 250, easing: Easing.InCubic);
                }
                else
                {
                    // Specially designed for Swap trap: Apply then "Trigger" as done
                    if (isPickWin && matchTrap.Mode != TrapType.Swap)
                    {
                        matchTrap.IsTriggered = true;
                    }
                }
            }

            // Perform a Swap with the latest untriggered Swap
            if (pickType == ChoiceType.TrapSwap)
            {
                // Normally there should be one match.
                var source = CurrentMatch.Value.PendingSwaps.FirstOrDefault();
                if (source != null)
                {
                    // "Trigger" the trap upon final swap, after marking colors
                    var targetTrap = CurrentMatch.Value.Traps.FirstOrDefault(p => (p.BeatmapID == source.BeatmapID && !p.IsTriggered));
                    if (targetTrap != null) targetTrap.IsTriggered = true;
                    SwapMap(source.BeatmapID, beatmapId);
                }
            }

            // Trap action specific
            if (pickType == ChoiceType.Trap)
            {
                CurrentMatch.Value.Traps.Add(new TrapInfo
                {
                    Team = pickColour,
                    Mode = new TrapInfo().GetReversedType(trapTypeDropdown.Current.Value),
                    BeatmapID = beatmapId
                });
            }

            // Not to add a same map reference of the same type twice!
            if (pickType == ChoiceType.Protect && !CurrentMatch.Value.Protects.Any(p => p.BeatmapID == beatmapId))
            {
                CurrentMatch.Value.Protects.Add(new BeatmapChoice
                {
                    Team = pickColour,
                    Type = pickType,
                    BeatmapID = beatmapId
                });
            }

            if (!CurrentMatch.Value.PicksBans.Any(p => p.BeatmapID == beatmapId && p.Type == pickType))
            {
                CurrentMatch.Value.PicksBans.Add(new BeatmapChoice
                {
                    Team = pickColour,
                    Type = pickType,
                    BeatmapID = beatmapId
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

            if (source != null && target != null)
            {
                if (!CurrentMatch.Value.Round.Value.UseBoard.Value) return;

                int middleX = source.BoardX;
                int middleY = source.BoardY;

                source.BoardX = target.BoardX;
                source.BoardY = target.BoardY;

                target.BoardX = middleX;
                target.BoardY = middleY;

                CurrentMatch.Value?.PendingSwaps.Clear();

                // TODO: A better way to reload maps
                DetectWin();
                DetectEX();
                updateDisplay();
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

            var winner = TeamColour.Neutral;
            int i = 0;

            while (i == 0)
            {
                if ((winner = isWin(1, 1, 1, 4)) != TeamColour.Neutral) break;
                if ((winner = isWin(2, 1, 2, 4)) != TeamColour.Neutral) break;
                if ((winner = isWin(3, 1, 3, 4)) != TeamColour.Neutral) break;
                if ((winner = isWin(4, 1, 4, 4)) != TeamColour.Neutral) break;
                if ((winner = isWin(1, 1, 4, 1)) != TeamColour.Neutral) break;
                if ((winner = isWin(1, 2, 4, 2)) != TeamColour.Neutral) break;
                if ((winner = isWin(1, 3, 4, 3)) != TeamColour.Neutral) break;
                if ((winner = isWin(1, 4, 4, 4)) != TeamColour.Neutral) break;
                if ((winner = isWin(1, 1, 4, 4)) != TeamColour.Neutral) break;
                if ((winner = isWin(1, 4, 4, 1)) != TeamColour.Neutral) break;
                i++;
            }

            if (winner == TeamColour.Neutral) return false;
            else
            {
                // 6 is just enough to fill the score bar, subject to change
                teamWinner = winner;
                if (winner == TeamColour.Red)
                {
                    CurrentMatch.Value.Team1Score.Value = 6;
                    CurrentMatch.Value.Team2Score.Value = 0;
                }
                else
                {
                    CurrentMatch.Value.Team2Score.Value = 6;
                    CurrentMatch.Value.Team1Score.Value = 0;
                }
                return true;
            }
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
        private TeamColour isWin(int startX, int startY, int endX, int endY)
        {
            const TeamColour colourfalse = TeamColour.Neutral;
            TeamColour thisColour = TeamColour.Neutral;

            // Currently limited to 4x4 use only
            if ((endX - startX) % 3 != 0 || (endY - startY) % 3 != 0) return colourfalse;

            // Reject null matches
            if (CurrentMatch.Value == null) return colourfalse;

            // Exclusively for cases like from (1, 4) to (4, 1)
            for (int i = startX; endX > startX ? i <= endX : i >= endX; i += (endX - startX) / 3)
            {
                for (int j = startY; endY > startY ? j <= endY : j >= endY; j += (endY - startY) / 3)
                {
                    var next = getBoardMap(i, j);
                    if (next == null) continue;
                    // Get the coloured map
                    var pickedMap = CurrentMatch.Value.PicksBans.FirstOrDefault(p => (p.BeatmapID == next.Beatmap?.OnlineID &&
                        (p.Type == ChoiceType.RedWin || p.Type == ChoiceType.BlueWin)));
                    // Have banned maps: Cannot win
                    if (CurrentMatch.Value.PicksBans.Any(p => (p.BeatmapID == next.Beatmap?.OnlineID && p.Type == ChoiceType.Ban))) return colourfalse;
                    if (pickedMap != null)
                    {
                        // Set the default colour
                        if (thisColour == TeamColour.Neutral) { thisColour = pickedMap.Team; }
                        // Different mark colour: Cannot win
                        else { if (thisColour != pickedMap.Team) return colourfalse; }
                    }
                    else return colourfalse;

                    if (endY == startY) break;
                }
                if (endX == startX) break;
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
        private bool canWin(int startX, int startY, int endX, int endY)
        {
            TeamColour thisColour = TeamColour.Neutral;

            // Currently limited to 4x4 use only
            if ((endX - startX) % 3 != 0 || (endY - startY) % 3 != 0) return false;

            // Reject null matches
            if (CurrentMatch.Value == null) return false;

            // Exclusively for cases like from (1, 4) to (4, 1)
            for (int i = startX; endX > startX ? i <= endX : i >= endX; i += (endX - startX) / 3)
            {
                for (int j = startY; endY > startY ? j <= endY : j >= endY; j += (endY - startY) / 3)
                {
                    var next = getBoardMap(i, j);
                    if (next == null) continue;
                    // Get the coloured map
                    var pickedMap = CurrentMatch.Value.PicksBans.FirstOrDefault(p => (p.BeatmapID == next.Beatmap?.OnlineID &&
                        (p.Type == ChoiceType.RedWin || p.Type == ChoiceType.BlueWin)));
                    // Have banned maps: Cannot win
                    if (CurrentMatch.Value.PicksBans.Any(p => (p.BeatmapID == next.Beatmap?.OnlineID && p.Type == ChoiceType.Ban))) return false;
                    if (pickedMap != null)
                    {
                        // Set the default colour
                        if (thisColour == TeamColour.Neutral) { thisColour = pickedMap.Team; }
                        // Different mark colour: Cannot win
                        else { if (thisColour != pickedMap.Team) return false; }
                    }
                    if (endY == startY) break;
                }
                if (endX == startX) break;
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
            => CurrentMatch.Value?.Round.Value?.Beatmaps.FirstOrDefault(p => (p.BoardX == X && p.BoardY == Y)) ?? null;

        private void updateDisplay()
        {
            mapFlows.Clear();

            if (CurrentMatch.Value == null)
                return;

            // const int maxRows = 4;
            int totalRows = 0;

            if (CurrentMatch.Value.Round.Value != null)
            {
                FillFlowContainer<BoardBeatmapPanel>? currentFlow = null;
                int flowCount = 0;

                // Use predefined Board coodinate
                if (CurrentMatch.Value.Round.Value.UseBoard.Value)
                {
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
                            var nextMap = CurrentMatch.Value.Round.Value.Beatmaps.FirstOrDefault(p => (p.Mods != "EX" && p.BoardX == i && p.BoardY == j));
                            if (nextMap != null)
                            {
                                currentFlow.Add(new BoardBeatmapPanel(nextMap.Beatmap, nextMap.Mods, nextMap.ModIndex)
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    Height = 150,
                                });
                            }

                        }
                    }
                }
                // Normal placement
                else
                {
                    foreach (var b in CurrentMatch.Value.Round.Value.Beatmaps)
                    {
                        // Exclude EX beatmaps from the list
                        if (b.Mods == "EX") continue;

                        if (currentFlow == null)
                        {
                            mapFlows.Add(currentFlow = new FillFlowContainer<BoardBeatmapPanel>
                            {
                                Spacing = new Vector2(10, 10),
                                Direction = FillDirection.Full,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y
                            });

                            totalRows++;
                            flowCount = 0;
                        }

                        // One flow per row
                        if (++flowCount > 2)
                        {
                            totalRows++;
                            flowCount = 1;
                        }

                        currentFlow.Add(new BoardBeatmapPanel(b.Beatmap, b.Mods, b.ModIndex)
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Height = 150,
                        });
                    }
                }
            }

            mapFlows.Padding = new MarginPadding(5)
            {
                // remove horizontal padding to increase flow width to 3 panels
                Horizontal = totalRows > 9 ? 0 : 100
            };
        }
    }
}
