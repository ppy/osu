//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Beatmaps.Objects;
using osu.Game.Beatmaps.Objects.Mania;
using OpenTK;
using osu.Game.Beatmaps.Objects.Mania.Drawable;
using System.Collections.Generic;

namespace osu.Game.GameModes.Play.Mania
{
    public class ManiaHitRenderer : HitRenderer<ManiaBaseHit>
    {
        private readonly int columns;

        public ManiaHitRenderer(int columns = 5)
        {
            this.columns = columns;
        }

        protected override List<ManiaBaseHit> Convert(List<HitObject> objects)
        {
            ManiaConverter converter = new ManiaConverter(columns);
            return converter.Convert(objects);
        }

        protected override Playfield CreatePlayfield() => new ManiaPlayfield(columns);

        protected override Drawable GetVisualRepresentation(ManiaBaseHit h)
        {
            return new DrawableNote(h)
            {
                Position = new Vector2((float)(h.Column + 0.5) / columns, -0.1f),
                RelativePositionAxes = Axes.Both
            };
        }
    }
}
