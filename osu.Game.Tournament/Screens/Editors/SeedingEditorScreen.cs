﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.Settings;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Tournament.Screens.Editors
{
    public partial class SeedingEditorScreen : TournamentEditorScreen<SeedingEditorScreen.SeedingResultRow, SeedingResult>
    {
        private readonly TournamentTeam team;

        protected override BindableList<SeedingResult> Storage => team.SeedingResults;

        public SeedingEditorScreen(TournamentTeam team, TournamentScreen parentScreen)
            : base(parentScreen)
        {
            this.team = team;
        }

        public partial class SeedingResultRow : CompositeDrawable, IModelBacked<SeedingResult>
        {
            public SeedingResult Model { get; }

            public SeedingResultRow(TournamentTeam team, SeedingResult round)
            {
                Model = round;

                Masking = true;
                CornerRadius = 10;

                SeedingBeatmapEditor beatmapEditor = new SeedingBeatmapEditor(round)
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
                                LabelText = "Mod",
                                Width = 0.33f,
                                Current = Model.Mod
                            },
                            new SettingsSlider<int>
                            {
                                LabelText = "Seed",
                                Width = 0.33f,
                                Current = Model.Seed
                            },
                            new SettingsButton
                            {
                                Width = 0.2f,
                                Margin = new MarginPadding(10),
                                Text = "Add beatmap",
                                Action = () => beatmapEditor.CreateNew()
                            },
                            beatmapEditor
                        }
                    },
                    new DangerousSettingsButton
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        RelativeSizeAxes = Axes.None,
                        Width = 150,
                        Text = "Delete result",
                        Action = () =>
                        {
                            Expire();
                            team.SeedingResults.Remove(Model);
                        },
                    }
                };

                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
            }

            public partial class SeedingBeatmapEditor : CompositeDrawable
            {
                private readonly SeedingResult round;
                private readonly FillFlowContainer flow;

                public SeedingBeatmapEditor(SeedingResult round)
                {
                    this.round = round;

                    RelativeSizeAxes = Axes.X;
                    AutoSizeAxes = Axes.Y;

                    InternalChild = flow = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        ChildrenEnumerable = round.Beatmaps.Select(p => new SeedingBeatmapRow(round, p))
                    };
                }

                public void CreateNew()
                {
                    var user = new SeedingBeatmap();
                    round.Beatmaps.Add(user);
                    flow.Add(new SeedingBeatmapRow(round, user));
                }

                public partial class SeedingBeatmapRow : CompositeDrawable
                {
                    private readonly SeedingResult result;
                    public SeedingBeatmap Model { get; }

                    [Resolved]
                    protected IAPIProvider API { get; private set; } = null!;

                    private readonly Bindable<int?> beatmapId = new Bindable<int?>();

                    private readonly Bindable<string> score = new Bindable<string>(string.Empty);

                    private readonly Container drawableContainer;

                    public SeedingBeatmapRow(SeedingResult result, SeedingBeatmap beatmap)
                    {
                        this.result = result;
                        Model = beatmap;

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
                                        LabelText = "Beatmap ID",
                                        RelativeSizeAxes = Axes.None,
                                        Width = 200,
                                        Current = beatmapId,
                                    },
                                    new SettingsSlider<int>
                                    {
                                        LabelText = "Seed",
                                        RelativeSizeAxes = Axes.None,
                                        Width = 200,
                                        Current = beatmap.Seed
                                    },
                                    new SettingsTextBox
                                    {
                                        LabelText = "Score",
                                        RelativeSizeAxes = Axes.None,
                                        Width = 200,
                                        Current = score,
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
                                Text = "Delete Beatmap",
                                Action = () =>
                                {
                                    Expire();
                                    result.Beatmaps.Remove(beatmap);
                                },
                            }
                        };
                    }

                    [BackgroundDependencyLoader]
                    private void load()
                    {
                        beatmapId.Value = Model.ID;
                        beatmapId.BindValueChanged(id =>
                        {
                            Model.ID = id.NewValue ?? 0;

                            if (id.NewValue != id.OldValue)
                                Model.Beatmap = null;

                            if (Model.Beatmap != null)
                            {
                                updatePanel();
                                return;
                            }

                            var req = new GetBeatmapRequest(new APIBeatmap { OnlineID = Model.ID });

                            req.Success += res => Schedule(() =>
                            {
                                Model.Beatmap = new TournamentBeatmap(res);
                                updatePanel();
                            });

                            req.Failure += _ => Schedule(() =>
                            {
                                Model.Beatmap = null;
                                updatePanel();
                            });

                            API.Queue(req);
                        }, true);

                        score.Value = Model.Score.ToString();
                        score.BindValueChanged(str => long.TryParse(str.NewValue, out Model.Score));
                    }

                    private void updatePanel()
                    {
                        drawableContainer.Clear();

                        if (Model.Beatmap != null)
                        {
                            drawableContainer.Child = new TournamentBeatmapPanel(Model.Beatmap, result.Mod.Value)
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Width = 300
                            };
                        }
                    }
                }
            }
        }

        protected override SeedingResultRow CreateDrawable(SeedingResult model) => new SeedingResultRow(team, model);
    }
}
