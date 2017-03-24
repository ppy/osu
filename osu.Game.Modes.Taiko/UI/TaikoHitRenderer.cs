// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Scoring;
using osu.Game.Modes.Taiko.Beatmaps;
using osu.Game.Modes.Taiko.Judgements;
using osu.Game.Modes.Taiko.Objects;
using osu.Game.Modes.Taiko.Scoring;
using osu.Game.Modes.UI;

namespace osu.Game.Modes.Taiko.UI
{
    public class TaikoHitRenderer : HitRenderer<TaikoHitObject, TaikoJudgement>
    {
        public TaikoHitRenderer(WorkingBeatmap beatmap)
            : base(beatmap)
        {
        }

        public override ScoreProcessor CreateScoreProcessor() => new TaikoScoreProcessor(this);

        protected override IBeatmapConverter<TaikoHitObject> CreateBeatmapConverter() => new TaikoBeatmapConverter();

        protected override IBeatmapProcessor<TaikoHitObject> CreateBeatmapProcessor() => new TaikoBeatmapProcessor();

        protected override Playfield<TaikoHitObject, TaikoJudgement> CreatePlayfield() => new TaikoPlayfield();

        protected override DrawableHitObject<TaikoHitObject, TaikoJudgement> GetVisualRepresentation(TaikoHitObject h) => null;
    }
}
