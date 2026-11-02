// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Lines;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Localisation;
using osu.Game.Localisation.SkinComponents;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.RankingV2.Legacy
{
    /// <seealso href="https://github.com/peppy/osu-stable-reference/blob/6b3838c41c4b8005cadf7db0b503ae967776d9f9/osu!/GameModes/Ranking/Ranking.cs#L1043-L1141"/>
    public partial class LegacyRankingHealthGraph : CompositeDrawable, ISerialisableDrawable, IAnimatableSkinnable, IHasTooltip
    {
        public LocalisableString TooltipText { get; private set; }

        private Container graphContainer = null!;
        private Container passingContainer = null!;
        private Container failingContainer = null!;
        private SmoothPath passingPath = null!;
        private SmoothPath failingPath = null!;
        private Vector2 pathSize;

        [Resolved]
        private IBindable<IScoreInfo> score { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin)
        {
            AutoSizeAxes = Axes.Both;

            bool useNewLayout = skin.GetConfig<SkinConfiguration.LegacySetting, decimal>(SkinConfiguration.LegacySetting.Version)?.Value > 1M;

            // https://github.com/peppy/osu-stable-reference/blob/6b3838c41c4b8005cadf7db0b503ae967776d9f9/osu!/GameModes/Ranking/Ranking.cs#L467
            var graphSize = new Vector2(186, 86) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR;
            const float graph_path_radius = 3;
            pathSize = graphSize - 2 * new Vector2(graph_path_radius);

            InternalChildren =
            [
                new Sprite
                {
                    Texture = skin.GetTexture(@"ranking-graph"),
                },
                graphContainer = new Container
                {
                    // https://github.com/peppy/osu-stable-reference/blob/6b3838c41c4b8005cadf7db0b503ae967776d9f9/osu!/GameModes/Ranking/Ranking.cs#L463-L465
                    Position = new Vector2(useNewLayout ? 5 : 10) * LegacySkin.STABLE_MAGIC_SCALE_FACTOR,
                    Size = graphSize + new Vector2(2 * graph_path_radius, 0),
                    Children =
                    [
                        // Transition from green to red line (passing/failing) is implemented
                        // by having two copies of the graph path.
                        // One copy is masked to its upper half and is green,
                        // and the other copy is masked to its lower half and is red.
                        // This is different to stable which would colour individual segments of the line
                        // based whether the health value at the end of the line was below 0.5:
                        // https://github.com/peppy/osu-stable-reference/blob/6b3838c41c4b8005cadf7db0b503ae967776d9f9/osu!/GameModes/Ranking/Ranking.cs#L1090
                        passingContainer = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            Height = 0.5f,
                            Colour = Colour4.YellowGreen,
                            Child = passingPath = new SmoothPath
                            {
                                Anchor = Anchor.TopLeft,
                                Origin = Anchor.TopLeft,
                                PathRadius = graph_path_radius,
                            },
                        },
                        failingContainer = new Container
                        {
                            RelativePositionAxes = Axes.Y,
                            Y = 0.5f,
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            Height = 0.5f,
                            Colour = Colour4.Red,
                            Child = failingPath = new SmoothPath
                            {
                                Anchor = Anchor.BottomLeft,
                                PathRadius = graph_path_radius,
                            },
                        },
                    ],
                },
            ];

            passingPath.OriginPosition = failingPath.PositionInBoundingBox(new Vector2(-graph_path_radius, -graph_path_radius));
            failingPath.OriginPosition = failingPath.PositionInBoundingBox(new Vector2(-graph_path_radius, -graph_path_radius + graphSize.Y));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            score.BindValueChanged(_ => updateState(), true);
        }

        private void updateState()
        {
            if (score.Value is IScoreWithReplayData hasReplayData)
            {
                var graph = getDownsampledGraph(hasReplayData);

                graphContainer.Alpha = graph.Count > 0 ? 1 : 0;
                passingPath.Vertices = failingPath.Vertices = graph;

                updateTooltipText(hasReplayData);
            }
            else
            {
                graphContainer.Alpha = 0;
                TooltipText = default;
            }
        }

        private List<Vector2> getDownsampledGraph(IScoreWithReplayData scoreWithReplayData)
        {
            if (scoreWithReplayData.HealthGraph.Count < 2)
                return [];

            var hpGraph = scoreWithReplayData.HealthGraph;

            double startTime = hpGraph[0].Time;
            double endTime = hpGraph[^1].Time;
            double timeRange = endTime - startTime;

            // https://github.com/peppy/osu-stable-reference/blob/6b3838c41c4b8005cadf7db0b503ae967776d9f9/osu!/GameModes/Ranking/Ranking.cs#L1074-L1078
            int stride = 1;
            while (hpGraph.Count / stride > 100)
                stride *= 2;

            var result = new List<Vector2>(hpGraph.Count / stride);

            for (int i = 0; i < hpGraph.Count; i += stride)
            {
                var point = hpGraph[i];
                double x = (point.Time - startTime) / timeRange;
                double y = 1 - point.Health;
                result.Add(new Vector2((float)x, (float)y) * pathSize);
            }

            return result;
        }

        /// <seealso href="https://github.com/peppy/osu-stable-reference/blob/6b3838c41c4b8005cadf7db0b503ae967776d9f9/osu!/GameModes/Ranking/Ranking.cs#L416-L428"/>
        /// <remarks>
        /// Spinning statistics (see https://github.com/peppy/osu-stable-reference/blob/6b3838c41c4b8005cadf7db0b503ae967776d9f9/osu!/GameModes/Ranking/Ranking.cs#L430-L437)
        /// currently not implemented.
        /// </remarks>
        private void updateTooltipText(IScoreWithReplayData hasReplayData)
        {
            if (hasReplayData.HitEvents.Count == 0)
            {
                TooltipText = default;
                return;
            }

            double earlySum = 0, lateSum = 0;
            int earlyCount = 0, lateCount = 0;

            foreach (var hitEvent in hasReplayData.HitEvents)
            {
                if (hitEvent.TimeOffset >= 0)
                {
                    lateSum += hitEvent.TimeOffset;
                    lateCount++;
                }
                else
                {
                    earlySum += hitEvent.TimeOffset;
                    earlyCount++;
                }
            }

            double earlyAverage = earlyCount > 0 ? earlySum / earlyCount : 0;
            double lateAverage = lateCount > 0 ? lateSum / lateCount : 0;

            var unstableRate = hasReplayData.HitEvents.CalculateUnstableRate();
            TooltipText = ResultsScreenStrings.LegacyHealthGraphTooltipText(earlyAverage, lateAverage, unstableRate?.Result ?? 0);
        }

        #region ISerialisableDrawable

        public bool UsesFixedAnchor { get; set; }

        #endregion

        #region IAnimatableSkinnable

        [SettingSource(nameof(SkinnableComponentStrings), nameof(SkinnableComponentStrings.AnimationSequence))]
        public BindableInt GroupNumber { get; } = new BindableInt
        {
            MinValue = 0,
            MaxValue = 10,
        };

        int IAnimatableSkinnable.GroupNumber => GroupNumber.Value;

        public double StartAnimating(double startTime)
        {
            FinishAnimating();

            // https://github.com/peppy/osu-stable-reference/blob/6b3838c41c4b8005cadf7db0b503ae967776d9f9/osu!/GameModes/Ranking/Ranking.cs#L467
            const double graph_reveal_duration = 4000;

            passingContainer.ResizeWidthTo(0)
                            .Then()
                            .ResizeWidthTo(1, graph_reveal_duration);
            failingContainer.ResizeWidthTo(0)
                            .Then()
                            .ResizeWidthTo(1, graph_reveal_duration);

            // do not delay any further animations in the sequence with this element's animation
            return startTime;
        }

        public void FinishAnimating()
        {
            passingContainer.FinishTransforms();
            failingContainer.FinishTransforms();
        }

        #endregion
    }
}
