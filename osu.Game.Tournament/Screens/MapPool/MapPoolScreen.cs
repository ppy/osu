// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Screens.Ladder.Components;
using OpenTK;
using OpenTK.Input;

namespace osu.Game.Tournament.Screens.MapPool
{
    public class MapPoolScreen : OsuScreen
    {
        private readonly FillFlowContainer<TournamentBeatmapPanel> maps;

        private readonly Bindable<MatchPairing> currentMatch = new Bindable<MatchPairing>();

        public MapPoolScreen()
        {
            InternalChildren = new Drawable[]
            {
                maps = new FillFlowContainer<TournamentBeatmapPanel>
                {
                    Spacing = new Vector2(20),
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
                        }
                    }
                }
            };
        }

        private TeamColour pickColour;
        private ChoiceType pickType;

        private readonly TriangleButton buttonRedBan;
        private readonly TriangleButton buttonBlueBan;
        private readonly TriangleButton buttonRedPick;
        private readonly TriangleButton buttonBluePick;

        private void setMode(TeamColour colour, ChoiceType choiceType)
        {
            pickColour = colour;
            pickType = choiceType;

            var enabled = currentMatch.Value.PicksBans.Count == 0;

            buttonRedBan.Enabled.Value = enabled || pickColour == TeamColour.Red && pickType == ChoiceType.Ban;
            buttonBlueBan.Enabled.Value = enabled || pickColour == TeamColour.Blue && pickType == ChoiceType.Ban;
            buttonRedPick.Enabled.Value = enabled || pickColour == TeamColour.Red && pickType == ChoiceType.Pick;
            buttonBluePick.Enabled.Value = enabled || pickColour == TeamColour.Blue && pickType == ChoiceType.Pick;
        }

        private void setNextMode()
        {
            const TeamColour roll_winner = TeamColour.Red; //todo: draw from match

            var nextColour = (currentMatch.Value.PicksBans.LastOrDefault()?.Team ?? roll_winner) == TeamColour.Red ? TeamColour.Blue : TeamColour.Red;

            setMode(nextColour, currentMatch.Value.PicksBans.Count(p => p.Type == ChoiceType.Ban) >= 2 ? ChoiceType.Pick : ChoiceType.Ban);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            var map = maps.FirstOrDefault(m => m.ReceivePositionalInputAt(e.ScreenSpaceMousePosition));
            if (map != null)
            {
                if (e.Button == MouseButton.Left)
                {
                    currentMatch.Value.PicksBans.Add(new BeatmapChoice
                    {
                        Team = pickColour,
                        Type = pickType,
                        BeatmapID = map.Beatmap.OnlineBeatmapID ?? -1
                    });

                    setNextMode();
                }
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

        [BackgroundDependencyLoader]
        private void load(LadderInfo ladder)
        {
            currentMatch.BindValueChanged(matchChanged);
            currentMatch.BindTo(ladder.CurrentMatch);
        }

        private void matchChanged(MatchPairing match)
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
