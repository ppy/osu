// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using OpenTK;
using osu.Framework.Allocation;

namespace osu.Game.Rulesets.UI
{
    public abstract class Playfield : Container
    {
        /// <summary>
        /// The HitObjects contained in this Playfield.
        /// </summary>
        public HitObjectContainer HitObjects { get; private set; }

        public Container<Drawable> ScaledContent;

        protected override Container<Drawable> Content => content;
        private readonly Container<Drawable> content;

        private List<Playfield> nestedPlayfields;

        /// <summary>
        /// All the <see cref="Playfield"/>s nested inside this playfield.
        /// </summary>
        public IReadOnlyList<Playfield> NestedPlayfields => nestedPlayfields;

        /// <summary>
        /// A container for keeping track of DrawableHitObjects.
        /// </summary>
        /// <param name="customWidth">Whether we want our internal coordinate system to be scaled to a specified width.</param>
        protected Playfield(float? customWidth = null)
        {
            // Default height since we force relative size axes
            Size = Vector2.One;

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
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            HitObjects = CreateHitObjectContainer();
            HitObjects.RelativeSizeAxes = Axes.Both;

            Add(HitObjects);
        }

        public override Axes RelativeSizeAxes
        {
            get { return Axes.Both; }
            set { throw new InvalidOperationException($@"{nameof(Playfield)}'s {nameof(RelativeSizeAxes)} should never be changed from {Axes.Both}"); }
        }

        /// <summary>
        /// Performs post-processing tasks (if any) after all DrawableHitObjects are loaded into this Playfield.
        /// </summary>
        public virtual void PostProcess() => nestedPlayfields?.ForEach(p => p.PostProcess());

        /// <summary>
        /// Adds a DrawableHitObject to this Playfield.
        /// </summary>
        /// <param name="h">The DrawableHitObject to add.</param>
        public virtual void Add(DrawableHitObject h) => HitObjects.Add(h);

        /// <summary>
        /// Remove a DrawableHitObject from this Playfield.
        /// </summary>
        /// <param name="h">The DrawableHitObject to remove.</param>
        public virtual void Remove(DrawableHitObject h) => HitObjects.Remove(h);

        /// <summary>
        /// Registers a <see cref="Playfield"/> as a nested <see cref="Playfield"/>.
        /// This does not add the <see cref="Playfield"/> to the draw hierarchy.
        /// </summary>
        /// <param name="otherPlayfield">The <see cref="Playfield"/> to add.</param>
        protected void AddNested(Playfield otherPlayfield)
        {
            if (nestedPlayfields == null)
                nestedPlayfields = new List<Playfield>();

            nestedPlayfields.Add(otherPlayfield);
        }

        /// <summary>
        /// Creates the container that will be used to contain the <see cref="DrawableHitObject"/>s.
        /// </summary>
        protected virtual HitObjectContainer CreateHitObjectContainer() => new HitObjectContainer();

        private class ScaledContainer : Container
        {
            /// <summary>
            /// A value (in game pixels that we should scale our content to match).
            /// </summary>
            public float? CustomWidth;

            //dividing by the customwidth will effectively scale our content to the required container size.
            protected override Vector2 DrawScale => CustomWidth.HasValue ? new Vector2(DrawSize.X / CustomWidth.Value) : base.DrawScale;

            protected override void Update()
            {
                base.Update();
                RelativeChildSize = new Vector2(DrawScale.X, RelativeChildSize.Y);
            }
        }
    }
}
