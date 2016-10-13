//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps.Objects;
using osu.Framework;

namespace osu.Game.GameModes.Play
{
    public abstract class HitRenderer<T> : Container
    {
        private List<T> objects;

        public List<HitObject> Objects
        {
            set
            {
                objects = Convert(value);
                if (IsLoaded)
                    loadObjects();
            }
        }

        private Playfield playfield;

        protected abstract Playfield CreatePlayfield();

        protected abstract List<T> Convert(List<HitObject> objects);

        public override void Load(BaseGame game)
        {
            base.Load(game);

            RelativeSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                playfield = CreatePlayfield()
            };

            loadObjects();
        }

        private void loadObjects()
        {
            if (objects == null) return;
            foreach (T h in objects)
                playfield.Add(GetVisualRepresentation(h));
        }

        protected abstract Drawable GetVisualRepresentation(T h);
    }
}
