// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Legacy
{
    public partial class LegacyReverseArrow : CompositeDrawable
    {
        [Resolved]
        private DrawableHitObject drawableObject { get; set; } = null!;

        private Drawable proxy = null!;

        private Bindable<Color4> accentColour = null!;

        private bool textureIsDefaultSkin;

        private Drawable arrow = null!;

        private bool shouldRotate;

        [BackgroundDependencyLoader]
        private void load(ISkinSource skinSource)
        {
            AutoSizeAxes = Axes.Both;

            string lookupName = new OsuSkinComponentLookup(OsuSkinComponents.ReverseArrow).LookupName;

            var skin = skinSource.FindProvider(s => s.GetTexture(lookupName) != null);

            InternalChild = arrow = (skin?.GetAnimation(lookupName, true, true, maxSize: OsuHitObject.OBJECT_DIMENSIONS) ?? Empty());
            textureIsDefaultSkin = skin is ISkinTransformer transformer && transformer.Skin is DefaultLegacySkin;

            drawableObject.ApplyCustomUpdateState += updateStateTransforms;

            shouldRotate = skinSource.GetConfig<SkinConfiguration.LegacySetting, decimal>(SkinConfiguration.LegacySetting.Version)?.Value <= 1;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            proxy = CreateProxy();

            drawableObject.HitObjectApplied += onHitObjectApplied;
            onHitObjectApplied(drawableObject);

            accentColour = drawableObject.AccentColour.GetBoundCopy();
            accentColour.BindValueChanged(c =>
            {
                arrow.Colour = textureIsDefaultSkin && c.NewValue.R + c.NewValue.G + c.NewValue.B > (600 / 255f) ? Color4.Black : Color4.White;
            }, true);
        }

        private void onHitObjectApplied(DrawableHitObject drawableObject)
        {
            Debug.Assert(proxy.Parent == null);

            // see logic in LegacySliderHeadHitCircle.
            (drawableObject as DrawableSliderRepeat)?.DrawableSlider
                                                    .OverlayElementContainer.Add(proxy);
        }

        private void updateStateTransforms(DrawableHitObject hitObject, ArmedState state)
        {
            const double move_out_duration = 35;
            const double move_in_duration = 250;
            const double total = 300;

            switch (state)
            {
                case ArmedState.Idle:
                    if (shouldRotate)
                    {
                        InternalChild.ScaleTo(1.3f, move_out_duration)
                                     .RotateTo(5.625f)
                                     .Then()
                                     .ScaleTo(1f, move_in_duration)
                                     .RotateTo(-5.625f, move_in_duration)
                                     .Loop(total - (move_in_duration + move_out_duration));
                    }
                    else
                    {
                        InternalChild.ScaleTo(1.3f)
                                     .Then()
                                     .ScaleTo(1f, move_in_duration)
                                     .Loop(total - (move_in_duration + move_out_duration));
                    }

                    break;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            drawableObject.HitObjectApplied -= onHitObjectApplied;
            drawableObject.ApplyCustomUpdateState -= updateStateTransforms;
        }
    }
}
