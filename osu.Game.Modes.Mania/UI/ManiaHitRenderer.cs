// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Modes.Mania.Beatmaps;
using osu.Game.Modes.Mania.Objects;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.UI;

namespace osu.Game.Modes.Mania.UI
{
    public class ManiaHitRenderer : HitRenderer<ManiaBaseHit>
    {
        private readonly int columns;

        public ManiaHitRenderer(WorkingBeatmap beatmap, int columns = 5)
            : base(beatmap)
        {
            this.columns = columns;
        }

        protected override IBeatmapConverter<ManiaBaseHit> CreateBeatmapConverter() => new ManiaBeatmapConverter();

        protected override Playfield<ManiaBaseHit> CreatePlayfield() => new ManiaPlayfield(columns);

        protected override DrawableHitObject<ManiaBaseHit> GetVisualRepresentation(ManiaBaseHit h)
        {
            return null;
            //return new DrawableNote(h)
            //{
            //    Position = new Vector2((float)(h.Column + 0.5) / columns, -0.1f),
            //    RelativePositionAxes = Axes.Both
            //};
        }
    }
}
