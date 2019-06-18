// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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
    public class RoundEditorScreen : TournamentEditorScreen<RoundEditorScreen.RoundRow>
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            foreach (var r in LadderInfo.Rounds)
                Flow.Add(new RoundRow(r));
        }

        protected override void AddNew()
        {
            var round = new TournamentRound
            {
                StartDate = { Value = DateTimeOffset.UtcNow }
            };

            Flow.Add(new RoundRow(round));
            LadderInfo.Rounds.Add(round);
        }

        public class RoundRow : CompositeDrawable
        {
            public readonly TournamentRound Round;

            [Resolved]
            private LadderInfo ladderInfo { get; set; }

            public RoundRow(TournamentRound round)
            {
                Round = round;

                Masking = true;
                CornerRadius = 10;

                RoundBeatmapEditor beatmapEditor = new RoundBeatmapEditor(round)
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
                                LabelText = "Name",
                                Width = 0.33f,
                                Bindable = Round.Name
                            },
                            new SettingsTextBox
                            {
                                LabelText = "Description",
                                Width = 0.33f,
                                Bindable = Round.Description
                            },
                            new DateTextBox
                            {
                                LabelText = "Start Time",
                                Width = 0.33f,
                                Bindable = Round.StartDate
                            },
                            new SettingsSlider<int>
                            {
                                LabelText = "Best of",
                                Width = 0.33f,
                                Bindable = Round.BestOf
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
                        Text = "Delete Round",
                        Action = () =>
                        {
                            Expire();
                            ladderInfo.Rounds.Remove(Round);
                        },
                    }
                };

                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
            }

            public class RoundBeatmapEditor : CompositeDrawable
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
                        LayoutDuration = 200,
                        LayoutEasing = Easing.OutQuint,
                        ChildrenEnumerable = round.Beatmaps.Select(p => new RoundBeatmapRow(round, p))
                    };
                }

                public void CreateNew()
                {
                    var user = new RoundBeatmap();
                    round.Beatmaps.Add(user);
                    flow.Add(new RoundBeatmapRow(round, user));
                }

                public class RoundBeatmapRow : CompositeDrawable
                {
                    private readonly RoundBeatmap beatmap;

                    [Resolved]
                    protected IAPIProvider API { get; private set; }

                    private readonly Bindable<string> beatmapId = new Bindable<string>();

                    private readonly Bindable<string> mods = new Bindable<string>();

                    private readonly Container drawableContainer;

                    public RoundBeatmapRow(TournamentRound team, RoundBeatmap beatmap)
                    {
                        this.beatmap = beatmap;

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
                                    new SettingsTextBox
                                    {
                                        LabelText = "Beatmap ID",
                                        RelativeSizeAxes = Axes.None,
                                        Width = 200,
                                        Bindable = beatmapId,
                                    },
                                    new SettingsTextBox
                                    {
                                        LabelText = "Mods",
                                        RelativeSizeAxes = Axes.None,
                                        Width = 200,
                                        Bindable = mods,
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
                                    team.Beatmaps.Remove(beatmap);
                                },
                            }
                        };
                    }

                    [BackgroundDependencyLoader]
                    private void load(RulesetStore rulesets)
                    {
                        beatmapId.Value = beatmap.ID.ToString();
                        beatmapId.BindValueChanged(idString =>
                        {
                            int parsed;

                            int.TryParse(idString.NewValue, out parsed);

                            beatmap.ID = parsed;

                            if (idString.NewValue != idString.OldValue)
                                beatmap.BeatmapInfo = null;

                            if (beatmap.BeatmapInfo != null)
                            {
                                updatePanel();
                                return;
                            }

                            var req = new GetBeatmapRequest(new BeatmapInfo { OnlineBeatmapID = beatmap.ID });

                            req.Success += res =>
                            {
                                beatmap.BeatmapInfo = res.ToBeatmap(rulesets);
                                updatePanel();
                            };

                            req.Failure += _ =>
                            {
                                beatmap.BeatmapInfo = null;
                                updatePanel();
                            };

                            API.Queue(req);
                        }, true);

                        mods.Value = beatmap.Mods;
                        mods.BindValueChanged(modString => beatmap.Mods = modString.NewValue);
                    }

                    private void updatePanel()
                    {
                        drawableContainer.Clear();

                        if (beatmap.BeatmapInfo != null)
                            drawableContainer.Child = new TournamentBeatmapPanel(beatmap.BeatmapInfo, beatmap.Mods)
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
}
