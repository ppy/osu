// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    public partial class TestSceneDrawableJudgementSliderTicks : OsuSkinnableTestScene
    {
        private bool classic;
        private readonly Container<DrawableOsuJudgement>[,,] judgementContainers;
        private readonly JudgementPooler<DrawableOsuJudgement>[] judgementPools;

        public TestSceneDrawableJudgementSliderTicks()
        {
            judgementContainers = new Container<DrawableOsuJudgement>[Rows * Cols, 5, 2];
            judgementPools = new JudgementPooler<DrawableOsuJudgement>[Rows * Cols];
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            int cellIndex = 0;

            SetContents(_ =>
            {
                var container = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        judgementPools[cellIndex] = new JudgementPooler<DrawableOsuJudgement>(new[]
                        {
                            HitResult.Great,
                            HitResult.Miss,
                            HitResult.LargeTickHit,
                            HitResult.SliderTailHit,
                            HitResult.LargeTickMiss,
                            HitResult.IgnoreMiss,
                        }),
                        new GridContainer
                        {
                            Padding = new MarginPadding { Top = 26f },
                            RelativeSizeAxes = Axes.Both,
                            RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                            ColumnDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                            Content =
                                new[]
                                {
                                    new[]
                                    {
                                        Empty(),
                                        new OsuSpriteText
                                        {
                                            Text = "hit",
                                            Anchor = Anchor.BottomCentre,
                                            Origin = Anchor.BottomCentre,
                                        },
                                        new OsuSpriteText
                                        {
                                            Text = "miss",
                                            Anchor = Anchor.BottomCentre,
                                            Origin = Anchor.BottomCentre,
                                        },
                                    },
                                }.Concat(new[]
                                {
                                    "head",
                                    "tick",
                                    "repeat",
                                    "tail",
                                    "slider",
                                }.Select((label, hitObjectIndex) => new Drawable[]
                                {
                                    new OsuSpriteText
                                    {
                                        Text = label,
                                        Anchor = Anchor.CentreRight,
                                        Origin = Anchor.CentreRight,
                                    },
                                    judgementContainers[cellIndex, hitObjectIndex, 0] =
                                        new Container<DrawableOsuJudgement> { RelativeSizeAxes = Axes.Both },
                                    judgementContainers[cellIndex, hitObjectIndex, 1] =
                                        new Container<DrawableOsuJudgement> { RelativeSizeAxes = Axes.Both },
                                })).ToArray(),
                        },
                    },
                };

                cellIndex++;

                return container;
            });

            AddToggleStep("Toggle classic behaviour", c => classic = c);

            AddStep("Show judgements", createAllJudgements);
        }

        private void createAllJudgements()
        {
            for (int cellIndex = 0; cellIndex < Rows * Cols; cellIndex++)
            {
                for (int hitObjectIndex = 0; hitObjectIndex < 5; hitObjectIndex++)
                {
                    createJudgement(cellIndex, hitObjectIndex, true);
                    createJudgement(cellIndex, hitObjectIndex, false);
                }
            }
        }

        private void createJudgement(int cellIndex, int hitObjectIndex, bool hit)
        {
            var container = judgementContainers[cellIndex, hitObjectIndex, hit ? 0 : 1];
            container.Clear(false);

            var slider = new Slider { StartTime = Time.Current, ClassicSliderBehaviour = classic };
            slider.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());
            OsuHitObject hitObject = hitObjectIndex switch
            {
                0 => new SliderHeadCircle { StartTime = Time.Current, ClassicSliderBehaviour = classic },
                1 => new SliderTick { StartTime = Time.Current },
                2 => new SliderRepeat(slider) { StartTime = Time.Current },
                3 => new SliderTailCircle(slider) { StartTime = Time.Current, ClassicSliderBehaviour = classic },
                4 => slider,
                _ => throw new UnreachableException(),
            };

            DrawableOsuHitObject drawableHitObject = hitObject switch
            {
                SliderHeadCircle head => new DrawableSliderHead(head),
                SliderTick tick => new DrawableSliderTick(tick),
                SliderRepeat repeat => new DrawableSliderRepeat(repeat),
                SliderTailCircle tail => new DrawableSliderTail(tail),
                Slider s => new DrawableSlider(s),
                _ => throw new UnreachableException(),
            };

            if (!drawableHitObject.DisplayResult)
                return;

            var result = new OsuJudgementResult(hitObject, hitObject.Judgement)
            {
                Type = hit ? hitObject.Judgement.MaxResult : hitObject.Judgement.MinResult,
            };

            var judgement = judgementPools[cellIndex].Get(result.Type, d =>
            {
                d.Anchor = Anchor.Centre;
                d.Origin = Anchor.Centre;
                d.Scale = new Vector2(0.7f);
                d.Apply(result, null);
            });

            if (judgement != null)
                container.Add(judgement);
        }
    }
}
