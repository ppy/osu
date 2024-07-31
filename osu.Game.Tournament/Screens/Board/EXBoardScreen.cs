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
using osu.Game.Overlays.Settings;
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

        private TeamColour pickColour;
        private ChoiceType pickType;

        private OsuButton buttonPick = null!;
        private OsuButton buttonRedWin = null!;
        private OsuButton buttonBlueWin = null!;
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

                mapFlows = new FillFlowContainer<FillFlowContainer<EXBoardBeatmapPanel>>
                {
                    Y = 160,
                    Spacing = new Vector2(10, 10),
                    Direction = FillDirection.Vertical,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
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
                        new SettingsTextBox
                        {
                            LabelText = "EX1 mapID",
                            Width = 0.2f,
                        },
                        new SettingsTextBox
                        {
                            LabelText = "EX2 mapID",
                            Width = 0.2f,
                        },
                        new SettingsTextBox
                        {
                            LabelText = "EX3 mapID",
                            Width = 0.2f,
                        },
                        new SettingsTextBox
                        {
                            LabelText = "EX4 mapID",
                            Width = 0.2f,
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

            buttonPick.Colour = setColour(pickColour == TeamColour.Neutral && pickType == ChoiceType.Pick);
            buttonRedWin.Colour = setWin(pickColour == TeamColour.Red && pickType == ChoiceType.RedWin);
            buttonBlueWin.Colour = setWin(pickColour == TeamColour.Blue && pickType == ChoiceType.BlueWin);
            buttonDraw.Colour = setColour(pickColour == TeamColour.Neutral && pickType == ChoiceType.Draw);

            static Color4 setColour(bool active) => active ? Color4.White : Color4.Gray;
            static Color4 setWin(bool active) => active ? Color4.White : Color4.Gray;
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
                    var existing = CurrentMatch.Value?.PicksBans.FirstOrDefault(p => p.BeatmapID == map.Beatmap?.OnlineID);

                    if (existing != null)
                    {
                        CurrentMatch.Value?.PicksBans.Remove(existing);
                    }
                }

                return true;
            }

            return base.OnMouseDown(e);
        }

        private void reset()
        {
            CurrentMatch.Value?.PicksBans.Clear();
        }

        private void addForBeatmap(int beatmapId)
        {
            if (CurrentMatch.Value?.Round.Value == null)
                return;

            if (CurrentMatch.Value.Round.Value.Beatmaps.All(b => b.Beatmap?.OnlineID != beatmapId))
                // don't attempt to add if the beatmap isn't in our pool
                return;

            if (CurrentMatch.Value.PicksBans.Any(p => p.BeatmapID == beatmapId))
                // don't attempt to add if already exists.
                return;

            CurrentMatch.Value.PicksBans.Add(new BeatmapChoice
            {
                Team = pickColour,
                Type = pickType,
                BeatmapID = beatmapId
            });

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
