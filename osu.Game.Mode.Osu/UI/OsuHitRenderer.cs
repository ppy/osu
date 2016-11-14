//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Modes.Objects;
using osu.Game.Modes.UI;
using Circle = osu.Game.Modes.Osu.Objects.Circle;
using DrawableCircle = osu.Game.Modes.Osu.Objects.Drawables.DrawableCircle;
using OsuBaseHit = osu.Game.Modes.Osu.Objects.OsuBaseHit;
using OsuConverter = osu.Game.Modes.Osu.Objects.OsuConverter;

namespace osu.Game.Modes.Osu.UI
{
    public class OsuHitRenderer : HitRenderer<OsuBaseHit>
    {
        protected override HitObjectConverter<OsuBaseHit> Converter => new OsuConverter();

        protected override Playfield CreatePlayfield() => new OsuPlayfield();

        protected override DrawableHitObject GetVisualRepresentation(OsuBaseHit h)
            => h is Circle ? new DrawableCircle(h as Circle) : null;
    }
}
