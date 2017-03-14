// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Taiko.Beatmaps;
using osu.Game.Modes.Taiko.Objects;
using osu.Game.Modes.UI;

namespace osu.Game.Modes.Taiko.UI
{
    public class TaikoHitRenderer : HitRenderer<TaikoBaseHit>
    {
        public TaikoHitRenderer(WorkingBeatmap beatmap)
            : base(beatmap)
        {
        }

        protected override IBeatmapConverter<TaikoBaseHit> CreateBeatmapConverter() => new TaikoBeatmapConverter();

        protected override Playfield<TaikoBaseHit> CreatePlayfield() => new TaikoPlayfield();

        protected override DrawableHitObject<TaikoBaseHit> GetVisualRepresentation(TaikoBaseHit h) => null;// new DrawableTaikoHit(h);
    }
}
