// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Game.Modes.Objects.Drawables;

namespace osu.Game.Modes.UI
{
    public abstract class Playfield : Container
    {
        public HitObjectContainer HitObjects;
        private Container<Drawable> scaledContent;

        public virtual void Add(DrawableHitObject h) => HitObjects.Add(h);

        public override bool Contains(Vector2 screenSpacePos) => true;

        protected override Container<Drawable> Content => content;

        private Container content;

        public Playfield()
        {
            AddInternal(scaledContent = new ScaledContainer
            {
                RelativeSizeAxes = Axes.Both,
                Children = new[]
                {
                    content = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                    }
                }
            });

            Add(HitObjects = new HitObjectContainer
            {
                RelativeSizeAxes = Axes.Both,
            });
        }

        /// <summary>
        /// An optional inputManager to provide interactivity etc.
        /// </summary>
        public InputManager InputManager;

        [BackgroundDependencyLoader]
        private void load()
        {
            if (InputManager != null)
            {
                //if we've been provided an InputManager, we want it to sit inside the scaledcontainer 
                scaledContent.Remove(content);
                scaledContent.Add(InputManager);
                InputManager.Add(content);
            }
        }

        public virtual void PostProcess()
        {
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
