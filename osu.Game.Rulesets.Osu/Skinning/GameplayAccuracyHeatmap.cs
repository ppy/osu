// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Game.Skinning;
using osu.Game.Rulesets.Osu.Statistics;
using osu.Game.Scoring;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Scoring;
using System.Collections.Generic;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Objects.Legacy;
using osu.Game.Screens.Play;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Osu.Judgements;

namespace osu.Game.Rulesets.Osu.Skinning
{
    public partial class GameplayAccuracyHeatmap : Container, ISerialisableDrawable
    {
        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

        [Resolved]
        private ScoreProcessor scoreProcessor { get; set; } = null!;

        [Resolved(canBeNull: true)]
        private GameplayClockContainer? gameplayClockContainer { get; set; }

        public bool UsesFixedAnchor { get; set; }

        private float radius;
        private AccuracyHeatmap heatmap = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AutoSizeAxes = Axes.Both;

            radius = OsuHitObject.OBJECT_RADIUS * LegacyRulesetExtensions.CalculateScaleFromCircleSize(beatmap.Value.Beatmap.Difficulty.CircleSize, true);

            initHeatmap();

            if (gameplayClockContainer != null)
                gameplayClockContainer.OnSeek += initHeatmap;

            scoreProcessor.NewJudgement += updateHeatmap;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (gameplayClockContainer != null)
                gameplayClockContainer.OnSeek -= initHeatmap;

            scoreProcessor.NewJudgement -= updateHeatmap;
        }

        private void initHeatmap()
        {
            ScoreInfo scoreInfo = new ScoreInfo { BeatmapInfo = beatmap.Value.BeatmapInfo, HitEvents = (List<HitEvent>)scoreProcessor.HitEvents };
            Child = new Container
            {
                Height = 200,
                Width = 200,
                Child = heatmap = new AccuracyHeatmap(scoreInfo, beatmap.Value.Beatmap, false)
                {
                    RelativeSizeAxes = Axes.Both
                }
            };
        }

        private void updateHeatmap(JudgementResult j)
        {
            if (j is not OsuHitCircleJudgementResult circleJudgementResult || circleJudgementResult.CursorPositionAtHit == null)
                return;

            heatmap.AddPoint(((OsuHitObject)j.HitObject).StackedEndPosition, ((OsuHitObject)j.HitObject).StackedEndPosition, circleJudgementResult.CursorPositionAtHit.Value, radius);
        }
    }
}
