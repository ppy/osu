// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Objects.Drawables;
using OpenTK;
using osu.Game.Modes.Judgements;

namespace osu.Game.Modes.UI
{
    public abstract class Playfield<TObject, TJudgement> : Container
        where TObject : HitObject
        where TJudgement : JudgementInfo
    {
        /// <summary>
        /// The HitObjects contained in this Playfield.
        /// </summary>
        public HitObjectContainer<DrawableHitObject<TObject, TJudgement>> HitObjects;

        internal Container<Drawable> ScaledContent;

        protected override Container<Drawable> Content => content;
        private Container<Drawable> content;

        /// <summary>
        /// A container for keeping track of DrawableHitObjects.
        /// </summary>
        /// <param name="customWidth">Whether we want our internal coordinate system to be scaled to a specified width.</param>
        protected Playfield(float? customWidth = null)
        {
            AlwaysReceiveInput = true;

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

            Add(HitObjects = new HitObjectContainer<DrawableHitObject<TObject, TJudgement>>
            {
                RelativeSizeAxes = Axes.Both,
            });
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
