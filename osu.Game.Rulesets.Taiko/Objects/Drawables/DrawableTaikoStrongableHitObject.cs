// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using JetBrains.Annotations;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    public abstract partial class DrawableTaikoStrongableHitObject<TObject, TStrongNestedObject> : DrawableTaikoHitObject<TObject>
        where TObject : TaikoStrongableHitObject
        where TStrongNestedObject : StrongNestedHitObject
    {
        private readonly Bindable<bool> isStrong = new BindableBool();

        private readonly Container<DrawableStrongNestedHit> strongHitContainer;

        protected DrawableTaikoStrongableHitObject([CanBeNull] TObject hitObject)
            : base(hitObject)
        {
            AddInternal(strongHitContainer = new Container<DrawableStrongNestedHit>());
        }

        protected override void OnApply()
        {
            isStrong.BindTo(HitObject.IsStrongBindable);
            // this doesn't need to be run inline as RecreatePieces is called by the base call below.
            isStrong.BindValueChanged(_ => Scheduler.AddOnce(RecreatePieces));

            base.OnApply();
        }

        protected override void OnFree()
        {
            base.OnFree();

            isStrong.UnbindFrom(HitObject.IsStrongBindable);
            // ensure the next application does not accidentally overwrite samples.
            isStrong.UnbindEvents();
        }

        protected override void RecreatePieces()
        {
            base.RecreatePieces();
            if (HitObject.IsStrong)
                Size = BaseSize = new Vector2(TaikoStrongableHitObject.DEFAULT_STRONG_SIZE);
        }

        protected override void AddNestedHitObject(DrawableHitObject hitObject)
        {
            base.AddNestedHitObject(hitObject);

            switch (hitObject)
            {
                case DrawableStrongNestedHit strong:
                    strongHitContainer.Add(strong);
                    break;
            }
        }

        protected override void ClearNestedHitObjects()
        {
            base.ClearNestedHitObjects();
            strongHitContainer.Clear(false);
        }

        protected override DrawableHitObject CreateNestedHitObject(HitObject hitObject)
        {
            switch (hitObject)
            {
                case TStrongNestedObject strong:
                    return CreateStrongNestedHit(strong);
            }

            return base.CreateNestedHitObject(hitObject);
        }

        /// <summary>
        /// Creates the handler for this <see cref="DrawableHitObject"/>'s <see cref="StrongNestedHitObject"/>.
        /// This is only invoked if <see cref="TaikoStrongableHitObject.IsStrong"/> is true for <see cref="HitObject"/>.
        /// </summary>
        /// <param name="hitObject">The strong hitobject.</param>
        /// <returns>The strong hitobject handler.</returns>
        protected abstract DrawableStrongNestedHit CreateStrongNestedHit(TStrongNestedObject hitObject);
    }
}
