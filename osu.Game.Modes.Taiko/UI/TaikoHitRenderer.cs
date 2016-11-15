//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Modes.Objects;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Taiko.Objects;
using osu.Game.Modes.UI;

namespace osu.Game.Modes.Taiko.UI
{
    public class TaikoHitRenderer : HitRenderer<TaikoBaseHit>
    {
        protected override HitObjectConverter<TaikoBaseHit> Converter => new TaikoConverter();

        protected override Playfield CreatePlayfield() => new TaikoPlayfield();

        protected override DrawableHitObject GetVisualRepresentation(TaikoBaseHit h) => null;// new DrawableTaikoHit(h);
    }
}
