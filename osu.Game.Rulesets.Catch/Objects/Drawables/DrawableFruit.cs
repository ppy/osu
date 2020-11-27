// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Utils;
using osu.Game.Rulesets.Catch.Objects.Drawables.Pieces;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Catch.Objects.Drawables
{
    public class DrawableFruit : DrawablePalpableCatchHitObject
    {
        public readonly Bindable<int> IndexInBeatmap = new Bindable<int>();

        public readonly Bindable<FruitVisualRepresentation> VisualRepresentation = new Bindable<FruitVisualRepresentation>();

        protected virtual FruitVisualRepresentation GetVisualRepresentation(int indexInBeatmap) => (FruitVisualRepresentation)(indexInBeatmap % 4);

        private FruitPiece fruitPiece;

        public DrawableFruit(CatchHitObject h)
            : base(h)
        {
            IndexInBeatmap.Value = h.IndexInBeatmap;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            ScaleContainer.Rotation = (float)(RNG.NextDouble() - 0.5f) * 40;

            IndexInBeatmap.BindValueChanged(change =>
            {
                VisualRepresentation.Value = GetVisualRepresentation(change.NewValue);
            }, true);

            VisualRepresentation.BindValueChanged(_ => updatePiece());
            HyperDash.BindValueChanged(_ => updatePiece(), true);
        }

        private void updatePiece()
        {
            ScaleContainer.Child = new SkinnableDrawable(
                new CatchSkinComponent(getComponent(VisualRepresentation.Value)),
                _ => fruitPiece = new FruitPiece
                {
                    VisualRepresentation = { BindTarget = VisualRepresentation },
                    HyperDash = { BindTarget = HyperDash },
                });
        }

        protected override void OnApply()
        {
            base.OnApply();

            IndexInBeatmap.BindTo(HitObject.IndexInBeatmapBindable);
        }

        protected override void OnFree()
        {
            IndexInBeatmap.UnbindFrom(HitObject.IndexInBeatmapBindable);

            base.OnFree();
        }

        protected override void Update()
        {
            base.Update();

            if (fruitPiece != null)
                fruitPiece.Border.Alpha = (float)Math.Clamp((StartTimeBindable.Value - Time.Current) / 500, 0, 1);
        }

        private CatchSkinComponents getComponent(FruitVisualRepresentation hitObjectVisualRepresentation)
        {
            switch (hitObjectVisualRepresentation)
            {
                case FruitVisualRepresentation.Pear:
                    return CatchSkinComponents.FruitPear;

                case FruitVisualRepresentation.Grape:
                    return CatchSkinComponents.FruitGrapes;

                case FruitVisualRepresentation.Pineapple:
                    return CatchSkinComponents.FruitApple;

                case FruitVisualRepresentation.Raspberry:
                    return CatchSkinComponents.FruitOrange;

                case FruitVisualRepresentation.Banana:
                    return CatchSkinComponents.FruitBananas;

                default:
                    throw new ArgumentOutOfRangeException(nameof(hitObjectVisualRepresentation), hitObjectVisualRepresentation, null);
            }
        }
    }

    public enum FruitVisualRepresentation
    {
        Pear,
        Grape,
        Pineapple,
        Raspberry,
        Banana // banananananannaanana
    }
}
