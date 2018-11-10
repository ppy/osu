// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.IPC;
using osu.Game.Tournament.Screens.Gameplay;
using osu.Game.Tournament.Screens.Ladder.Components;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;

namespace osu.Game.Tournament.Screens.MapPool
{
    public class MapPoolScreen : TournamentScreen
    {
        private readonly FillFlowContainer<TournamentBeatmapPanel> maps;

        private readonly Bindable<MatchPairing> currentMatch = new Bindable<MatchPairing>();

        private TeamColour pickColour;
        private ChoiceType pickType;

        private readonly TriangleButton buttonRedBan;
        private readonly TriangleButton buttonBlueBan;
        private readonly TriangleButton buttonRedPick;
        private readonly TriangleButton buttonBluePick;

        public MapPoolScreen()
        {
            InternalChildren = new Drawable[]
            {
                new MatchHeader(),
                maps = new FillFlowContainer<TournamentBeatmapPanel>
                {
                    Y = 100,
                    Spacing = new Vector2(10),
                    Padding = new MarginPadding(50),
                    Direction = FillDirection.Full,
                    RelativeSizeAxes = Axes.Both,
                },
                new ControlPanel
                {
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Text = "Current Mode"
                        },
                        buttonRedBan = new TriangleButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Red Ban",
                            Action = () => setMode(TeamColour.Red, ChoiceType.Ban)
                        },
                        buttonBlueBan = new TriangleButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Blue Ban",
                            Action = () => setMode(TeamColour.Blue, ChoiceType.Ban)
                        },
                        buttonRedPick = new TriangleButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Red Pick",
                            Action = () => setMode(TeamColour.Red, ChoiceType.Pick)
                        },
                        buttonBluePick = new TriangleButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Blue Pick",
                            Action = () => setMode(TeamColour.Blue, ChoiceType.Pick)
                        },
                        new ControlPanel.Spacer(),
                        new TriangleButton
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
        private void load(LadderInfo ladder, FileBasedIPC ipc)
        {
            currentMatch.BindValueChanged(matchChanged);
            currentMatch.BindTo(ladder.CurrentMatch);

            ipc.Beatmap.BindValueChanged(beatmapChanged);
        }

        private void beatmapChanged(BeatmapInfo beatmap)
        {
            if (currentMatch.Value.PicksBans.Count(p => p.Type == ChoiceType.Ban) < 2)
                return;

            if (beatmap.OnlineBeatmapID != null)
                addForBeatmap(beatmap.OnlineBeatmapID.Value);
        }

        private void setMode(TeamColour colour, ChoiceType choiceType)
        {
            pickColour = colour;
            pickType = choiceType;

            Color4 setColour(bool active) => active ? Color4.White : Color4.Gray;

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
            var map = maps.FirstOrDefault(m => m.ReceivePositionalInputAt(e.ScreenSpaceMousePosition));
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

        private void addForBeatmap(int beatmapId)
        {
            if (currentMatch.Value == null)
                return;

            if (currentMatch.Value.Grouping.Value.Beatmaps.All(b => b.BeatmapInfo.OnlineBeatmapID != beatmapId))
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
        }

        private void matchChanged(MatchPairing match)
        {
            maps.Clear();

            if (match.Grouping.Value != null)
            {
                foreach (var b in match.Grouping.Value.Beatmaps)
                    maps.Add(new TournamentBeatmapPanel(b.BeatmapInfo)
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                    });
            }
        }
    }
}
