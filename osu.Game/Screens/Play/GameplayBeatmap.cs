// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Screens.Play
{
    public class GameplayBeatmap : Component, IBeatmap
    {
        public readonly IBeatmap PlayableBeatmap;

        private readonly GameplayClockContainer gameplayClockContainer;

        public GameplayBeatmap(IBeatmap playableBeatmap, GameplayClockContainer gameplayClockContainer = null)
        {
            PlayableBeatmap = playableBeatmap;

            if (gameplayClockContainer != null)
                this.gameplayClockContainer = gameplayClockContainer;
        }

        public BeatmapInfo BeatmapInfo
        {
            get => PlayableBeatmap.BeatmapInfo;
            set => PlayableBeatmap.BeatmapInfo = value;
        }

        public BeatmapMetadata Metadata => PlayableBeatmap.Metadata;

        public ControlPointInfo ControlPointInfo => PlayableBeatmap.ControlPointInfo;

        public List<BreakPeriod> Breaks => PlayableBeatmap.Breaks;

        public double TotalBreakTime => PlayableBeatmap.TotalBreakTime;

        public IReadOnlyList<HitObject> HitObjects => PlayableBeatmap.HitObjects;

        public IEnumerable<BeatmapStatistic> GetStatistics() => PlayableBeatmap.GetStatistics();

        public IBeatmap Clone() => PlayableBeatmap.Clone();

        private readonly Bindable<JudgementResult> lastJudgementResult = new Bindable<JudgementResult>();

        public IBindable<JudgementResult> LastJudgementResult => lastJudgementResult;

        public BindableNumber<double> UserPlaybackRate => gameplayClockContainer.UserPlaybackRate;

        public void ApplyResult(JudgementResult result) => lastJudgementResult.Value = result;
    }
}
