//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Beatmaps.Objects;
using osu.Game.Beatmaps.Objects.Catch;
using osu.Game.Beatmaps.Objects.Catch.Drawable;

namespace osu.Game.GameModes.Play.Catch
{
    public class CatchHitRenderer : HitRenderer<CatchBaseHit>
    {
        protected override HitObjectConverter<CatchBaseHit> Converter => new CatchConverter();

        protected override Playfield CreatePlayfield() => new CatchPlayfield();

        protected override Drawable GetVisualRepresentation(CatchBaseHit h) => new DrawableFruit(h);
    }
}
