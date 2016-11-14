//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Modes.Objects;
using osu.Game.Modes.Objects.Osu;
using osu.Game.Modes.Objects.Osu.Drawable;

namespace osu.Game.Modes.Osu
{
    public class OsuHitRenderer : HitRenderer<OsuBaseHit>
    {
        protected override HitObjectConverter<OsuBaseHit> Converter => new OsuConverter();

        protected override Playfield CreatePlayfield() => new OsuPlayfield();

        protected override DrawableHitObject GetVisualRepresentation(OsuBaseHit h)
            => h is Circle ? new DrawableCircle(h as Circle) : null;
    }
}
