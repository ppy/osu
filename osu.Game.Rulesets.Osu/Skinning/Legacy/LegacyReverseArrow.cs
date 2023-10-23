// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
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
        private DrawableSliderRepeat drawableRepeat { get; set; } = null!;

        private Drawable proxy = null!;

        private Bindable<Color4> accentColour = null!;

        private bool textureIsDefaultSkin;

        private Drawable arrow = null!;

        private bool shouldRotate;

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject drawableObject, ISkinSource skinSource)
        {
            drawableRepeat = (DrawableSliderRepeat)drawableObject;

            AutoSizeAxes = Axes.Both;

            string lookupName = new OsuSkinComponentLookup(OsuSkinComponents.ReverseArrow).LookupName;

            var skin = skinSource.FindProvider(s => s.GetTexture(lookupName) != null);

            InternalChild = arrow = (skin?.GetAnimation(lookupName, true, true, maxSize: OsuHitObject.OBJECT_DIMENSIONS * 2) ?? Empty()).With(d =>
            {
                d.Anchor = Anchor.Centre;
                d.Origin = Anchor.Centre;
            });

            textureIsDefaultSkin = skin is ISkinTransformer transformer && transformer.Skin is DefaultLegacySkin;

            drawableObject.ApplyCustomUpdateState += updateStateTransforms;

            shouldRotate = skinSource.GetConfig<SkinConfiguration.LegacySetting, decimal>(SkinConfiguration.LegacySetting.Version)?.Value <= 1;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            proxy = CreateProxy();

            drawableRepeat.HitObjectApplied += onHitObjectApplied;
            onHitObjectApplied(drawableRepeat);

            accentColour = drawableRepeat.AccentColour.GetBoundCopy();
            accentColour.BindValueChanged(c =>
            {
                arrow.Colour = textureIsDefaultSkin && c.NewValue.R + c.NewValue.G + c.NewValue.B > (600 / 255f) ? Color4.Black : Color4.White;
            }, true);
        }

        private void onHitObjectApplied(DrawableHitObject drawableObject)
        {
            Debug.Assert(proxy.Parent == null);

            // see logic in LegacySliderHeadHitCircle.
            drawableRepeat.DrawableSlider.OverlayElementContainer.Add(proxy);
        }

        private void updateStateTransforms(DrawableHitObject hitObject, ArmedState state)
        {
            const double duration = 300;
            const float rotation = 5.625f;

            switch (state)
            {
                case ArmedState.Idle:
                    if (shouldRotate)
                    {
                        InternalChild.ScaleTo(1.3f)
                                     .RotateTo(rotation)
                                     .Then()
                                     .ScaleTo(1f, duration)
                                     .RotateTo(-rotation, duration)
                                     .Loop();
                    }
                    else
                    {
                        InternalChild.ScaleTo(1.3f).Then()
                                     .ScaleTo(1f, duration, Easing.Out)
                                     .Loop();
                    }

                    break;

                case ArmedState.Hit:
                    double animDuration = Math.Min(300, drawableRepeat.HitObject.SpanDuration);
                    InternalChild.ScaleTo(1.4f, animDuration, Easing.Out);
                    break;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (drawableRepeat.IsNotNull())
            {
                drawableRepeat.HitObjectApplied -= onHitObjectApplied;
                drawableRepeat.ApplyCustomUpdateState -= updateStateTransforms;
            }
        }
    }
}
