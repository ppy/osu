// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring.Legacy;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    public abstract partial class ScoringTestScene : OsuTestScene
    {
        protected abstract IBeatmap CreateBeatmap(int maxCombo);

        protected abstract IScoringAlgorithm CreateScoreV1(IReadOnlyList<Mod> selectedMods);
        protected abstract IScoringAlgorithm CreateScoreV2(int maxCombo, IReadOnlyList<Mod> selectedMods);
        protected abstract ProcessorBasedScoringAlgorithm CreateScoreAlgorithm(IBeatmap beatmap, ScoringMode mode, IReadOnlyList<Mod> mods);

        protected Bindable<int> MaxCombo => sliderMaxCombo.Current;
        protected BindableList<double> NonPerfectLocations => graphs.NonPerfectLocations;
        protected BindableList<double> MissLocations => graphs.MissLocations;

        private readonly bool supportsNonPerfectJudgements;

        private GraphContainer graphs = null!;
        private SettingsSlider<int> sliderMaxCombo = null!;
        private SettingsCheckbox scaleToMax = null!;

        private FillFlowContainer<LegendEntry> legend = null!;

        private readonly BindableBool standardisedVisible = new BindableBool(true);
        private readonly BindableBool classicVisible = new BindableBool(true);
        private readonly BindableBool scoreV1Visible = new BindableBool(true);
        private readonly BindableBool scoreV2Visible = new BindableBool(true);

        private RoundedButton changeModsButton = null!;
        private OsuSpriteText modsText = null!;
        private TestModSelectOverlay modSelect = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        protected ScoringTestScene(bool supportsNonPerfectJudgements = true)
        {
            this.supportsNonPerfectJudgements = supportsNonPerfectJudgements;
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("setup tests", () =>
            {
                OsuTextFlowContainer clickExplainer;

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
                            new Dimension(GridSizeMode.AutoSize),
                        },
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                graphs = new GraphContainer(supportsNonPerfectJudgements)
                                {
                                    RelativeSizeAxes = Axes.Both,
                                },
                            },
                            new Drawable[]
                            {
                                legend = new FillFlowContainer<LegendEntry>
                                {
                                    Padding = new MarginPadding(20),
                                    Direction = FillDirection.Vertical,
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                },
                            },
                            new Drawable[]
                            {
                                new Container
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Padding = new MarginPadding { Horizontal = 20 },
                                    Children = new Drawable[]
                                    {
                                        new OsuSpriteText
                                        {
                                            Text = "Selected mods",
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                        },
                                        new FillFlowContainer
                                        {
                                            Anchor = Anchor.TopRight,
                                            Origin = Anchor.TopRight,
                                            AutoSizeAxes = Axes.Both,
                                            Direction = FillDirection.Horizontal,
                                            Spacing = new Vector2(10),
                                            Children = new Drawable[]
                                            {
                                                changeModsButton = new RoundedButton
                                                {
                                                    Text = "Change",
                                                    Width = 100,
                                                    Anchor = Anchor.CentreRight,
                                                    Origin = Anchor.CentreRight,
                                                },
                                                modsText = new OsuSpriteText
                                                {
                                                    Anchor = Anchor.CentreRight,
                                                    Origin = Anchor.CentreRight,
                                                },
                                            }
                                        }
                                    }
                                }
                            },
                            new Drawable[]
                            {
                                new FillFlowContainer
                                {
                                    Padding = new MarginPadding(20),
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical,
                                    Spacing = new Vector2(10),
                                    Children = new Drawable[]
                                    {
                                        sliderMaxCombo = new SettingsSlider<int>
                                        {
                                            TransferValueOnCommit = true,
                                            Current = new BindableInt(1024)
                                            {
                                                MinValue = 96,
                                                MaxValue = 8192,
                                            },
                                            LabelText = "Max combo",
                                        },
                                        scaleToMax = new SettingsCheckbox
                                        {
                                            LabelText = "Rescale plots to 100%",
                                            Current = { Value = true, Default = true }
                                        },
                                        clickExplainer = new OsuTextFlowContainer
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Margin = new MarginPadding { Top = 20 }
                                        }
                                    }
                                },
                            },
                        }
                    },
                    modSelect = new TestModSelectOverlay
                    {
                        RelativeSizeAxes = Axes.Both,
                        SelectedMods = { BindTarget = SelectedMods }
                    }
                };

                clickExplainer.AddParagraph("Left click to add miss");
                if (supportsNonPerfectJudgements)
                    clickExplainer.AddParagraph("Right click to add OK");

                sliderMaxCombo.Current.BindValueChanged(_ => Rerun());
                scaleToMax.Current.BindValueChanged(_ => Rerun());

                standardisedVisible.BindValueChanged(_ => rescalePlots());
                classicVisible.BindValueChanged(_ => rescalePlots());
                scoreV1Visible.BindValueChanged(_ => rescalePlots());
                scoreV2Visible.BindValueChanged(_ => rescalePlots());

                graphs.MissLocations.BindCollectionChanged((_, __) => Rerun());
                graphs.NonPerfectLocations.BindCollectionChanged((_, __) => Rerun());

                graphs.MaxCombo.BindTo(sliderMaxCombo.Current);

                changeModsButton.Action = () => modSelect.Show();
                SelectedMods.BindValueChanged(mods => Rerun());

                Rerun();
            });
        }

        protected void Rerun()
        {
            graphs.Clear();
            legend.Clear();

            modsText.Text = SelectedMods.Value.Any()
                ? string.Join(", ", SelectedMods.Value.Select(mod => mod.Acronym))
                : "(none)";

            runForProcessor("lazer-standardised", colours.Green1, ScoringMode.Standardised, standardisedVisible);
            runForProcessor("lazer-classic", colours.Blue1, ScoringMode.Classic, classicVisible);

            runForAlgorithm(new ScoringAlgorithmInfo
            {
                Name = "ScoreV1 (classic)",
                Colour = colours.Purple1,
                Algorithm = CreateScoreV1(SelectedMods.Value),
                Visible = scoreV1Visible
            });
            runForAlgorithm(new ScoringAlgorithmInfo
            {
                Name = "ScoreV2",
                Colour = colours.Red1,
                Algorithm = CreateScoreV2(sliderMaxCombo.Current.Value, SelectedMods.Value),
                Visible = scoreV2Visible
            });

            rescalePlots();
        }

        private void rescalePlots()
        {
            if (!scaleToMax.Current.Value && legend.Any(entry => entry.Visible.Value))
            {
                long maxScore = legend.Where(entry => entry.Visible.Value).Max(entry => entry.FinalScore);

                foreach (var graph in graphs)
                    graph.Height = graph.Values.Max() / maxScore;
            }
            else
            {
                foreach (var graph in graphs)
                    graph.Height = 1;
            }
        }

        private void runForProcessor(string name, Color4 colour, ScoringMode scoringMode, BindableBool visibility)
        {
            int maxCombo = sliderMaxCombo.Current.Value;
            var beatmap = CreateBeatmap(maxCombo);
            var algorithm = CreateScoreAlgorithm(beatmap, scoringMode, SelectedMods.Value);

            runForAlgorithm(new ScoringAlgorithmInfo
            {
                Name = name,
                Colour = colour,
                Algorithm = algorithm,
                Visible = visibility
            });
        }

        private void runForAlgorithm(ScoringAlgorithmInfo algorithmInfo)
        {
            int maxCombo = sliderMaxCombo.Current.Value;

            List<float> results = new List<float>();

            for (int i = 0; i < maxCombo; i++)
            {
                if (graphs.MissLocations.Contains(i))
                    algorithmInfo.Algorithm.ApplyMiss();
                else if (graphs.NonPerfectLocations.Contains(i))
                    algorithmInfo.Algorithm.ApplyNonPerfect();
                else
                    algorithmInfo.Algorithm.ApplyHit();

                results.Add(algorithmInfo.Algorithm.TotalScore);
            }

            LineGraph graph;
            graphs.Add(graph = new LineGraph
            {
                Name = algorithmInfo.Name,
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
                RelativeSizeAxes = Axes.Both,
                LineColour = algorithmInfo.Colour,
                Values = results
            });

            legend.Add(new LegendEntry(algorithmInfo, graph)
            {
                AccentColour = algorithmInfo.Colour,
            });
        }

        private class ScoringAlgorithmInfo
        {
            public string Name { get; init; } = null!;
            public Color4 Colour { get; init; }
            public IScoringAlgorithm Algorithm { get; init; } = null!;
            public BindableBool Visible { get; init; } = null!;
        }

        protected interface IScoringAlgorithm
        {
            void ApplyHit();
            void ApplyNonPerfect();
            void ApplyMiss();

            long TotalScore { get; }
        }

        protected abstract class ProcessorBasedScoringAlgorithm : IScoringAlgorithm
        {
            protected abstract ScoreProcessor CreateScoreProcessor();
            protected abstract Judgement CreatePerfectJudgementResult();
            protected abstract Judgement CreateNonPerfectJudgementResult();
            protected abstract Judgement CreateMissJudgementResult();

            private readonly ScoreProcessor scoreProcessor;
            private readonly ScoringMode mode;

            protected ProcessorBasedScoringAlgorithm(IBeatmap beatmap, ScoringMode mode, IReadOnlyList<Mod> selectedMods)
            {
                this.mode = mode;
                scoreProcessor = CreateScoreProcessor();
                scoreProcessor.ApplyBeatmap(beatmap);
                scoreProcessor.Mods.Value = selectedMods;
            }

            public void ApplyHit() => scoreProcessor.ApplyResult(CreatePerfectJudgementResult());
            public void ApplyNonPerfect() => scoreProcessor.ApplyResult(CreateNonPerfectJudgementResult());
            public void ApplyMiss() => scoreProcessor.ApplyResult(CreateMissJudgementResult());

            public long TotalScore => scoreProcessor.GetDisplayScore(mode);
        }

        public partial class GraphContainer : Container<LineGraph>, IHasCustomTooltip<IEnumerable<LineGraph>>
        {
            private readonly bool supportsNonPerfectJudgements;

            public readonly BindableList<double> MissLocations = new BindableList<double>();
            public readonly BindableList<double> NonPerfectLocations = new BindableList<double>();

            public Bindable<int> MaxCombo = new Bindable<int>();

            protected override Container<LineGraph> Content { get; } = new Container<LineGraph> { RelativeSizeAxes = Axes.Both };

            private readonly Box hoverLine;

            private readonly Container missLines;
            private readonly Container verticalGridLines;

            public int CurrentHoverCombo { get; private set; }

            public GraphContainer(bool supportsNonPerfectJudgements)
            {
                this.supportsNonPerfectJudgements = supportsNonPerfectJudgements;
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
                else if (supportsNonPerfectJudgements)
                    NonPerfectLocations.Add(CurrentHoverCombo);

                return true;
            }

            private GraphTooltip? tooltip;

            public ITooltip<IEnumerable<LineGraph>> GetCustomTooltip() => tooltip ??= new GraphTooltip(this);

            public IEnumerable<LineGraph> TooltipContent => Content;

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
                        if (graph.Alpha == 0) continue;

                        float valueAtHover = graph.Values.ElementAt(relevantCombo);
                        float ofTotal = valueAtHover / graph.Values.Last();

                        textFlow.AddParagraph($"{graph.Name}: {valueAtHover:#,0} ({ofTotal * 100:N0}% of final)\n", st => st.Colour = graph.LineColour);
                    }
                }

                public void Move(Vector2 pos) => this.MoveTo(pos);
            }
        }

        private partial class LegendEntry : OsuClickableContainer, IHasAccentColour
        {
            public Color4 AccentColour { get; set; }

            public BindableBool Visible { get; } = new BindableBool(true);

            public readonly long FinalScore;

            private readonly string description;
            private readonly LineGraph lineGraph;

            private OsuSpriteText descriptionText = null!;
            private OsuSpriteText finalScoreText = null!;

            public LegendEntry(ScoringAlgorithmInfo scoringAlgorithmInfo, LineGraph lineGraph)
            {
                description = scoringAlgorithmInfo.Name;
                FinalScore = scoringAlgorithmInfo.Algorithm.TotalScore;
                AccentColour = scoringAlgorithmInfo.Colour;
                Visible.BindTo(scoringAlgorithmInfo.Visible);

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
                finalScoreText.Text = FinalScore.ToString("#,0");
                lineGraph.Alpha = Visible.Value ? 1 : 0;
            }
        }

        private partial class TestModSelectOverlay : UserModSelectOverlay
        {
            protected override bool ShowModEffects => true;
            protected override bool ShowPresets => false;

            public TestModSelectOverlay()
                : base(OverlayColourScheme.Aquamarine)
            {
            }
        }
    }
}
