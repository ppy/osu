//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps.Objects;
using osu.Game.Beatmaps.Objects.Taiko;

namespace osu.Game.Screens.Play.Taiko
{
    public class TaikoHitRenderer : HitRenderer<TaikoBaseHit>
    {
        protected override HitObjectConverter<TaikoBaseHit> Converter => new TaikoConverter();

        protected override Playfield CreatePlayfield() => new TaikoPlayfield();

        protected override DrawableHitObject GetVisualRepresentation(TaikoBaseHit h) => null;// new DrawableTaikoHit(h);
    }
}
