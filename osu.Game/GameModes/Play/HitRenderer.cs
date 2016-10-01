//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Batches;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Drawables;
using osu.Game.Beatmaps.Objects;
using OpenTK;

namespace osu.Game.GameModes.Play
{
    public abstract class HitRenderer : Container
    {
        public abstract List<HitObject> Objects { set; }

        public HitRenderer()
        {
            RelativeSizeAxes = Axes.Both;
        }

        public override void Load()
        {
            base.Load();

            Add(new Box() { RelativeSizeAxes = Axes.Both, Alpha = 0.1f, Scale = new Vector2(0.99f) });
        }
    }
}
