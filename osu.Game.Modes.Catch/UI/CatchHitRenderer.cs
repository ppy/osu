// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Modes.Catch.Beatmaps;
using osu.Game.Modes.Catch.Judgements;
using osu.Game.Modes.Catch.Objects;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.UI;

namespace osu.Game.Modes.Catch.UI
{
    public class CatchHitRenderer : HitRenderer<CatchBaseHit, CatchJudgementInfo>
    {
        public CatchHitRenderer(WorkingBeatmap beatmap)
            : base(beatmap)
        {
        }

        public override ScoreProcessor CreateScoreProcessor() => new CatchScoreProcessor(this);

        protected override IBeatmapConverter<CatchBaseHit> CreateBeatmapConverter() => new CatchBeatmapConverter();

        protected override IBeatmapProcessor<CatchBaseHit> CreateBeatmapProcessor() => new CatchBeatmapProcessor();

        protected override Playfield<CatchBaseHit, CatchJudgementInfo> CreatePlayfield() => new CatchPlayfield();

        protected override DrawableHitObject<CatchBaseHit, CatchJudgementInfo> GetVisualRepresentation(CatchBaseHit h) => null;
    }
}
