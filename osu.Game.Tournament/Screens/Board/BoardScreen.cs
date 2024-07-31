// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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

        private TeamColour pickColour;
        private ChoiceType pickType;

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
        private OsuButton buttonDraw = null!;

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
                    ShowScores = true,
                },
                new TournamentMatchChatDisplay(cornerRadius: 10)
                {
                    RelativeSizeAxes = Axes.None,
                    Height = 400,
                    Width = 600,
                    Origin = Anchor.CentreLeft,
                    Position = new Vector2(30, -350),
                },
                mapFlows = new FillFlowContainer<FillFlowContainer<BoardBeatmapPanel>>
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    // RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    X = 300,
                    Y = -270,
                    Height = 1f,
                    Width = 900,
                    Padding = new MarginPadding{ Left = 50 },
                    Spacing = new Vector2(10, 10),
                },

                new ControlPanel
                {
                    Children = new Drawable[]
                    {
                        new TournamentSpriteText
                        {
                            Text = "Current Mode"
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
                        buttonDraw = new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Mark Draw",
                            BackgroundColour = Color4.Indigo,
                            Action = () => setMode(TeamColour.Neutral, ChoiceType.Draw)
                        },
                        new ControlPanel.Spacer(),
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
            buttonRedProtect.Colour = setProtect(pickColour == TeamColour.Red && pickType == ChoiceType.Protect);
            buttonBlueProtect.Colour = setProtect(pickColour == TeamColour.Blue && pickType == ChoiceType.Protect);
            buttonRedWin.Colour = setWin(pickColour == TeamColour.Red && pickType == ChoiceType.RedWin);
            buttonBlueWin.Colour = setWin(pickColour == TeamColour.Blue && pickType == ChoiceType.BlueWin);
            buttonRedTrap.Colour = setColour(pickColour == TeamColour.Red && pickType == ChoiceType.Trap);
            buttonBlueTrap.Colour = setColour(pickColour == TeamColour.Blue && pickType == ChoiceType.Trap);
            buttonDraw.Colour = setColour(pickColour == TeamColour.Neutral && pickType == ChoiceType.Draw);

            static Color4 setColour(bool active) => active ? Color4.White : Color4.Gray;
            // Odd color?
            static Color4 setProtect(bool active) => active ? Color4.Aqua : Color4.Yellow;
            static Color4 setWin(bool active) => active ? Color4.White : Color4.Gray;
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
                    if (pickType == ChoiceType.RedWin || pickType == ChoiceType.BlueWin)
                    {
                        var existing = CurrentMatch.Value?.PicksBans.FirstOrDefault(p => p.BeatmapID == map.Beatmap?.OnlineID);
                        CurrentMatch.Value?.PicksBans.Remove(existing);
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
            CurrentMatch.Value?.PicksBans.Clear();
            //setNextMode();
        }

        private void addForBeatmap(int beatmapId)
        {
            if (CurrentMatch.Value?.Round.Value == null)
                return;

            if (CurrentMatch.Value.Round.Value.Beatmaps.All(b => b.Beatmap?.OnlineID != beatmapId))
                // don't attempt to add if the beatmap isn't in our pool
                return;

            if (CurrentMatch.Value.PicksBans.Any(p => p.BeatmapID == beatmapId && (p.Type != ChoiceType.RedWin && p.Type != ChoiceType.BlueWin)))
                // don't attempt to add if already exists and it's not a win type.
                return;

            CurrentMatch.Value.PicksBans.Add(new BeatmapChoice
            {
                Team = pickColour,
                Type = pickType,
                BeatmapID = beatmapId
            });

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
                string? currentMods;
                int flowCount = 0;

                foreach (var b in CurrentMatch.Value.Round.Value.Beatmaps)
                {
                    if (currentFlow == null)
                    {
                        mapFlows.Add(currentFlow = new FillFlowContainer<BoardBeatmapPanel>
                        {
                            Spacing = new Vector2(10, 10),
                            Direction = FillDirection.Full,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y
                        });

                        currentMods = b.Mods;

                        totalRows++;
                        flowCount = 0;
                    }

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

            mapFlows.Padding = new MarginPadding(5)
            {
                // remove horizontal padding to increase flow width to 3 panels
                Horizontal = totalRows > 9 ? 0 : 100
            };
        }
    }
}
