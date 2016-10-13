//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.Objects;
using osu.Game.Beatmaps.Objects.Osu;
using osu.Game.Beatmaps.Objects.Osu.Drawable;

namespace osu.Game.GameModes.Play.Osu
{
    public class OsuHitRenderer : HitRenderer<OsuBaseHit>
    {
        protected override Playfield CreatePlayfield() => new OsuPlayfield();

        protected override List<OsuBaseHit> Convert(List<HitObject> objects) => new OsuConverter().Convert(objects);

        protected override Drawable GetVisualRepresentation(OsuBaseHit h) => new DrawableCircle(h);
    }
}
