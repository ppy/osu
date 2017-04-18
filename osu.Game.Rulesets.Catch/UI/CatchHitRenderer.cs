// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Beatmaps;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Catch.Judgements;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Scoring;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Catch.UI
{
    public class CatchHitRenderer : HitRenderer<CatchBaseHit, CatchJudgement>
    {
        public CatchHitRenderer(WorkingBeatmap beatmap)
            : base(beatmap)
        {
        }

        public override ScoreProcessor CreateScoreProcessor() => new CatchScoreProcessor(this);

        protected override BeatmapConverter<CatchBaseHit> CreateBeatmapConverter() => new CatchBeatmapConverter();

        protected override Playfield<CatchBaseHit, CatchJudgement> CreatePlayfield() => new CatchPlayfield();

        protected override DrawableHitObject<CatchBaseHit, CatchJudgement> GetVisualRepresentation(CatchBaseHit h) => null;
    }
}
