// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.Game.Tournament.Components;
using osuTK;

namespace osu.Game.Tournament.Screens.Teams
{
    public class TeamsEditorScreen : TournamentScreen, IProvideVideo
    {
        private readonly FillFlowContainer<TeamRow> items;

        public TeamsEditorScreen()
        {
            AddRangeInternal(new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = OsuColour.Gray(0.2f),
                },
                new OsuScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.9f,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Child = items = new FillFlowContainer<TeamRow>
                    {
                        Direction = FillDirection.Vertical,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                    },
                },
                new ControlPanel
                {
                    Children = new Drawable[]
                    {
                        new TriangleButton
                        {
                            RelativeSizeAxes = Axes.X,
                            Text = "Add new",
                            Action = addNew
                        },
                    }
                }
            });
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            foreach (var g in LadderInfo.Teams)
                items.Add(new TeamRow(g));
        }

        private void addNew()
        {
            var team = new TournamentTeam();

            items.Add(new TeamRow(team));
            LadderInfo.Teams.Add(team);
        }

        public class TeamRow : CompositeDrawable
        {
            public readonly TournamentTeam Team;

            private readonly Container drawableContainer;

            [Resolved]
            private LadderInfo ladderInfo { get; set; }

            public TeamRow(TournamentTeam team)
            {
                Margin = new MarginPadding(10);

                Team = team;

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        Colour = OsuColour.Gray(0.1f),
                        RelativeSizeAxes = Axes.Both,
                    },
                    new FillFlowContainer
                    {
                        Margin = new MarginPadding(5),
                        Padding = new MarginPadding { Right = 160 },
                        Spacing = new Vector2(5),
                        Direction = FillDirection.Full,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            new SettingsTextBox
                            {
                                LabelText = "Name",
                                Width = 0.25f,
                                Bindable = Team.FullName
                            },
                            new SettingsTextBox
                            {
                                LabelText = "Acronym",
                                Width = 0.25f,
                                Bindable = Team.Acronym
                            },
                            new SettingsTextBox
                            {
                                LabelText = "Flag",
                                Width = 0.25f,
                                Bindable = Team.FlagName
                            },
                            drawableContainer = new Container
                            {
                                Width = 0.22f,
                                RelativeSizeAxes = Axes.X,
                                Height = 50,
                            },
                        }
                    },
                    new DangerousSettingsButton
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        RelativeSizeAxes = Axes.None,
                        Width = 150,
                        Text = "Delete",
                        Action = () =>
                        {
                            Expire();
                            ladderInfo.Teams.Remove(Team);
                        },
                    }
                };

                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;

                Team.FlagName.BindValueChanged(updateDrawable, true);
            }

            private void updateDrawable(ValueChangedEvent<string> flag)
            {
                drawableContainer.Child = new RowTeam(Team);
            }

            private class RowTeam : DrawableTournamentTeam
            {
                public RowTeam(TournamentTeam team)
                    : base(team)
                {
                    InternalChild = Flag;
                    RelativeSizeAxes = Axes.Both;

                    Flag.Anchor = Anchor.Centre;
                    Flag.Origin = Anchor.Centre;
                }
            }
        }
    }
}
