//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Modes.Objects.Drawables;
using OpenTK;

namespace osu.Game.Modes.UI
{
    public abstract class Playfield : Container
    {
        public HitObjectContainer HitObjects;

        public virtual void Add(DrawableHitObject h) => HitObjects.Add(h);

        public Playfield()
        {
            AddInternal(HitObjects = new HitObjectContainer
            {
                RelativeSizeAxes = Axes.Both,
            });
        }

        public class HitObjectContainer : Container<DrawableHitObject>
        {
            protected override Vector2 DrawScale => new Vector2(DrawSize.X / 512);
        }
    }
}
