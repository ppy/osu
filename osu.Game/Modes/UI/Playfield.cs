﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Objects.Drawables;
using OpenTK;

namespace osu.Game.Modes.UI
{
    public abstract class Playfield<T> : Container
        where T : HitObject
    {
        public HitObjectContainer<DrawableHitObject<T>> HitObjects;

        public virtual void Add(DrawableHitObject<T> h) => HitObjects.Add(h);

        protected internal Container<Drawable> ScaledContent;

        public override bool Contains(Vector2 screenSpacePos) => true;

        protected override Container<Drawable> Content => content;

        private Container<Drawable> content;

        /// <summary>
        /// A container for keeping track of DrawableHitObjects.
        /// </summary>
        /// <param name="customWidth">Whether we want our internal coordinate system to be scaled to a specified width.</param>
        protected Playfield(float? customWidth = null)
        {
            AddInternal(ScaledContent = new ScaledContainer
            {
                CustomWidth = customWidth,
                RelativeSizeAxes = Axes.Both,
                Children = new[]
                {
                    content = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                    }
                }
            });

            HitObjects = new HitObjectContainer<DrawableHitObject<T>>
            {
                RelativeSizeAxes = Axes.Both,
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(HitObjects);
        }

        public virtual void PostProcess()
        {
        }

        private class ScaledContainer : Container
        {
            /// <summary>
            /// A value (in game pixels that we should scale our content to match).
            /// </summary>
            public float? CustomWidth;

            //dividing by the customwidth will effectively scale our content to the required container size.
            protected override Vector2 DrawScale => CustomWidth.HasValue ? new Vector2(DrawSize.X / CustomWidth.Value) : base.DrawScale;

            public override bool Contains(Vector2 screenSpacePos) => true;
        }

        public class HitObjectContainer<U> : Container<U> where U : Drawable
        {
            public override bool Contains(Vector2 screenSpacePos) => true;
        }
    }
}
