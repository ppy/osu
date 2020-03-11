// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
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
    public class MapPoolScreen : TournamentScreen
    {
        private readonly FillFlowContainer<FillFlowContainer<TournamentBeatmapPanel>> mapFlows;

        private readonly Bindable<TournamentMatch> currentMatch = new Bindable<TournamentMatch>();

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
                new TourneyVideo("gameplay")
                {
                    Loop = true,
                    RelativeSizeAxes = Axes.Both,
                },
                new MatchHeader(),
                mapFlows = new FillFlowContainer<FillFlowContainer<TournamentBeatmapPanel>>
                {
                    Y = 100,
                    Spacing = new Vector2(10, 10),
                    Padding = new MarginPadding(25),
                    Direction = FillDirection.Vertical,
                    RelativeSizeAxes = Axes.Both,
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
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(MatchIPCInfo ipc)
        {
            currentMatch.BindValueChanged(matchChanged);
            currentMatch.BindTo(LadderInfo.CurrentMatch);

            ipc.Beatmap.BindValueChanged(beatmapChanged);
        }

        private void beatmapChanged(ValueChangedEvent<BeatmapInfo> beatmap)
        {
            if (currentMatch.Value == null || currentMatch.Value.PicksBans.Count(p => p.Type == ChoiceType.Ban) < 2)
                return;

            // if bans have already been placed, beatmap changes result in a selection being made autoamtically
            if (beatmap.NewValue.OnlineBeatmapID != null)
                addForBeatmap(beatmap.NewValue.OnlineBeatmapID.Value);
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

            var nextColour = (currentMatch.Value.PicksBans.LastOrDefault()?.Team ?? roll_winner) == TeamColour.Red ? TeamColour.Blue : TeamColour.Red;

            if (pickType == ChoiceType.Ban && currentMatch.Value.PicksBans.Count(p => p.Type == ChoiceType.Ban) >= 2)
                setMode(pickColour, ChoiceType.Pick);
            else
                setMode(nextColour, currentMatch.Value.PicksBans.Count(p => p.Type == ChoiceType.Ban) >= 2 ? ChoiceType.Pick : ChoiceType.Ban);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            var maps = mapFlows.Select(f => f.FirstOrDefault(m => m.ReceivePositionalInputAt(e.ScreenSpaceMousePosition)));
            var map = maps.FirstOrDefault(m => m != null);

            if (map != null)
            {
                if (e.Button == MouseButton.Left && map.Beatmap.OnlineBeatmapID != null)
                    addForBeatmap(map.Beatmap.OnlineBeatmapID.Value);
                else
                {
                    var existing = currentMatch.Value.PicksBans.FirstOrDefault(p => p.BeatmapID == map.Beatmap.OnlineBeatmapID);

                    if (existing != null)
                    {
                        currentMatch.Value.PicksBans.Remove(existing);
                        setNextMode();
                    }
                }

                return true;
            }

            return base.OnMouseDown(e);
        }

        private void reset()
        {
            currentMatch.Value.PicksBans.Clear();
            setNextMode();
        }

        private ScheduledDelegate scheduledChange;

        private void addForBeatmap(int beatmapId)
        {
            if (currentMatch.Value == null)
                return;

            if (currentMatch.Value.Round.Value.Beatmaps.All(b => b.BeatmapInfo.OnlineBeatmapID != beatmapId))
                // don't attempt to add if the beatmap isn't in our pool
                return;

            if (currentMatch.Value.PicksBans.Any(p => p.BeatmapID == beatmapId))
                // don't attempt to add if already exists.
                return;

            currentMatch.Value.PicksBans.Add(new BeatmapChoice
            {
                Team = pickColour,
                Type = pickType,
                BeatmapID = beatmapId
            });

            setNextMode();

            if (pickType == ChoiceType.Pick && currentMatch.Value.PicksBans.Any(i => i.Type == ChoiceType.Pick))
            {
                scheduledChange?.Cancel();
                scheduledChange = Scheduler.AddDelayed(() => { sceneManager?.SetScreen(typeof(GameplayScreen)); }, 10000);
            }
        }

        private void matchChanged(ValueChangedEvent<TournamentMatch> match)
        {
            mapFlows.Clear();

            if (match.NewValue.Round.Value != null)
            {
                FillFlowContainer<TournamentBeatmapPanel> currentFlow = null;
                string currentMod = null;

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
                    }

                    currentFlow.Add(new TournamentBeatmapPanel(b.BeatmapInfo, b.Mods)
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                    });
                }
            }
        }
    }
}
