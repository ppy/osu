// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Beatmaps;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Judgements;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Scoring;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mania.UI
{
    public class ManiaHitRenderer : HitRenderer<ManiaBaseHit, ManiaJudgement>
    {
        private readonly int columns;

        public ManiaHitRenderer(WorkingBeatmap beatmap, int columns = 5)
            : base(beatmap)
        {
            this.columns = columns;
        }

        public override ScoreProcessor CreateScoreProcessor() => new ManiaScoreProcessor(this);

        protected override BeatmapConverter<ManiaBaseHit> CreateBeatmapConverter() => new ManiaBeatmapConverter();

        protected override Playfield<ManiaBaseHit, ManiaJudgement> CreatePlayfield() => new ManiaPlayfield(columns);

        protected override DrawableHitObject<ManiaBaseHit, ManiaJudgement> GetVisualRepresentation(ManiaBaseHit h) => null;
    }
}
