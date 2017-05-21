// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using OpenTK;
using osu.Game.Rulesets.Judgements;
using osu.Framework.Allocation;

namespace osu.Game.Rulesets.UI
{
    public abstract class Playfield<TObject, TJudgement> : Container
        where TObject : HitObject
        where TJudgement : Judgement
    {
        /// <summary>
        /// The HitObjects contained in this Playfield.
        /// </summary>
        protected HitObjectContainer<DrawableHitObject<TObject, TJudgement>> HitObjects;

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
            AlwaysReceiveInput = true;

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
                        AlwaysReceiveInput = true,
                        RelativeSizeAxes = Axes.Both,
                    }
                }
            });

            HitObjects = new HitObjectContainer<DrawableHitObject<TObject, TJudgement>>
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
            set { throw new InvalidOperationException($@"{nameof(Playfield<TObject, TJudgement>)}'s {nameof(RelativeSizeAxes)} should never be changed from {Axes.Both}"); }
        }

        /// <summary>
        /// Performs post-processing tasks (if any) after all DrawableHitObjects are loaded into this Playfield.
        /// </summary>
        public virtual void PostProcess() { }

        /// <summary>
        /// Adds a DrawableHitObject to this Playfield.
        /// </summary>
        /// <param name="h">The DrawableHitObject to add.</param>
        public virtual void Add(DrawableHitObject<TObject, TJudgement> h) => HitObjects.Add(h);

        /// <summary>
        /// Triggered when an object's Judgement is updated.
        /// </summary>
        /// <param name="judgedObject">The object that Judgement has been updated for.</param>
        public virtual void OnJudgement(DrawableHitObject<TObject, TJudgement> judgedObject) { }

        private class ScaledContainer : Container
        {
            /// <summary>
            /// A value (in game pixels that we should scale our content to match).
            /// </summary>
            public float? CustomWidth;

            //dividing by the customwidth will effectively scale our content to the required container size.
            protected override Vector2 DrawScale => CustomWidth.HasValue ? new Vector2(DrawSize.X / CustomWidth.Value) : base.DrawScale;

            public ScaledContainer()
            {
                AlwaysReceiveInput = true;
            }
        }

        public class HitObjectContainer<U> : Container<U> where U : Drawable
        {
            public HitObjectContainer()
            {
                AlwaysReceiveInput = true;
            }
        }
    }
}
