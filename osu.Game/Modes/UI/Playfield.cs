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
        private Container<Drawable> content;

        public virtual void Add(DrawableHitObject h) => HitObjects.Add(h);

        public override bool Contains(Vector2 screenSpacePos) => true;

        protected override Container<Drawable> Content => content;

        public Playfield()
        {
            AddInternal(content = new ScaledContainer()
            {
                RelativeSizeAxes = Axes.Both,
            });

            Add(HitObjects = new HitObjectContainer
            {
                RelativeSizeAxes = Axes.Both,
            });
        }

        public class ScaledContainer : Container
        {
            protected override Vector2 DrawScale => new Vector2(DrawSize.X / 512);

            public override bool Contains(Vector2 screenSpacePos) => true;
        }

        public class HitObjectContainer : Container<DrawableHitObject>
        {
            public override bool Contains(Vector2 screenSpacePos) => true;
        }
    }
}
