// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Framework.Allocation;
using osu.Framework.Configuration;

namespace osu.Game.Rulesets.UI
{
    public abstract class Playfield : ScalableContainer
    {
        /// <summary>
        /// The HitObjects contained in this Playfield.
        /// </summary>
        public HitObjectContainer HitObjects { get; private set; }

        /// <summary>
        /// All the <see cref="Playfield"/>s nested inside this playfield.
        /// </summary>
        public IReadOnlyList<Playfield> NestedPlayfields => nestedPlayfields;
        private List<Playfield> nestedPlayfields;

        /// <summary>
        /// Whether judgements should be displayed by this and and all nested <see cref="Playfield"/>s.
        /// </summary>
        public readonly BindableBool DisplayJudgements = new BindableBool(true);

        /// <summary>
        /// A container for keeping track of DrawableHitObjects.
        /// </summary>
        /// <param name="customWidth">The width to scale the internal coordinate space to.
        /// May be null if scaling based on <paramref name="customHeight"/> is desired. If <paramref name="customHeight"/> is also null, no scaling will occur.
        /// </param>
        /// <param name="customHeight">The height to scale the internal coordinate space to.
        /// May be null if scaling based on <paramref name="customWidth"/> is desired. If <paramref name="customWidth"/> is also null, no scaling will occur.
        /// </param>
        protected Playfield(float? customWidth = null, float? customHeight = null)
            : base(customWidth, customHeight)
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            HitObjects = CreateHitObjectContainer();
            HitObjects.RelativeSizeAxes = Axes.Both;

            Add(HitObjects);
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

            otherPlayfield.DisplayJudgements.BindTo(DisplayJudgements);
        }

        /// <summary>
        /// Creates the container that will be used to contain the <see cref="DrawableHitObject"/>s.
        /// </summary>
        protected virtual HitObjectContainer CreateHitObjectContainer() => new HitObjectContainer();
    }
}
