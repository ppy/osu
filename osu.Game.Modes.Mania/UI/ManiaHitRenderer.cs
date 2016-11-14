//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Modes.Mania.Objects;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.UI;

namespace osu.Game.Modes.Mania.UI
{
    public class ManiaHitRenderer : HitRenderer<ManiaBaseHit>
    {
        private readonly int columns;

        public ManiaHitRenderer(int columns = 5)
        {
            this.columns = columns;
        }

        protected override HitObjectConverter<ManiaBaseHit> Converter => new ManiaConverter(columns);

        protected override Playfield CreatePlayfield() => new ManiaPlayfield(columns);

        protected override DrawableHitObject GetVisualRepresentation(ManiaBaseHit h)
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
