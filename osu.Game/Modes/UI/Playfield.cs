// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Screens.Play;

namespace osu.Game.Modes.UI
{
    public abstract class Playfield<T> : Container
        where T : HitObject
    {
        public HitObjectContainer<DrawableHitObject<T>> HitObjects;

        public virtual void Add(DrawableHitObject<T> h) => HitObjects.Add(h);

        public class HitObjectContainer<U> : Container<U>
            where U : Drawable
        {
            public override bool Contains(Vector2 screenSpacePos) => true;
        }

        private Container<Drawable> scaledContent;

        public override bool Contains(Vector2 screenSpacePos) => true;

        protected override Container<Drawable> Content { get; }

        public Playfield()
        {
            AddInternal(scaledContent = new ScaledContainer
            {
                RelativeSizeAxes = Axes.Both,
                Children = new[]
                {
                    Content = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                    }
                }
            });

            Add(HitObjects = new HitObjectContainer<DrawableHitObject<T>>
            {
                RelativeSizeAxes = Axes.Both,
            });
        }

        /// <summary>
        /// An optional inputManager to provide interactivity etc.
        /// </summary>
        public PlayerInputManager InputManager;

        [BackgroundDependencyLoader]
        private void load()
        {
            if (InputManager != null)
            {
                //if we've been provided an InputManager, we want it to sit inside the scaledcontainer 
                scaledContent.Remove(Content);
                scaledContent.Add(InputManager);
                InputManager.Add(Content);
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
