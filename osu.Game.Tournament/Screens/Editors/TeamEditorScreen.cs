// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Online.API;
using osu.Game.Overlays.Settings;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Tournament.Screens.Editors
{
    public class TeamEditorScreen : TournamentEditorScreen<TeamEditorScreen.TeamRow, TournamentTeam>
    {
        [Resolved]
        private TournamentGameBase game { get; set; }

        protected override BindableList<TournamentTeam> Storage => LadderInfo.Teams;

        [BackgroundDependencyLoader]
        private void load()
        {
            ControlPanel.Add(new TourneyButton
            {
                RelativeSizeAxes = Axes.X,
                Text = "Add all countries",
                Action = addAllCountries
            });
        }

        protected override TeamRow CreateDrawable(TournamentTeam model) => new TeamRow(model, this);

        private void addAllCountries()
        {
            List<TournamentTeam> countries;
            using (Stream stream = game.Resources.GetStream("Resources/countries.json"))
            using (var sr = new StreamReader(stream))
                countries = JsonConvert.DeserializeObject<List<TournamentTeam>>(sr.ReadToEnd());

            foreach (var c in countries)
                Storage.Add(c);
        }

        public class TeamRow : CompositeDrawable, IModelBacked<TournamentTeam>
        {
            public TournamentTeam Model { get; }

            private readonly Container drawableContainer;

            [Resolved(canBeNull: true)]
            private TournamentSceneManager sceneManager { get; set; }

            [Resolved]
            private LadderInfo ladderInfo { get; set; }

            public TeamRow(TournamentTeam team, TournamentScreen parent)
            {
                Model = team;

                Masking = true;
                CornerRadius = 10;

                PlayerEditor playerEditor = new PlayerEditor(Model)
                {
                    Width = 0.95f
                };

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        Colour = OsuColour.Gray(0.1f),
                        RelativeSizeAxes = Axes.Both,
                    },
                    drawableContainer = new Container
                    {
                        Size = new Vector2(100, 50),
                        Margin = new MarginPadding(10),
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                    },
                    new FillFlowContainer
                    {
                        Margin = new MarginPadding(5),
                        Spacing = new Vector2(5),
                        Direction = FillDirection.Full,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            new SettingsTextBox
                            {
                                LabelText = "Name",
                                Width = 0.2f,
                                Current = Model.FullName
                            },
                            new SettingsTextBox
                            {
                                LabelText = "Acronym",
                                Width = 0.2f,
                                Current = Model.Acronym
                            },
                            new SettingsTextBox
                            {
                                LabelText = "Flag",
                                Width = 0.2f,
                                Current = Model.FlagName
                            },
                            new SettingsTextBox
                            {
                                LabelText = "Seed",
                                Width = 0.2f,
                                Current = Model.Seed
                            },
                            new SettingsSlider<int>
                            {
                                LabelText = "Last Year Placement",
                                Width = 0.33f,
                                Current = Model.LastYearPlacing
                            },
                            new SettingsButton
                            {
                                Width = 0.11f,
                                Margin = new MarginPadding(10),
                                Text = "Add player",
                                Action = () => playerEditor.CreateNew()
                            },
                            new DangerousSettingsButton
                            {
                                Width = 0.11f,
                                Text = "Delete Team",
                                Margin = new MarginPadding(10),
                                Action = () =>
                                {
                                    Expire();
                                    ladderInfo.Teams.Remove(Model);
                                },
                            },
                            playerEditor,
                            new SettingsButton
                            {
                                Width = 0.2f,
                                Margin = new MarginPadding(10),
                                Text = "Edit seeding results",
                                Action = () =>
                                {
                                    sceneManager?.SetScreen(new SeedingEditorScreen(team, parent));
                                }
                            },
                        }
                    },
                };

                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;

                Model.FlagName.BindValueChanged(updateDrawable, true);
            }

            private void updateDrawable(ValueChangedEvent<string> flag)
            {
                drawableContainer.Child = new DrawableTeamFlag(Model);
            }

            public class PlayerEditor : CompositeDrawable
            {
                private readonly TournamentTeam team;
                private readonly FillFlowContainer flow;

                public PlayerEditor(TournamentTeam team)
                {
                    this.team = team;

                    RelativeSizeAxes = Axes.X;
                    AutoSizeAxes = Axes.Y;

                    InternalChild = flow = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        ChildrenEnumerable = team.Players.Select(p => new PlayerRow(team, p))
                    };
                }

                public void CreateNew()
                {
                    var user = new User();
                    team.Players.Add(user);
                    flow.Add(new PlayerRow(team, user));
                }

                public class PlayerRow : CompositeDrawable
                {
                    private readonly User user;

                    [Resolved]
                    protected IAPIProvider API { get; private set; }

                    [Resolved]
                    private TournamentGameBase game { get; set; }

                    private readonly Bindable<string> userId = new Bindable<string>();

                    private readonly Container drawableContainer;

                    public PlayerRow(TournamentTeam team, User user)
                    {
                        this.user = user;

                        Margin = new MarginPadding(10);

                        RelativeSizeAxes = Axes.X;
                        AutoSizeAxes = Axes.Y;

                        Masking = true;
                        CornerRadius = 5;

                        InternalChildren = new Drawable[]
                        {
                            new Box
                            {
                                Colour = OsuColour.Gray(0.2f),
                                RelativeSizeAxes = Axes.Both,
                            },
                            new FillFlowContainer
                            {
                                Margin = new MarginPadding(5),
                                Padding = new MarginPadding { Right = 160 },
                                Spacing = new Vector2(5),
                                Direction = FillDirection.Horizontal,
                                AutoSizeAxes = Axes.Both,
                                Children = new Drawable[]
                                {
                                    new SettingsNumberBox
                                    {
                                        LabelText = "User ID",
                                        RelativeSizeAxes = Axes.None,
                                        Width = 200,
                                        Current = userId,
                                    },
                                    drawableContainer = new Container
                                    {
                                        Size = new Vector2(100, 70),
                                    },
                                }
                            },
                            new DangerousSettingsButton
                            {
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                RelativeSizeAxes = Axes.None,
                                Width = 150,
                                Text = "Delete Player",
                                Action = () =>
                                {
                                    Expire();
                                    team.Players.Remove(user);
                                },
                            }
                        };
                    }

                    [BackgroundDependencyLoader]
                    private void load()
                    {
                        userId.Value = user.Id.ToString();
                        userId.BindValueChanged(idString =>
                        {
                            int.TryParse(idString.NewValue, out var parsed);

                            user.Id = parsed;

                            if (idString.NewValue != idString.OldValue)
                                user.Username = string.Empty;

                            if (!string.IsNullOrEmpty(user.Username))
                            {
                                updatePanel();
                                return;
                            }

                            game.PopulateUser(user, updatePanel, updatePanel);
                        }, true);
                    }

                    private void updatePanel()
                    {
                        drawableContainer.Child = new UserGridPanel(user) { Width = 300 };
                    }
                }
            }
        }
    }
}
