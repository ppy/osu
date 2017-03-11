// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Modes.Catch.Objects;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.UI;

namespace osu.Game.Modes.Catch.UI
{
    public class CatchHitRenderer : HitRenderer<CatchBaseHit>
    {
        public CatchHitRenderer(Beatmap beatmap)
            : base(beatmap)
        {
        }

        protected override PlayMode PlayMode => PlayMode.Catch;

        protected override Playfield<CatchBaseHit> CreatePlayfield() => new CatchPlayfield();

        protected override DrawableHitObject<CatchBaseHit> GetVisualRepresentation(CatchBaseHit h) => null;// new DrawableFruit(h);
    }
}
