// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays;
using osu.Game.Overlays.Music;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Tests.Visual
{
    public abstract class TestCasePerformancePoints : OsuTestCase
    {
        public TestCasePerformancePoints(Ruleset ruleset)
        {
            Child = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Children = new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Width = 0.25f,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4.Black,
                                Alpha = 0.5f,
                            },
                            new ScrollContainer(Direction.Vertical)
                            {
                                RelativeSizeAxes = Axes.Both,
                                Child = new BeatmapList(ruleset)
                            }
                        }
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Width = 0.75f,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4.Black,
                                Alpha = 0.5f,
                            },
                            new ScrollContainer(Direction.Vertical)
                            {
                                RelativeSizeAxes = Axes.Both,
                                Child = new ScoreList { RelativeSizeAxes = Axes.Both }
                            }
                        }
                    },
                }
            };
        }

        private class BeatmapList : CompositeDrawable
        {
            private readonly Container<BeatmapDisplay> beatmapDisplays;
            private readonly Ruleset ruleset;

            public BeatmapList(Ruleset ruleset)
            {
                this.ruleset = ruleset;

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
            private void load(OsuGameBase osuGame, BeatmapManager beatmaps)
            {
                var sets = beatmaps.GetAllUsableBeatmapSets();
                var allBeatmaps = sets.SelectMany(s => s.Beatmaps).Where(b => ruleset.LegacyID < 0 || b.RulesetID == ruleset.LegacyID);

                allBeatmaps.ForEach(b => beatmapDisplays.Add(new BeatmapDisplay(b)));
            }

            private class BeatmapDisplay : CompositeDrawable
            {
                private readonly OsuSpriteText text;
                private readonly BeatmapInfo beatmap;

                private BeatmapManager beatmaps;
                private OsuGameBase osuGame;

                private bool isSelected;

                public BeatmapDisplay(BeatmapInfo beatmap)
                {
                    this.beatmap = beatmap;

                    AutoSizeAxes = Axes.Both;
                    InternalChild = text = new OsuSpriteText();
                }

                protected override bool OnClick(InputState state)
                {
                    if (osuGame.Beatmap.Value.BeatmapInfo.ID == beatmap.ID)
                        return false;

                    osuGame.Beatmap.Value = beatmaps.GetWorkingBeatmap(beatmap);
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

                [BackgroundDependencyLoader]
                private void load(OsuGameBase osuGame, BeatmapManager beatmaps)
                {
                    this.osuGame = osuGame;
                    this.beatmaps = beatmaps;

                    var working = beatmaps.GetWorkingBeatmap(beatmap);
                    text.Text = $"{beatmap.Metadata.Artist} - {beatmap.Metadata.Title} ({beatmap.Metadata.AuthorString}) [{beatmap.Version}]";

                    osuGame.Beatmap.ValueChanged += beatmapChanged;
                }

                private void beatmapChanged(WorkingBeatmap newBeatmap)
                {
                    if (isSelected)
                        this.FadeColour(Color4.White, 100);
                    isSelected = false;
                }
            }
        }

        private class ScoreList : CompositeDrawable
        {
            private readonly FillFlowContainer<ScoreDisplay> scores;
            private APIAccess api;

            public ScoreList()
            {
                InternalChild = scores = new FillFlowContainer<ScoreDisplay>
                {
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 4)
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuGameBase osuGame, APIAccess api)
            {
                this.api = api;
                osuGame.Beatmap.ValueChanged += beatmapChanged;
            }

            private GetScoresRequest lastRequest;
            private void beatmapChanged(WorkingBeatmap newBeatmap)
            {
                lastRequest?.Cancel();
                scores.Clear();

                lastRequest = new GetScoresRequest(newBeatmap.BeatmapInfo);
                lastRequest.Success += res => res.Scores.ForEach(s => scores.Add(new ScoreDisplay(s, newBeatmap.Beatmap)));
                api.Queue(lastRequest);
            }

            private class ScoreDisplay : CompositeDrawable
            {
                private readonly OsuSpriteText playerName;
                private readonly GridContainer attributeGrid;

                private readonly Score score;
                private readonly Beatmap beatmap;

                public ScoreDisplay(Score score, Beatmap beatmap)
                {
                    this.score = score;
                    this.beatmap = beatmap;

                    RelativeSizeAxes = Axes.X;
                    Height = 16;
                    InternalChild = new GridContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Content = new[]
                        {
                            new Drawable[] { playerName = new OsuSpriteText() },
                            new Drawable[] { attributeGrid = new GridContainer { RelativeSizeAxes = Axes.Both } }
                        },
                        RowDimensions = new[]
                        {
                            new Dimension(GridSizeMode.Relative, 0.75f),
                            new Dimension(GridSizeMode.Relative, 0.25f)
                        }
                    };
                }

                [BackgroundDependencyLoader]
                private void load()
                {
                    var ruleset = beatmap.BeatmapInfo.Ruleset.CreateInstance();
                    var calculator = ruleset.CreatePerformanceCalculator(beatmap, score);
                    if (calculator == null)
                        return;

                    var attributes = new Dictionary<string, string>();
                    double performance = calculator.Calculate(attributes);

                    playerName.Text = $"{score.PP} | {performance.ToString("0.00")} | {score.PP / performance}";
                    // var attributeRow =
                }
            }
        }
    }
}
