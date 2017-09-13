// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using OpenTK;
using osu.Game.Rulesets.Judgements;
using osu.Framework.Allocation;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Rulesets.UI
{
    public abstract class Playfield : Container
    {
        /// <summary>
        /// The HitObjects contained in this Playfield.
        /// </summary>
        public HitObjectContainer HitObjects { get; protected set; }

        internal Container<Drawable> ScaledContent;

        /// <summary>
        /// Whether we are currently providing the local user a gameplay cursor.
        /// </summary>
        public virtual bool ProvidingUserCursor => false;

        protected override Container<Drawable> Content => content;
        private readonly Container<Drawable> content;

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

            HitObjects = new HitObjectContainer
            {
                RelativeSizeAxes = Axes.Both,
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
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
        public virtual void PostProcess() { }

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
        /// Triggered when a new <see cref="Judgement"/> occurs on a <see cref="DrawableHitObject"/>.
        /// </summary>
        /// <param name="judgedObject">The object that <paramref name="judgement"/> occured for.</param>
        /// <param name="judgement">The <see cref="Judgement"/> that occurred.</param>
        public virtual void OnJudgement(DrawableHitObject judgedObject, Judgement judgement) { }

        public class HitObjectContainer : CompositeDrawable
        {
            public virtual IEnumerable<DrawableHitObject> Objects => InternalChildren.OfType<DrawableHitObject>();
            public virtual void Add(DrawableHitObject hitObject) => AddInternal(hitObject);
            public virtual bool Remove(DrawableHitObject hitObject) => RemoveInternal(hitObject);
        }

        private class ScaledContainer : Container
        {
            /// <summary>
            /// A value (in game pixels that we should scale our content to match).
            /// </summary>
            public float? CustomWidth;

            //dividing by the customwidth will effectively scale our content to the required container size.
            protected override Vector2 DrawScale => CustomWidth.HasValue ? new Vector2(DrawSize.X / CustomWidth.Value) : base.DrawScale;
        }
    }
}
