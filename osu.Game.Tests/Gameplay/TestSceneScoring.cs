// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Tests.Visual;
using osuTK.Graphics;

namespace osu.Game.Tests.Gameplay
{
    public class TestSceneScoring : OsuTestScene
    {
        private GraphContainer graphs = null!;
        private SettingsSlider<int> sliderMaxCombo = null!;

        private FillFlowContainer legend = null!;

        private static readonly Color4[] line_colours =
        {
            Color4Extensions.FromHex("588c7e"),
            Color4Extensions.FromHex("b2a367"),
            Color4Extensions.FromHex("c98f65"),
            Color4Extensions.FromHex("bc5151"),
            Color4Extensions.FromHex("5c8bd6"),
            Color4Extensions.FromHex("7f6ab7"),
            Color4Extensions.FromHex("a368ad"),
            Color4Extensions.FromHex("aa6880"),
        };

        [Test]
        public void TestBasic()
        {
            AddStep("setup tests", () =>
            {
                Children = new Drawable[]
                {
                    new GridContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                graphs = new GraphContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Height = 200,
                                },
                            },
                            new Drawable[]
                            {
                                legend = new FillFlowContainer
                                {
                                    Direction = FillDirection.Vertical,
                                    RelativeSizeAxes = Axes.X,
                                    Height = 200,
                                },
                            },
                            new Drawable[]
                            {
                                new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
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
                                        }
                                    }
                                },
                            },
                        }
                    }
                };

                sliderMaxCombo.Current.BindValueChanged(_ => rerun());
                graphs.MissLocations.BindCollectionChanged((_, __) => rerun());

                graphs.MaxCombo.BindTo(sliderMaxCombo.Current);

                rerun();
            });
        }

        private void rerun()
        {
            graphs.Clear();
            legend.Clear();

            runForProcessor("lazer-classic", new ScoreProcessor(new OsuRuleset()) { Mode = { Value = ScoringMode.Classic } });
            runForProcessor("lazer-standardised", new ScoreProcessor(new OsuRuleset()) { Mode = { Value = ScoringMode.Standardised } });
        }

        private void runForProcessor(string name, ScoreProcessor processor)
        {
            Color4 colour = line_colours[Math.Abs(name.GetHashCode()) % line_colours.Length];

            int maxCombo = sliderMaxCombo.Current.Value;

            var beatmap = new OsuBeatmap();

            for (int i = 0; i < maxCombo; i++)
            {
                beatmap.HitObjects.Add(new HitCircle());
            }

            processor.ApplyBeatmap(beatmap);

            List<float> results = new List<float>();

            for (int i = 0; i < maxCombo; i++)
            {
                if (graphs.MissLocations.Contains(i))
                {
                    processor.ApplyResult(new OsuJudgementResult(new HitCircle(), new OsuJudgement())
                    {
                        Type = HitResult.Miss
                    });
                }
                else
                {
                    processor.ApplyResult(new OsuJudgementResult(new HitCircle(), new OsuJudgement())
                    {
                        Type = HitResult.Great
                    });
                }

                results.Add((float)processor.TotalScore.Value);
            }

            graphs.Add(new LineGraph
            {
                RelativeSizeAxes = Axes.Both,
                LineColour = colour,
                Values = results
            });

            legend.Add(new OsuSpriteText
            {
                Colour = colour,
                Text = $"{FontAwesome.Solid.Circle.Icon} {name}"
            });
        }
    }

    public class GraphContainer : Container
    {
        public readonly BindableList<double> MissLocations = new BindableList<double>();

        public Bindable<int> MaxCombo = new Bindable<int>();

        protected override Container<Drawable> Content { get; } = new Container { RelativeSizeAxes = Axes.Both };

        private readonly Box hoverLine;

        private readonly Container missLines;
        private readonly Container verticalGridLines;

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
                    Content,
                    hoverLine = new Box
                    {
                        Colour = Color4.Yellow,
                        RelativeSizeAxes = Axes.Y,
                        Alpha = 0,
                        Width = 1,
                    },
                    missLines = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                }
            };

            MissLocations.BindCollectionChanged((_, _) => updateMissLocations());

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
                    verticalGridLines.Add(new Box
                    {
                        Colour = OsuColour.Gray(0.2f),
                        Width = 1,
                        RelativeSizeAxes = Axes.Y,
                        RelativePositionAxes = Axes.X,
                        X = (float)i / MaxCombo.Value,
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
            hoverLine.X = e.MousePosition.X;
            return base.OnMouseMove(e);
        }

        protected override bool OnClick(ClickEvent e)
        {
            MissLocations.Add((int)(e.MousePosition.X / DrawWidth * MaxCombo.Value));
            return true;
        }
    }
}
