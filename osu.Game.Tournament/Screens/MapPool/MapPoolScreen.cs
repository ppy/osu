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
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.IPC;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Gameplay;
using osu.Game.Tournament.Screens.Gameplay.Components;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Tournament.Screens.MapPool
{
    public class MapPoolScreen : TournamentMatchScreen
    {
        private readonly FillFlowContainer<FillFlowContainer<TournamentBeatmapPanel>> mapFlows;

        [Resolved(canBeNull: true)]
        private TournamentSceneManager sceneManager { get; set; }

        private TeamColour pickColour;
        private ChoiceType pickType;

        private readonly OsuButton buttonRedBan;
        private readonly OsuButton buttonBlueBan;
        private readonly OsuButton buttonRedPick;
        private readonly OsuButton buttonBluePick;

        public MapPoolScreen()
        {
            InternalChildren = new Drawable[]
            {
                new TourneyVideo("mappool")
                {
                    Loop = true,
                    RelativeSizeAxes = Axes.Both,
                },
                new MatchHeader(),
                mapFlows = new FillFlowContainer<FillFlowContainer<TournamentBeatmapPanel>>
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
                        buttonRedBan = new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Red Ban",
                            Action = () => setMode(TeamColour.Red, ChoiceType.Ban)
                        },
                        buttonBlueBan = new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Blue Ban",
                            Action = () => setMode(TeamColour.Blue, ChoiceType.Ban)
                        },
                        buttonRedPick = new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Red Pick",
                            Action = () => setMode(TeamColour.Red, ChoiceType.Pick)
                        },
                        buttonBluePick = new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Blue Pick",
                            Action = () => setMode(TeamColour.Blue, ChoiceType.Pick)
                        },
                        new ControlPanel.Spacer(),
                        new TourneyButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Reset",
                            Action = reset
                        },
                        new ControlPanel.Spacer(),
                    },
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(MatchIPCInfo ipc)
        {
            ipc.Beatmap.BindValueChanged(beatmapChanged);
        }

        private void beatmapChanged(ValueChangedEvent<APIBeatmap> beatmap)
        {
            if (CurrentMatch.Value == null || CurrentMatch.Value.PicksBans.Count(p => p.Type == ChoiceType.Ban) < 2)
                return;

            // if bans have already been placed, beatmap changes result in a selection being made autoamtically
            if (beatmap.NewValue.OnlineID > 0)
                addForBeatmap(beatmap.NewValue.OnlineID);
        }

        private void setMode(TeamColour colour, ChoiceType choiceType)
        {
            pickColour = colour;
            pickType = choiceType;

            static Color4 setColour(bool active) => active ? Color4.White : Color4.Gray;

            buttonRedBan.Colour = setColour(pickColour == TeamColour.Red && pickType == ChoiceType.Ban);
            buttonBlueBan.Colour = setColour(pickColour == TeamColour.Blue && pickType == ChoiceType.Ban);
            buttonRedPick.Colour = setColour(pickColour == TeamColour.Red && pickType == ChoiceType.Pick);
            buttonBluePick.Colour = setColour(pickColour == TeamColour.Blue && pickType == ChoiceType.Pick);
        }

        private void setNextMode()
        {
            const TeamColour roll_winner = TeamColour.Red; //todo: draw from match

            var nextColour = (CurrentMatch.Value.PicksBans.LastOrDefault()?.Team ?? roll_winner) == TeamColour.Red ? TeamColour.Blue : TeamColour.Red;

            if (pickType == ChoiceType.Ban && CurrentMatch.Value.PicksBans.Count(p => p.Type == ChoiceType.Ban) >= 2)
                setMode(pickColour, ChoiceType.Pick);
            else
                setMode(nextColour, CurrentMatch.Value.PicksBans.Count(p => p.Type == ChoiceType.Ban) >= 2 ? ChoiceType.Pick : ChoiceType.Ban);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            var maps = mapFlows.Select(f => f.FirstOrDefault(m => m.ReceivePositionalInputAt(e.ScreenSpaceMousePosition)));
            var map = maps.FirstOrDefault(m => m != null);

            if (map != null)
            {
                if (e.Button == MouseButton.Left && map.Beatmap.OnlineID > 0)
                    addForBeatmap(map.Beatmap.OnlineID);
                else
                {
                    var existing = CurrentMatch.Value.PicksBans.FirstOrDefault(p => p.BeatmapID == map.Beatmap.OnlineID);

                    if (existing != null)
                    {
                        CurrentMatch.Value.PicksBans.Remove(existing);
                        setNextMode();
                    }
                }

                return true;
            }

            return base.OnMouseDown(e);
        }

        private void reset()
        {
            CurrentMatch.Value.PicksBans.Clear();
            setNextMode();
        }

        private ScheduledDelegate scheduledChange;

        private void addForBeatmap(int beatmapId)
        {
            if (CurrentMatch.Value == null)
                return;

            if (CurrentMatch.Value.Round.Value.Beatmaps.All(b => b.Beatmap.OnlineID != beatmapId))
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

            setNextMode();

            if (pickType == ChoiceType.Pick && CurrentMatch.Value.PicksBans.Any(i => i.Type == ChoiceType.Pick))
            {
                scheduledChange?.Cancel();
                scheduledChange = Scheduler.AddDelayed(() => { sceneManager?.SetScreen(typeof(GameplayScreen)); }, 10000);
            }
        }

        protected override void CurrentMatchChanged(ValueChangedEvent<TournamentMatch> match)
        {
            base.CurrentMatchChanged(match);

            mapFlows.Clear();

            if (match.NewValue == null)
                return;

            int totalRows = 0;

            if (match.NewValue.Round.Value != null)
            {
                FillFlowContainer<TournamentBeatmapPanel> currentFlow = null;
                string currentMod = null;

                int flowCount = 0;

                foreach (var b in match.NewValue.Round.Value.Beatmaps)
                {
                    if (currentFlow == null || currentMod != b.Mods)
                    {
                        mapFlows.Add(currentFlow = new FillFlowContainer<TournamentBeatmapPanel>
                        {
                            Spacing = new Vector2(10, 5),
                            Direction = FillDirection.Full,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y
                        });

                        currentMod = b.Mods;

                        totalRows++;
                        flowCount = 0;
                    }

                    if (++flowCount > 2)
                    {
                        totalRows++;
                        flowCount = 1;
                    }

                    currentFlow.Add(new TournamentBeatmapPanel(b.Beatmap, b.Mods)
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Height = 42,
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
