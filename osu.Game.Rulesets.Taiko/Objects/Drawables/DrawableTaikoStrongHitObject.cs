// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Game.Audio;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    public abstract class DrawableTaikoStrongHitObject<TObject> : DrawableTaikoHitObject<TObject>
        where TObject : TaikoStrongHitObject
    {
        private readonly Bindable<bool> isStrong;

        private readonly Container<DrawableStrongNestedHit> strongHitContainer;

        protected DrawableTaikoStrongHitObject(TObject hitObject)
            : base(hitObject)
        {
            isStrong = HitObject.IsStrongBindable.GetBoundCopy();

            AddInternal(strongHitContainer = new Container<DrawableStrongNestedHit>());
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            isStrong.BindValueChanged(_ =>
            {
                // will overwrite samples, should only be called on change.
                updateSamplesFromStrong();

                RecreatePieces();
            });
        }

        private HitSampleInfo[] getStrongSamples() => HitObject.Samples.Where(s => s.Name == HitSampleInfo.HIT_FINISH).ToArray();

        protected override void LoadSamples()
        {
            base.LoadSamples();
            isStrong.Value = getStrongSamples().Any();
        }

        private void updateSamplesFromStrong()
        {
            var strongSamples = getStrongSamples();

            if (isStrong.Value != strongSamples.Any())
            {
                if (isStrong.Value)
                    HitObject.Samples.Add(new HitSampleInfo(HitSampleInfo.HIT_FINISH));
                else
                {
                    foreach (var sample in strongSamples)
                        HitObject.Samples.Remove(sample);
                }
            }
        }

        protected override void RecreatePieces()
        {
            base.RecreatePieces();
            if (HitObject.IsStrong)
                Size = BaseSize = new Vector2(TaikoStrongHitObject.DEFAULT_STRONG_SIZE);
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
            strongHitContainer.Clear();
        }

        protected override DrawableHitObject CreateNestedHitObject(HitObject hitObject)
        {
            switch (hitObject)
            {
                case StrongNestedHitObject strong:
                    return CreateStrongHit(strong);
            }

            return base.CreateNestedHitObject(hitObject);
        }

        /// <summary>
        /// Creates the handler for this <see cref="DrawableHitObject"/>'s <see cref="StrongNestedHitObject"/>.
        /// This is only invoked if <see cref="TaikoStrongHitObject.IsStrong"/> is true for <see cref="HitObject"/>.
        /// </summary>
        /// <param name="hitObject">The strong hitobject.</param>
        /// <returns>The strong hitobject handler.</returns>
        protected abstract DrawableStrongNestedHit CreateStrongHit(StrongNestedHitObject hitObject);
    }
}
