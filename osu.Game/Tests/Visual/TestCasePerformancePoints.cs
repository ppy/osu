// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Caching;
using osu.Framework.Configuration;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Tests.Visual
{
    public abstract class TestCasePerformancePoints : OsuTestCase
    {
        protected TestCasePerformancePoints(Ruleset ruleset)
        {
            Child = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                Content = new[]
                {
                    new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4.Black,
                                    Alpha = 0.5f,
                                },
                                new ScrollContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Child = new BeatmapList(ruleset, Beatmap)
                                }
                            }
                        },
                        null,
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4.Black,
                                    Alpha = 0.5f,
                                },
                                new ScrollContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Child = new StarRatingGrid()
                                }
                            }
                        },
                        null,
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4.Black,
                                    Alpha = 0.5f,
                                },
                                new ScrollContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Child = new PerformanceList()
                                }
                            }
                        },
                    }
                },
                ColumnDimensions = new[]
                {
                    new Dimension(),
                    new Dimension(GridSizeMode.Absolute, 20),
                    new Dimension(),
                    new Dimension(GridSizeMode.Absolute, 20)
                }
            };
        }

        private class BeatmapList : CompositeDrawable
        {
            private readonly Container<BeatmapDisplay> beatmapDisplays;
            private readonly Ruleset ruleset;
            private readonly BindableBeatmap beatmapBindable;

            public BeatmapList(Ruleset ruleset, BindableBeatmap beatmapBindable)
            {
                this.ruleset = ruleset;
                this.beatmapBindable = beatmapBindable;

                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                InternalChild = beatmapDisplays = new FillFlowContainer<BeatmapDisplay>
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 4)
                };
            }

            [BackgroundDependencyLoader]
            private void load(BeatmapManager beatmaps)
            {
                var sets = beatmaps.GetAllUsableBeatmapSets();
                var allBeatmaps = sets.SelectMany(s => s.Beatmaps).Where(b => ruleset.LegacyID == null || b.RulesetID == ruleset.LegacyID);

                allBeatmaps.ForEach(b => beatmapDisplays.Add(new BeatmapDisplay(b, beatmapBindable)));
            }

            private class BeatmapDisplay : CompositeDrawable, IHasTooltip
            {
                private readonly OsuSpriteText text;
                private readonly BeatmapInfo beatmap;

                private readonly BindableBeatmap beatmapBindable;

                private BeatmapManager beatmaps;

                private bool isSelected;

                public string TooltipText => text.Text;

                public BeatmapDisplay(BeatmapInfo beatmap, BindableBeatmap beatmapBindable)
                {
                    this.beatmap = beatmap;
                    this.beatmapBindable = beatmapBindable;

                    AutoSizeAxes = Axes.Both;
                    InternalChild = text = new OsuSpriteText();

                    this.beatmapBindable.ValueChanged += beatmapChanged;
                }

                [BackgroundDependencyLoader]
                private void load(BeatmapManager beatmaps)
                {
                    this.beatmaps = beatmaps;

                    var working = beatmaps.GetWorkingBeatmap(beatmap);
                    text.Text = $"{working.Metadata.Artist} - {working.Metadata.Title} ({working.Metadata.AuthorString}) [{working.BeatmapInfo.Version}]";
                }

                private void beatmapChanged(WorkingBeatmap newBeatmap)
                {
                    if (isSelected)
                        this.FadeColour(Color4.White, 100);
                    isSelected = false;
                }

                protected override bool OnClick(InputState state)
                {
                    if (beatmapBindable.Value.BeatmapInfo.ID == beatmap.ID)
                        return false;

                    beatmapBindable.Value = beatmaps.GetWorkingBeatmap(beatmap);
                    isSelected = true;
                    return true;
                }

                protected override bool OnHover(InputState state)
                {
                    if (isSelected)
                        return false;
                    this.FadeColour(Color4.Yellow, 100);
                    return true;
                }

                protected override void OnHoverLost(InputState state)
                {
                    if (isSelected)
                        return;
                    this.FadeColour(Color4.White, 100);
                }
            }
        }

        private class PerformanceList : CompositeDrawable
        {
            private readonly FillFlowContainer<PerformanceDisplay> scores;
            private APIAccess api;

            private readonly IBindable<WorkingBeatmap> currentBeatmap = new Bindable<WorkingBeatmap>();

            public PerformanceList()
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                InternalChild = scores = new FillFlowContainer<PerformanceDisplay>
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 4)
                };
            }

            [BackgroundDependencyLoader]
            private void load(IBindableBeatmap beatmap, APIAccess api)
            {
                this.api = api;

                if (!api.IsLoggedIn)
                {
                    InternalChild = new OsuSpriteText
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Text = "Please sign in to see online scores",
                    };
                }

                currentBeatmap.ValueChanged += beatmapChanged;
                currentBeatmap.BindTo(beatmap);
            }

            private GetScoresRequest lastRequest;
            private void beatmapChanged(WorkingBeatmap newBeatmap)
            {
                if (!IsAlive) return;

                lastRequest?.Cancel();
                scores.Clear();

                if (!api.IsLoggedIn)
                    return;

                lastRequest = new GetScoresRequest(newBeatmap.BeatmapInfo, newBeatmap.BeatmapInfo.Ruleset);
                lastRequest.Success += res => res.Scores.ForEach(s => scores.Add(new PerformanceDisplay(s, newBeatmap.Beatmap)));
                api.Queue(lastRequest);
            }

            private class PerformanceDisplay : CompositeDrawable
            {
                private readonly OsuSpriteText text;

                private readonly Score score;
                private readonly IBeatmap beatmap;

                public PerformanceDisplay(Score score, IBeatmap beatmap)
                {
                    this.score = score;
                    this.beatmap = beatmap;

                    RelativeSizeAxes = Axes.X;
                    AutoSizeAxes = Axes.Y;
                    InternalChild = text = new OsuSpriteText();
                }

                [BackgroundDependencyLoader]
                private void load()
                {
                    var ruleset = beatmap.BeatmapInfo.Ruleset.CreateInstance();
                    var calculator = ruleset.CreatePerformanceCalculator(beatmap, score);
                    if (calculator == null)
                        return;

                    var attributes = new Dictionary<string, double>();
                    double performance = calculator.Calculate(attributes);

                    text.Text = $"{score.User.Username} -> online: {score.PP:n2}pp | local: {performance:n2}pp";
                }
            }
        }

        private class StarRatingGrid : CompositeDrawable
        {
            private readonly FillFlowContainer<OsuCheckbox> modFlow;
            private readonly OsuSpriteText totalText;
            private readonly FillFlowContainer categoryTexts;

            public StarRatingGrid()
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                InternalChild = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        modFlow = new FillFlowContainer<OsuCheckbox>
                        {
                            Name = "Checkbox flow",
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Spacing = new Vector2(4, 4)
                        },
                        new FillFlowContainer
                        {
                            Name = "Information display",
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Spacing = new Vector2(0, 4),
                            Direction = FillDirection.Vertical,
                            Children = new Drawable[]
                            {
                                totalText = new OsuSpriteText { TextSize = 24 },
                                categoryTexts = new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical
                                }
                            }
                        }
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(IBindableBeatmap beatmap)
            {
                beatmap.ValueChanged += beatmapChanged;
            }

            private Cached informationCache = new Cached();

            private Ruleset ruleset;
            private WorkingBeatmap beatmap;

            private void beatmapChanged(WorkingBeatmap newBeatmap)
            {
                beatmap = newBeatmap;

                modFlow.Clear();

                ruleset = newBeatmap.BeatmapInfo.Ruleset.CreateInstance();
                foreach (var mod in ruleset.GetAllMods())
                {
                    var checkBox = new OsuCheckbox
                    {
                        RelativeSizeAxes = Axes.None,
                        Width = 50,
                        LabelText = mod.ShortenedName
                    };

                    checkBox.Current.ValueChanged += v => informationCache.Invalidate();
                    modFlow.Add(checkBox);
                }

                informationCache.Invalidate();
            }

            protected override void Update()
            {
                base.Update();

                if (ruleset == null)
                    return;

                if (!informationCache.IsValid)
                {
                    totalText.Text = string.Empty;
                    categoryTexts.Clear();

                    var allMods = ruleset.GetAllMods().ToList();
                    Mod[] activeMods = modFlow.Where(c => c.Current.Value).Select(c => allMods.First(m => m.ShortenedName == c.LabelText)).ToArray();

                    var diffCalc = ruleset.CreateDifficultyCalculator(beatmap.Beatmap, activeMods);
                    if (diffCalc != null)
                    {
                        var categories = new Dictionary<string, double>();
                        double totalSr = diffCalc.Calculate(categories);

                        totalText.Text = $"Star rating: {totalSr:n2}";
                        foreach (var kvp in categories)
                            categoryTexts.Add(new OsuSpriteText { Text = $"{kvp.Key}: {kvp.Value:n2}" });
                    }

                    informationCache.Validate();
                }
            }
        }
    }
}
