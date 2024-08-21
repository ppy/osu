// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osu.Framework.Threading;
using osu.Game.Graphics;
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
    public partial class EXBoardScreen : TournamentMatchScreen
    {
        private FillFlowContainer<FillFlowContainer<EXBoardBeatmapPanel>> mapFlows = null!;

        [Resolved]
        private TournamentSceneManager? sceneManager { get; set; }

        private TeamColour pickColour = TeamColour.Neutral;
        private ChoiceType pickType = ChoiceType.Pick;

        private OsuButton buttonPick = null!;
        private OsuButton buttonRedWin = null!;
        private OsuButton buttonBlueWin = null!;

        private Sprite additionalIcon = null!;

        private DrawableTeamPlayerList team1List = null!;
        private DrawableTeamPlayerList team2List = null!;

        private readonly int sideListHeight = 660;

        private ScheduledDelegate? scheduledScreenChange;

        [BackgroundDependencyLoader]
        private void load(TextureStore textures, MatchIPCInfo ipc)
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
                    ShowScores = false,
                    ShowRound = false,
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
                mapFlows = new FillFlowContainer<FillFlowContainer<EXBoardBeatmapPanel>>
                {
                    Y = 30,
                    Spacing = new Vector2(10, 10),
                    Direction = FillDirection.Vertical,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                },
                new EmptyBox(cornerRadius: 10)
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.None,
                    Height = 80,
                    Width = 650,
                    Colour = Color4.Black,
                    Margin = new MarginPadding { Bottom = 12 },
                    Alpha = 0.7f,
                },
                new FillFlowContainer
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.None,
                    AutoSizeAxes = Axes.X,
                    X = -310,
                    Y = -7,
                    Height = 60,
                    CornerRadius = 10,
                    Margin = new MarginPadding { Bottom = 10 },
                    Direction = FillDirection.Horizontal,
                    AlwaysPresent = true,
                    Children = new Drawable[]
                    {
                        new SpriteIcon
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Icon = FontAwesome.Solid.Bolt,
                            Colour = Color4.Orange,
                            Size = new Vector2(36),
                            Margin = new MarginPadding { Left = 12, Right = 12 },
                        },
                        new FillFlowContainer
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            AutoSizeAxes = Axes.X,
                            RelativeSizeAxes = Axes.Y,
                            Height = 0.9f,
                            Direction = FillDirection.Vertical,
                            Children = new Drawable[]
                            {
                                new TournamentSpriteText
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Text = @"Welcome to the EX Fumo era!",
                                    Font = OsuFont.HarmonyOSSans.With(size: 32, weight: FontWeight.Bold),
                                },
                                new TournamentSpriteText
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Text = @"从上到下循环进行，获胜方可将除对方保图外的有色格子染成己方颜色",
                                    Font = OsuFont.HarmonyOSSans.With(size: 20, weight: FontWeight.Regular),
                                },
                                new TournamentSpriteText
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Text = @"可用棋盘获胜时将自动退出此状态",
                                    Colour = Color4.Orange,
                                    Font = OsuFont.HarmonyOSSans.With(size: 20, weight: FontWeight.Regular),
                                },
                            },
                        },
                    },
                },
                additionalIcon = new Sprite
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomRight,
                    Position = new Vector2(300, -20),
                    Size = new Vector2(64),
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
                        buttonPick = new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Pick",
                            BackgroundColour = Color4.Indigo,
                            Action = () => setMode(TeamColour.Neutral, ChoiceType.Pick)
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

            if (beatmap.NewValue?.OnlineID > 0)
                addForBeatmap(beatmap.NewValue.OnlineID);
        }

        private void setMode(TeamColour colour, ChoiceType choiceType)
        {
            pickColour = colour;
            pickType = choiceType;

            buttonPick.Colour = setColour(pickColour == TeamColour.Neutral && pickType == ChoiceType.Pick);
            buttonRedWin.Colour = setColour(pickColour == TeamColour.Red && pickType == ChoiceType.RedWin);
            buttonBlueWin.Colour = setColour(pickColour == TeamColour.Blue && pickType == ChoiceType.BlueWin);

            static Color4 setColour(bool active) => active ? Color4.White : Color4.Gray;
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            var maps = mapFlows.Select(f => f.FirstOrDefault(m => m.ReceivePositionalInputAt(e.ScreenSpaceMousePosition)));
            var map = maps.FirstOrDefault(m => m != null);

            if (map != null)
            {
                if (e.Button == MouseButton.Left && map.Beatmap?.OnlineID > 0)
                    addForBeatmap(map.Beatmap.OnlineID);
                else
                {
                    var existing = CurrentMatch.Value?.EXPicks.FirstOrDefault(p => p.BeatmapID == map.Beatmap?.OnlineID);

                    if (existing != null)
                    {
                        CurrentMatch.Value?.EXPicks.Remove(existing);
                    }
                }
                return true;
            }

            return base.OnMouseDown(e);
        }

        private void reset()
        {
            CurrentMatch.Value?.EXPicks.Clear();

            // Reset buttons
            buttonPick.Colour = Color4.White;
            buttonBlueWin.Colour = Color4.White;
            buttonRedWin.Colour = Color4.White;
        }

        private void addForBeatmap(int beatmapId)
        {
            if (CurrentMatch.Value?.Round.Value == null)
                return;

            // Block illegal choice type actions.
            if (pickType != ChoiceType.Pick && pickType != ChoiceType.RedWin && pickType != ChoiceType.BlueWin)
                return;

            if (CurrentMatch.Value.Round.Value.Beatmaps.All(b => b.Beatmap?.OnlineID != beatmapId))
                // don't attempt to add if the beatmap isn't in our pool
                return;

            // In EX stage, just remove any existing marks before adding a new one.
            if (CurrentMatch.Value.EXPicks.Any(p => p.BeatmapID == beatmapId))
            {
                var existing = CurrentMatch.Value.EXPicks.FirstOrDefault(p => p.BeatmapID == beatmapId);
                if (existing != null) CurrentMatch.Value.EXPicks.Remove(existing);
            }

            CurrentMatch.Value.EXPicks.Add(new BeatmapChoice
            {
                Team = TeamColour.Neutral,
                Type = pickType,
                BeatmapID = beatmapId,
            });

            if (LadderInfo.AutoProgressScreens.Value)
            {
                if (pickType == ChoiceType.Pick && CurrentMatch.Value.EXPicks.Any(i => i.Type == ChoiceType.Pick))
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

            int totalRows = 0;

            if (CurrentMatch.Value.Round.Value != null)
            {
                FillFlowContainer<EXBoardBeatmapPanel>? currentFlow = null;
                string? currentMods;
                int flowCount = 0;

                foreach (var b in CurrentMatch.Value.Round.Value.Beatmaps)
                {
                    if (b.Mods != "EX") continue;
                    if (currentFlow == null)
                    {
                        mapFlows.Add(currentFlow = new FillFlowContainer<EXBoardBeatmapPanel>
                        {
                            Spacing = new Vector2(10, 10),
                            Direction = FillDirection.Vertical,
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

                    currentFlow.Add(new EXBoardBeatmapPanel(b.Beatmap, b.Mods, b.ModIndex)
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
