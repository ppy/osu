// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Models;
using osu.Game.Tournament.Screens.Editors.Components;
using osuTK;

namespace osu.Game.Tournament.Screens.Editors
{
    public partial class RoundEditorScreen : TournamentEditorScreen<RoundEditorScreen.RoundRow, TournamentRound>
    {
        protected override BindableList<TournamentRound> Storage => LadderInfo.Rounds;

        public partial class RoundRow : CompositeDrawable, IModelBacked<TournamentRound>
        {
            public TournamentRound Model { get; }

            [Resolved]
            private LadderInfo ladderInfo { get; set; } = null!;

            [Resolved]
            private IDialogOverlay? dialogOverlay { get; set; }

            public RoundRow(TournamentRound round)
            {
                Model = round;

                Masking = true;
                CornerRadius = 10;

                RoundBeatmapEditor beatmapEditor = new RoundBeatmapEditor(round)
                {
                    Width = 0.98f
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
                        // Padding = new MarginPadding { Right = 160 },
                        Spacing = new Vector2(5),
                        Direction = FillDirection.Full,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            new SettingsTextBox
                            {
                                LabelText = "Name",
                                Width = 0.33f,
                                Current = Model.Name
                            },
                            new SettingsTextBox
                            {
                                LabelText = "Description",
                                Width = 0.33f,
                                Current = Model.Description
                            },
                            new DateTextBox
                            {
                                LabelText = "Start Time",
                                Width = 0.33f,
                                Current = Model.StartDate
                            },
                            new SettingsSlider<int>
                            {
                                LabelText = "# of Bans",
                                Width = 0.33f,
                                Current = Model.BanCount
                            },
                            new SettingsSlider<int>
                            {
                                LabelText = "Best of",
                                Width = 0.33f,
                                Current = Model.BestOf
                            },
                            new OsuCheckbox
                            {
                                LabelText = "Board Mode",
                                Width = 0.2f,
                                Current = Model.UseBoard,
                            },
                            new SettingsNumberBox
                            {
                                LabelText = "Referee Bot ID",
                                Width = 0.25f,
                                Current = Model.RefereeId,
                            },
                            new OsuCheckbox
                            {
                                LabelText = "Trust All Special Commands",
                                Width = 0.25f,
                                Current = Model.TrustAll,
                            },
                            new DangerousSettingsButton
                            {
                                Width = 0.2f,
                                Text = "Delete Round",
                                Action = () => dialogOverlay?.Push(new DeleteRoundDialog(Model, () =>
                                {
                                    Expire();
                                    ladderInfo.Rounds.Remove(Model);
                                }))
                            },
                            beatmapEditor,
                            new SettingsButton
                            {
                                Text = "Add beatmap",
                                Margin = new MarginPadding { Top = 10, Bottom = 10 },
                                Action = () => beatmapEditor.CreateNew()
                            },
                        }
                    },
                };

                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
            }

            public partial class RoundBeatmapEditor : CompositeDrawable
            {
                private readonly TournamentRound round;
                private readonly FillFlowContainer flow;

                public RoundBeatmapEditor(TournamentRound round)
                {
                    this.round = round;

                    RelativeSizeAxes = Axes.X;
                    AutoSizeAxes = Axes.Y;

                    InternalChild = flow = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        ChildrenEnumerable = round.Beatmaps.Select(p => new RoundBeatmapRow(round, p))
                    };
                }

                public void CreateNew()
                {
                    var b = new RoundBeatmap();

                    round.Beatmaps.Add(b);

                    flow.Add(new RoundBeatmapRow(round, b));
                }

                public partial class RoundBeatmapRow : CompositeDrawable
                {
                    public RoundBeatmap Model { get; }

                    [Resolved]
                    protected IAPIProvider API { get; private set; } = null!;

                    [Resolved]
                    private IDialogOverlay? dialogOverlay { get; set; }

                    private readonly Bindable<int?> beatmapId = new Bindable<int?>();

                    private readonly Bindable<string> modIndex = new Bindable<string>(string.Empty);

                    private readonly Bindable<string> mods = new Bindable<string>(string.Empty);

                    private readonly Bindable<int?> boardX = new Bindable<int?>();
                    private readonly Bindable<int?> boardY = new Bindable<int?>();

                    private readonly Container drawableContainer;

                    public RoundBeatmapRow(TournamentRound team, RoundBeatmap beatmap)
                    {
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
                                Padding = new MarginPadding { Right = 10 },
                                Spacing = new Vector2(5),
                                Direction = FillDirection.Horizontal,
                                AutoSizeAxes = Axes.Both,
                                Children = new Drawable[]
                                {
                                    new SettingsNumberBox
                                    {
                                        LabelText = "Beatmap ID",
                                        RelativeSizeAxes = Axes.None,
                                        Width = 125,
                                        Current = beatmapId,
                                    },
                                    new SettingsTextBox
                                    {
                                        LabelText = "Mods",
                                        RelativeSizeAxes = Axes.None,
                                        Width = 100,
                                        Current = mods,
                                    },
                                    new SettingsTextBox
                                    {
                                        LabelText = "Mod Index",
                                        RelativeSizeAxes = Axes.None,
                                        Width = 100,
                                        Current = modIndex,
                                    },
                                    new SettingsNumberBox
                                    {
                                        LabelText = "Row",
                                        RelativeSizeAxes = Axes.None,
                                        Width = 100,
                                        Current = boardY,
                                    },
                                    new SettingsNumberBox
                                    {
                                        LabelText = "Column",
                                        RelativeSizeAxes = Axes.None,
                                        Width = 100,
                                        Current = boardX,
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
                                Action = () => dialogOverlay?.Push(new DeleteBeatmapDialog(Model, () =>
                                {
                                    Expire();
                                    team.Beatmaps.Remove(beatmap);
                                })),
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

                            req.Success += res =>
                            {
                                Model.Beatmap = new TournamentBeatmap(res);
                                updatePanel();
                            };

                            req.Failure += _ =>
                            {
                                Model.Beatmap = null;
                                updatePanel();
                            };

                            API.Queue(req);
                        }, true);

                        mods.Value = Model.Mods;
                        mods.BindValueChanged(modString => Model.Mods = modString.NewValue);
                        modIndex.Value = Model.ModIndex;
                        modIndex.BindValueChanged(newIndex => Model.ModIndex = newIndex.NewValue);
                        boardX.Value = Model.BoardX;
                        boardX.BindValueChanged(newX => { Model.BoardX = newX.NewValue ?? 0; });
                        boardY.Value = Model.BoardY;
                        boardY.BindValueChanged(newY => { Model.BoardY = newY.NewValue ?? 0; });
                    }

                    private void updatePanel() => Schedule(() =>
                    {
                        drawableContainer.Clear();

                        if (Model.Beatmap != null)
                        {
                            drawableContainer.Child = new TournamentBeatmapPanel(Model.Beatmap, Model.Mods, Model.ModIndex)
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Width = 500
                            };
                        }
                    });
                }
            }
        }

        protected override RoundRow CreateDrawable(TournamentRound model) => new RoundRow(model);
    }
}
