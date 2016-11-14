//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps.Objects;
using osu.Game.Beatmaps.Objects.Catch;

namespace osu.Game.Screens.Play.Catch
{
    public class CatchHitRenderer : HitRenderer<CatchBaseHit>
    {
        protected override HitObjectConverter<CatchBaseHit> Converter => new CatchConverter();

        protected override Playfield CreatePlayfield() => new CatchPlayfield();

        protected override DrawableHitObject GetVisualRepresentation(CatchBaseHit h) => null;// new DrawableFruit(h);
    }
}
