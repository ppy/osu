//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Modes.Objects;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Osu.Objects;
using osu.Game.Modes.Osu.Objects.Drawables;
using osu.Game.Modes.UI;
using OsuBaseHit = osu.Game.Modes.Osu.Objects.OsuBaseHit;
using OsuConverter = osu.Game.Modes.Osu.Objects.OsuConverter;

namespace osu.Game.Modes.Osu.UI
{
    public class OsuHitRenderer : HitRenderer<OsuBaseHit>
    {
        protected override HitObjectConverter<OsuBaseHit> Converter => new OsuConverter();

        protected override Playfield CreatePlayfield() => new OsuPlayfield();

        protected override DrawableHitObject GetVisualRepresentation(OsuBaseHit h)
        {
            if (h is HitCircle)
                return new DrawableHitCircle(h as HitCircle);
            if (h is Slider)
                return new DrawableSlider(h as Slider);

            return null;
        }
    }
}
