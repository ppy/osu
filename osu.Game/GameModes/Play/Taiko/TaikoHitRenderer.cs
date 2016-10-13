//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.Objects;
using osu.Game.Beatmaps.Objects.Taiko;
using osu.Game.Beatmaps.Objects.Taiko.Drawable;

namespace osu.Game.GameModes.Play.Taiko
{
    public class TaikoHitRenderer : HitRenderer<TaikoBaseHit>
    {
        protected override List<TaikoBaseHit> Convert(List<HitObject> objects) => new TaikoConverter().Convert(objects);

        protected override Playfield CreatePlayfield() => new TaikoPlayfield();

        protected override Drawable GetVisualRepresentation(TaikoBaseHit h) => new DrawableTaikoHit(h);
    }
}