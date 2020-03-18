// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osuTK;

namespace osu.Game.Tournament.Screens.Editors
{
    public class SeedingEditorScreen : TournamentEditorScreen<SeedingEditorScreen.SeeingResultRow, SeedingResult>
    {
        private readonly TournamentTeam team;

        protected override BindableList<SeedingResult> Storage => team.SeedingResults;

        public SeedingEditorScreen(TournamentTeam team)
        {
            this.team = team;
        }

        public class SeeingResultRow : CompositeDrawable, IModelBacked<SeedingResult>
        {
            public SeedingResult Model { get; }

            [Resolved]
            private LadderInfo ladderInfo { get; set; }

            public SeeingResultRow(TournamentTeam team, SeedingResult round)
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
                                Bindable = Model.Mod
                            },
                            new SettingsSlider<int>
                            {
                                LabelText = "Seed",
                                Width = 0.33f,
                                Bindable = Model.Seed
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

            public class SeedingBeatmapEditor : CompositeDrawable
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

                public class SeedingBeatmapRow : CompositeDrawable
                {
                    private readonly SeedingResult result;
                    public SeedingBeatmap Model { get; }

                    [Resolved]
                    protected IAPIProvider API { get; private set; }

                    private readonly Bindable<string> beatmapId = new Bindable<string>();

                    private readonly Bindable<string> score = new Bindable<string>();

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
                                        Bindable = beatmapId,
                                    },
                                    new SettingsSlider<int>
                                    {
                                        LabelText = "Seed",
                                        RelativeSizeAxes = Axes.None,
                                        Width = 200,
                                        Bindable = beatmap.Seed
                                    },
                                    new SettingsTextBox
                                    {
                                        LabelText = "Score",
                                        RelativeSizeAxes = Axes.None,
                                        Width = 200,
                                        Bindable = score,
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
                    private void load(RulesetStore rulesets)
                    {
                        beatmapId.Value = Model.ID.ToString();
                        beatmapId.BindValueChanged(idString =>
                        {
                            int parsed;

                            int.TryParse(idString.NewValue, out parsed);

                            Model.ID = parsed;

                            if (idString.NewValue != idString.OldValue)
                                Model.BeatmapInfo = null;

                            if (Model.BeatmapInfo != null)
                            {
                                updatePanel();
                                return;
                            }

                            var req = new GetBeatmapRequest(new BeatmapInfo { OnlineBeatmapID = Model.ID });

                            req.Success += res =>
                            {
                                Model.BeatmapInfo = res.ToBeatmap(rulesets);
                                updatePanel();
                            };

                            req.Failure += _ =>
                            {
                                Model.BeatmapInfo = null;
                                updatePanel();
                            };

                            API.Queue(req);
                        }, true);

                        score.Value = Model.Score.ToString();
                        score.BindValueChanged(str => long.TryParse(str.NewValue, out Model.Score));
                    }

                    private void updatePanel()
                    {
                        drawableContainer.Clear();

                        if (Model.BeatmapInfo != null)
                        {
                            drawableContainer.Child = new TournamentBeatmapPanel(Model.BeatmapInfo, result.Mod.Value)
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

        protected override SeeingResultRow CreateDrawable(SeedingResult model) => new SeeingResultRow(team, model);
    }
}
