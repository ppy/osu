// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Threading;
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
        private Container graphs = null!;
        private SettingsSlider<int> sliderMaxCombo = null!;

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
                                graphs = new Container
                                {
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

                rerun();
            });
        }

        private ScheduledDelegate? debouncedRun;

        private void rerun()
        {
            graphs.Clear();

            debouncedRun?.Cancel();
            debouncedRun = Scheduler.AddDelayed(() =>
            {
                runForProcessor("lazer-classic", new ScoreProcessor(new OsuRuleset()) { Mode = { Value = ScoringMode.Classic } });
                runForProcessor("lazer-standardised", new ScoreProcessor(new OsuRuleset()) { Mode = { Value = ScoringMode.Standardised } });
            }, 200);
        }

        private void runForProcessor(string name, ScoreProcessor processor)
        {
            int maxCombo = sliderMaxCombo.Current.Value;

            var beatmap = new OsuBeatmap();

            for (int i = 0; i < maxCombo; i++)
            {
                beatmap.HitObjects.Add(new HitCircle());
            }

            processor.ApplyBeatmap(beatmap);

            int[] missLocations = { 200, 500, 800 };

            List<float> results = new List<float>();

            for (int i = 0; i < maxCombo; i++)
            {
                if (missLocations.Contains(i))
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
                LineColour = Color4.Red,
                Values = results
            });
        }
    }
}
