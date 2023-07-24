// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Overlays.Settings;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Drawings.Components;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Tournament.Screens.Editors
{
    public class ImportedTeam
    {
        [JsonProperty("acronym")]
        public string Acronym { get; set; } = string.Empty;

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("flag")]
        public string Flag { get; set; } = string.Empty;

        [JsonProperty("seed")]
        public string Seed { get; set; } = string.Empty;

        [JsonProperty("last_year_placement")]
        public int LastYearPlacement { get; set; }

        [JsonProperty("players")]
        public IEnumerable<int> Players { get; set; } = new List<int>();

        public override string ToString()
        {
            return $"{(Name.Length > 0 ? Name : "<no name>")} ({(Acronym.Length > 0 ? Acronym : "<no acronym>")})";
        }
    }

    public partial class TeamEditorScreen : TournamentEditorScreen<TeamEditorScreen.TeamRow, TournamentTeam>
    {
        protected override BindableList<TournamentTeam> Storage => LadderInfo.Teams;
        private SettingsTextBox? teamJsonTextBox;

        [BackgroundDependencyLoader]
        private void load()
        {
            ControlPanel.Add(new TourneyButton
            {
                RelativeSizeAxes = Axes.X,
                Text = "Add all countries",
                Action = addAllCountries
            });

            ControlPanel.Add(teamJsonTextBox = new TournamentEditorJsonTextBox
            {
                RelativeSizeAxes = Axes.X,
                LabelText = "Rounds to import JSON"
            });
            ControlPanel.Add(new TourneyButton
            {
                RelativeSizeAxes = Axes.X,
                Text = "Import teams",
                Action = importTeamsJson
            });
        }

        private void importTeamsJson()
        {
            var teams = JsonConvert.DeserializeObject<List<ImportedTeam>>(teamJsonTextBox?.Current.Value ?? "[]",
                new JsonSerializerSettings
                {
                    Error = delegate(object? _, ErrorEventArgs args)
                    {
                        args.ErrorContext.Handled = true;
                        teamJsonTextBox?.SetNoticeText("Unable to parse JSON, please check your input");
                    }
                });

            if (teams == null) return;

            foreach (var team in teams)
            {
                // todo: check if team we're about to import already exists and skip if we're about to create a duplicate?
                var newTeam = new TournamentTeam
                {
                    FlagName = { Value = team.Flag },
                    FullName = { Value = team.Name },
                    Acronym = { Value = team.Acronym },
                    Seed = { Value = team.Seed },
                    LastYearPlacing = { Value = team.LastYearPlacement },
                };

                newTeam.Players.AddRange(team.Players.Select(player => new TournamentUser { OnlineID = player }));

                Storage.Add(newTeam);
            }
        }

        protected override TeamRow CreateDrawable(TournamentTeam model) => new TeamRow(model, this);

        private void addAllCountries()
        {
            var countries = new List<TournamentTeam>();

            foreach (var country in Enum.GetValues<CountryCode>().Skip(1))
            {
                countries.Add(new TournamentTeam
                {
                    FlagName = { Value = country.ToString() },
                    FullName = { Value = country.GetDescription() },
                    Acronym = { Value = country.GetAcronym() },
                });
            }

            foreach (var c in countries)
                Storage.Add(c);
        }

        public partial class TeamRow : CompositeDrawable, IModelBacked<TournamentTeam>
        {
            public TournamentTeam Model { get; }

            [Resolved]
            private TournamentSceneManager? sceneManager { get; set; }

            [Resolved]
            private LadderInfo ladderInfo { get; set; } = null!;

            public TeamRow(TournamentTeam team, TournamentScreen parent)
            {
                Model = team;

                Masking = true;
                CornerRadius = 10;

                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;

                PlayerEditor playerEditor = new PlayerEditor(Model);

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        Colour = OsuColour.Gray(0.1f),
                        RelativeSizeAxes = Axes.Both,
                    },
                    new GroupTeam(team)
                    {
                        Margin = new MarginPadding(16),
                        Scale = new Vector2(2),
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                    },
                    new FillFlowContainer
                    {
                        Spacing = new Vector2(5),
                        Padding = new MarginPadding(10),
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
                                Width = 0.2f,
                                Margin = new MarginPadding(10),
                                Text = "Edit seeding results",
                                Action = () =>
                                {
                                    sceneManager?.SetScreen(new SeedingEditorScreen(team, parent));
                                }
                            },
                            playerEditor,
                            new SettingsButton
                            {
                                Text = "Add player",
                                Action = () => playerEditor.CreateNew()
                            },
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Children = new Drawable[]
                                {
                                    new DangerousSettingsButton
                                    {
                                        Width = 0.2f,
                                        Text = "Delete Team",
                                        Anchor = Anchor.TopRight,
                                        Origin = Anchor.TopRight,
                                        Action = () =>
                                        {
                                            Expire();
                                            ladderInfo.Teams.Remove(Model);
                                        },
                                    },
                                }
                            },
                        }
                    },
                };
            }

            public partial class PlayerEditor : CompositeDrawable
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
                        Padding = new MarginPadding(5),
                        Spacing = new Vector2(5),
                        ChildrenEnumerable = team.Players.Select(p => new PlayerRow(team, p))
                    };
                }

                public void CreateNew()
                {
                    var player = new TournamentUser();
                    team.Players.Add(player);
                    flow.Add(new PlayerRow(team, player));
                }

                public partial class PlayerRow : CompositeDrawable
                {
                    private readonly TournamentUser user;

                    [Resolved]
                    private TournamentGameBase game { get; set; } = null!;

                    private readonly Bindable<int?> playerId = new Bindable<int?>();

                    private readonly Container userPanelContainer;

                    public PlayerRow(TournamentTeam team, TournamentUser user)
                    {
                        this.user = user;

                        RelativeSizeAxes = Axes.X;
                        AutoSizeAxes = Axes.Y;

                        Masking = true;
                        CornerRadius = 10;

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
                                Padding = new MarginPadding { Right = 60 },
                                Spacing = new Vector2(5),
                                Direction = FillDirection.Horizontal,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Children = new Drawable[]
                                {
                                    new SettingsNumberBox
                                    {
                                        LabelText = "User ID",
                                        RelativeSizeAxes = Axes.None,
                                        Width = 200,
                                        Current = playerId,
                                    },
                                    userPanelContainer = new Container
                                    {
                                        Width = 400,
                                        RelativeSizeAxes = Axes.Y,
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
                        playerId.Value = user.OnlineID;
                        playerId.BindValueChanged(id =>
                        {
                            user.OnlineID = id.NewValue ?? 0;

                            if (id.NewValue != id.OldValue)
                                user.Username = string.Empty;

                            if (!string.IsNullOrEmpty(user.Username))
                            {
                                updatePanel();
                                return;
                            }

                            game.PopulatePlayer(user, updatePanel, updatePanel);
                        }, true);
                    }

                    private void updatePanel() => Scheduler.AddOnce(() =>
                    {
                        userPanelContainer.Child = new UserListPanel(user.ToAPIUser())
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Scale = new Vector2(1f),
                        };
                    });
                }
            }
        }
    }
}
