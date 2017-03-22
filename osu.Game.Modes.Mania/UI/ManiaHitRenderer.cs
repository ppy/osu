// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Modes.Mania.Beatmaps;
using osu.Game.Modes.Mania.Judgements;
using osu.Game.Modes.Mania.Objects;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.UI;

namespace osu.Game.Modes.Mania.UI
{
    public class ManiaHitRenderer : HitRenderer<ManiaBaseHit, ManiaJudgementInfo>
    {
        private readonly int columns;

        public ManiaHitRenderer(WorkingBeatmap beatmap, int columns = 5)
            : base(beatmap)
        {
            this.columns = columns;
        }

        public override ScoreProcessor CreateScoreProcessor() => new ManiaScoreProcessor(this);

        protected override IBeatmapConverter<ManiaBaseHit> CreateBeatmapConverter() => new ManiaBeatmapConverter();

        protected override IBeatmapProcessor<ManiaBaseHit> CreateBeatmapProcessor() => new ManiaBeatmapProcessor();

        protected override Playfield<ManiaBaseHit, ManiaJudgementInfo> CreatePlayfield() => new ManiaPlayfield(columns);

        protected override DrawableHitObject<ManiaBaseHit, ManiaJudgementInfo> GetVisualRepresentation(ManiaBaseHit h) => null;
    }
}
