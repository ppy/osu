//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.Objects;
using osu.Game.Beatmaps.Objects.Catch;
using osu.Game.Beatmaps.Objects.Catch.Drawable;

namespace osu.Game.GameModes.Play.Catch
{
    public class CatchHitRenderer : HitRenderer<CatchBaseHit>
    {
        protected override Playfield CreatePlayfield() => new CatchPlayfield();

        protected override List<CatchBaseHit> Convert(List<HitObject> objects) => new CatchConverter().Convert(objects);

        protected override Drawable GetVisualRepresentation(CatchBaseHit h) => new DrawableFruit(h);
    }
}
