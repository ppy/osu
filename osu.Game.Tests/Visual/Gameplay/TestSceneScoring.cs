// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring.Legacy;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneScoring : OsuTestScene
    {
        private GraphContainer graphs = null!;
        private SettingsSlider<int> sliderMaxCombo = null!;

        private FillFlowContainer legend = null!;

        private readonly BindableBool standardisedVisible = new BindableBool(true);
        private readonly BindableBool classicVisible = new BindableBool(true);
        private readonly BindableBool scoreV1Visible = new BindableBool(true);
        private readonly BindableBool scoreV2Visible = new BindableBool(true);

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Test]
        public void TestBasic()
        {
            AddStep("setup tests", () =>
            {
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Colour4.Black
                    },
                    new GridContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        RowDimensions = new[]
                        {
                            new Dimension(),
                            new Dimension(GridSizeMode.AutoSize),
                            new Dimension(GridSizeMode.AutoSize),
                        },
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                graphs = new GraphContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                },
                            },
                            new Drawable[]
                            {
                                legend = new FillFlowContainer
                                {
                                    Padding = new MarginPadding(20),
                                    Direction = FillDirection.Vertical,
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                },
                            },
                            new Drawable[]
                            {
                                new FillFlowContainer
                                {
                                    Padding = new MarginPadding(20),
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Full,
                                    Children = new Drawable[]
                                    {
                                        sliderMaxCombo = new SettingsSlider<int>
                                        {
                                            Width = 0.5f,
                                            TransferValueOnCommit = true,
                                            Current = new BindableInt(1024)
                                            {
                                                MinValue = 96,
                                                MaxValue = 8192,
                                            },
                                            LabelText = "max combo",
                                        },
                                        new OsuTextFlowContainer
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            Width = 0.5f,
                                            AutoSizeAxes = Axes.Y,
                                            Text = $"Left click to add miss\nRight click to add OK/{base_ok}"
                                        }
                                    }
                                },
                            },
                        }
                    }
                };

                sliderMaxCombo.Current.BindValueChanged(_ => rerun());

                graphs.MissLocations.BindCollectionChanged((_, __) => rerun());
                graphs.NonPerfectLocations.BindCollectionChanged((_, __) => rerun());

                graphs.MaxCombo.BindTo(sliderMaxCombo.Current);

                rerun();
            });
        }

        private const int base_great = 300;
        private const int base_ok = 100;

        private void rerun()
        {
            graphs.Clear();
            legend.Clear();

            runForProcessor("lazer-standardised", colours.Green1, new OsuScoreProcessor(), ScoringMode.Standardised, standardisedVisible);
            runForProcessor("lazer-classic", colours.Blue1, new OsuScoreProcessor(), ScoringMode.Classic, classicVisible);

            runScoreV1();
            runScoreV2();
        }

        private void runScoreV1()
        {
            int totalScore = 0;
            int currentCombo = 0;

            void applyHitV1(int baseScore)
            {
                if (baseScore == 0)
                {
                    currentCombo = 0;
                    return;
                }

                const float score_multiplier = 1;

                totalScore += baseScore;

                // combo multiplier
                // ReSharper disable once PossibleLossOfFraction
                totalScore += (int)(Math.Max(0, currentCombo - 1) * (baseScore / 25 * score_multiplier));

                currentCombo++;
            }

            runForAlgorithm(new ScoringAlgorithm
            {
                Name = "ScoreV1 (classic)",
                Colour = colours.Purple1,
                ApplyHit = () => applyHitV1(base_great),
                ApplyNonPerfect = () => applyHitV1(base_ok),
                ApplyMiss = () => applyHitV1(0),
                GetTotalScore = () =>
                {
                    // Arbitrary value chosen towards the upper range.
                    const double score_multiplier = 4;

                    return (int)(totalScore * score_multiplier);
                },
                Visible = scoreV1Visible
            });
        }

        private void runScoreV2()
        {
            int maxCombo = sliderMaxCombo.Current.Value;

            int currentCombo = 0;
            double comboPortion = 0;
            double currentBaseScore = 0;
            double maxBaseScore = 0;
            int currentHits = 0;

            for (int i = 0; i < maxCombo; i++)
                applyHitV2(base_great);

            double comboPortionMax = comboPortion;

            currentCombo = 0;
            comboPortion = 0;
            currentBaseScore = 0;
            maxBaseScore = 0;
            currentHits = 0;

            void applyHitV2(int baseScore)
            {
                maxBaseScore += base_great;
                currentBaseScore += baseScore;
                comboPortion += baseScore * (1 + ++currentCombo / 10.0);

                currentHits++;
            }

            runForAlgorithm(new ScoringAlgorithm
            {
                Name = "ScoreV2",
                Colour = colours.Red1,
                ApplyHit = () => applyHitV2(base_great),
                ApplyNonPerfect = () => applyHitV2(base_ok),
                ApplyMiss = () =>
                {
                    currentHits++;
                    maxBaseScore += base_great;
                    currentCombo = 0;
                },
                GetTotalScore = () =>
                {
                    double accuracy = currentBaseScore / maxBaseScore;

                    return (int)Math.Round
                    (
                        700000 * comboPortion / comboPortionMax +
                        300000 * Math.Pow(accuracy, 10) * ((double)currentHits / maxCombo)
                    );
                },
                Visible = scoreV2Visible
            });
        }

        private void runForProcessor(string name, Color4 colour, ScoreProcessor processor, ScoringMode mode, BindableBool visibility)
        {
            int maxCombo = sliderMaxCombo.Current.Value;

            var beatmap = new OsuBeatmap();
            for (int i = 0; i < maxCombo; i++)
                beatmap.HitObjects.Add(new HitCircle());

            processor.ApplyBeatmap(beatmap);

            runForAlgorithm(new ScoringAlgorithm
            {
                Name = name,
                Colour = colour,
                ApplyHit = () => processor.ApplyResult(new OsuJudgementResult(new HitCircle(), new OsuJudgement()) { Type = HitResult.Great }),
                ApplyNonPerfect = () => processor.ApplyResult(new OsuJudgementResult(new HitCircle(), new OsuJudgement()) { Type = HitResult.Ok }),
                ApplyMiss = () => processor.ApplyResult(new OsuJudgementResult(new HitCircle(), new OsuJudgement()) { Type = HitResult.Miss }),
                GetTotalScore = () => processor.GetDisplayScore(mode),
                Visible = visibility
            });
        }

        private void runForAlgorithm(ScoringAlgorithm scoringAlgorithm)
        {
            int maxCombo = sliderMaxCombo.Current.Value;

            List<float> results = new List<float>();

            for (int i = 0; i < maxCombo; i++)
            {
                if (graphs.MissLocations.Contains(i))
                    scoringAlgorithm.ApplyMiss();
                else if (graphs.NonPerfectLocations.Contains(i))
                    scoringAlgorithm.ApplyNonPerfect();
                else
                    scoringAlgorithm.ApplyHit();

                results.Add(scoringAlgorithm.GetTotalScore());
            }

            LineGraph graph;
            graphs.Add(graph = new LineGraph
            {
                Name = scoringAlgorithm.Name,
                RelativeSizeAxes = Axes.Both,
                LineColour = scoringAlgorithm.Colour,
                Values = results
            });

            legend.Add(new LegendEntry(scoringAlgorithm, graph)
            {
                AccentColour = scoringAlgorithm.Colour,
            });
        }
    }

    public class ScoringAlgorithm
    {
        public string Name { get; init; } = null!;
        public Color4 Colour { get; init; }
        public Action ApplyHit { get; init; } = () => { };
        public Action ApplyNonPerfect { get; init; } = () => { };
        public Action ApplyMiss { get; init; } = () => { };
        public Func<long> GetTotalScore { get; init; } = null!;
        public BindableBool Visible { get; init; } = null!;
    }

    public partial class GraphContainer : Container, IHasCustomTooltip<IEnumerable<LineGraph>>
    {
        public readonly BindableList<double> MissLocations = new BindableList<double>();
        public readonly BindableList<double> NonPerfectLocations = new BindableList<double>();

        public Bindable<int> MaxCombo = new Bindable<int>();

        protected override Container<Drawable> Content { get; } = new Container { RelativeSizeAxes = Axes.Both };

        private readonly Box hoverLine;

        private readonly Container missLines;
        private readonly Container verticalGridLines;

        public int CurrentHoverCombo { get; private set; }

        public GraphContainer()
        {
            InternalChild = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new Box
                    {
                        Colour = OsuColour.Gray(0.1f),
                        RelativeSizeAxes = Axes.Both,
                    },
                    verticalGridLines = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    hoverLine = new Box
                    {
                        Colour = Color4.Yellow,
                        RelativeSizeAxes = Axes.Y,
                        Origin = Anchor.TopCentre,
                        Alpha = 0,
                        Width = 1,
                    },
                    missLines = new Container
                    {
                        Alpha = 0.6f,
                        RelativeSizeAxes = Axes.Both,
                    },
                    Content,
                }
            };

            MissLocations.BindCollectionChanged((_, _) => updateMissLocations());
            NonPerfectLocations.BindCollectionChanged((_, _) => updateMissLocations());

            MaxCombo.BindValueChanged(_ =>
            {
                updateMissLocations();
                updateVerticalGridLines();
            }, true);
        }

        private void updateVerticalGridLines()
        {
            verticalGridLines.Clear();

            for (int i = 0; i < MaxCombo.Value; i++)
            {
                if (i % 100 == 0)
                {
                    verticalGridLines.AddRange(new Drawable[]
                    {
                        new Box
                        {
                            Colour = OsuColour.Gray(0.2f),
                            Origin = Anchor.TopCentre,
                            Width = 1,
                            RelativeSizeAxes = Axes.Y,
                            RelativePositionAxes = Axes.X,
                            X = (float)i / MaxCombo.Value,
                        },
                        new OsuSpriteText
                        {
                            RelativePositionAxes = Axes.X,
                            X = (float)i / MaxCombo.Value,
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Text = $"{i:#,0}",
                            Rotation = -30,
                            Y = -20,
                        }
                    });
                }
            }
        }

        private void updateMissLocations()
        {
            missLines.Clear();

            foreach (int miss in MissLocations)
            {
                missLines.Add(new Box
                {
                    Colour = Color4.Red,
                    Origin = Anchor.TopCentre,
                    Width = 1,
                    RelativeSizeAxes = Axes.Y,
                    RelativePositionAxes = Axes.X,
                    X = (float)miss / MaxCombo.Value,
                });
            }

            foreach (int miss in NonPerfectLocations)
            {
                missLines.Add(new Box
                {
                    Colour = Color4.Orange,
                    Origin = Anchor.TopCentre,
                    Width = 1,
                    RelativeSizeAxes = Axes.Y,
                    RelativePositionAxes = Axes.X,
                    X = (float)miss / MaxCombo.Value,
                });
            }
        }

        protected override bool OnHover(HoverEvent e)
        {
            hoverLine.Show();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            hoverLine.Hide();
            base.OnHoverLost(e);
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            CurrentHoverCombo = (int)(e.MousePosition.X / DrawWidth * MaxCombo.Value);

            hoverLine.X = e.MousePosition.X;
            return base.OnMouseMove(e);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (e.Button == MouseButton.Left)
                MissLocations.Add(CurrentHoverCombo);
            else
                NonPerfectLocations.Add(CurrentHoverCombo);

            return true;
        }

        private GraphTooltip? tooltip;

        public ITooltip<IEnumerable<LineGraph>> GetCustomTooltip() => tooltip ??= new GraphTooltip(this);

        public IEnumerable<LineGraph> TooltipContent => Content.OfType<LineGraph>();

        public partial class GraphTooltip : CompositeDrawable, ITooltip<IEnumerable<LineGraph>>
        {
            private readonly GraphContainer graphContainer;

            private readonly OsuTextFlowContainer textFlow;

            public GraphTooltip(GraphContainer graphContainer)
            {
                this.graphContainer = graphContainer;
                AutoSizeAxes = Axes.Both;

                Masking = true;
                CornerRadius = 10;

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        Colour = OsuColour.Gray(0.15f),
                        RelativeSizeAxes = Axes.Both,
                    },
                    textFlow = new OsuTextFlowContainer
                    {
                        Colour = Color4.White,
                        AutoSizeAxes = Axes.Both,
                        Padding = new MarginPadding(10),
                    }
                };
            }

            private int? lastContentCombo;

            public void SetContent(IEnumerable<LineGraph> content)
            {
                int relevantCombo = graphContainer.CurrentHoverCombo;

                if (lastContentCombo == relevantCombo)
                    return;

                lastContentCombo = relevantCombo;
                textFlow.Clear();

                textFlow.AddParagraph($"At combo {relevantCombo}:");

                foreach (var graph in content)
                {
                    float valueAtHover = graph.Values.ElementAt(relevantCombo);
                    float ofTotal = valueAtHover / graph.Values.Last();

                    textFlow.AddParagraph($"{graph.Name}: {valueAtHover:#,0} ({ofTotal * 100:N0}% of final)\n", st => st.Colour = graph.LineColour);
                }
            }

            public void Move(Vector2 pos) => this.MoveTo(pos);
        }
    }

    public partial class LegendEntry : OsuClickableContainer, IHasAccentColour
    {
        public Color4 AccentColour { get; set; }

        public BindableBool Visible { get; } = new BindableBool(true);

        private readonly string description;
        private readonly long finalScore;
        private readonly LineGraph lineGraph;

        private OsuSpriteText descriptionText = null!;
        private OsuSpriteText finalScoreText = null!;

        public LegendEntry(ScoringAlgorithm scoringAlgorithm, LineGraph lineGraph)
        {
            description = scoringAlgorithm.Name;
            finalScore = scoringAlgorithm.GetTotalScore();
            AccentColour = scoringAlgorithm.Colour;
            Visible.BindTo(scoringAlgorithm.Visible);

            this.lineGraph = lineGraph;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Content.RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Content.AutoSizeAxes = Axes.Y;

            Children = new Drawable[]
            {
                descriptionText = new OsuSpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                },
                finalScoreText = new OsuSpriteText
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Font = OsuFont.Default.With(fixedWidth: true)
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Visible.BindValueChanged(_ => updateState(), true);
            Action = Visible.Toggle;
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateState();
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateState();
            base.OnHoverLost(e);
        }

        private void updateState()
        {
            Colour = IsHovered ? AccentColour.Lighten(0.2f) : AccentColour;

            descriptionText.Text = $"{(Visible.Value ? FontAwesome.Solid.CheckCircle.Icon : FontAwesome.Solid.Circle.Icon)} {description}";
            finalScoreText.Text = finalScore.ToString("#,0");
            lineGraph.Alpha = Visible.Value ? 1 : 0;
        }
    }
}
