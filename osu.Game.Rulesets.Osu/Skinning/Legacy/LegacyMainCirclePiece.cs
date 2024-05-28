// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Legacy
{
    public partial class LegacyMainCirclePiece : CompositeDrawable
    {
        public override bool RemoveCompletedTransforms => false;

        /// <summary>
        /// A prioritised prefix to perform texture lookups with.
        /// </summary>
        private readonly string? priorityLookupPrefix;

        private readonly bool hasNumber;

        protected LegacyKiaiFlashingDrawable CircleSprite = null!;
        protected LegacyKiaiFlashingDrawable OverlaySprite = null!;

        protected Container OverlayLayer { get; private set; } = null!;

        private SkinnableSpriteText hitCircleText = null!;

        private readonly Bindable<Color4> accentColour = new Bindable<Color4>();
        private readonly IBindable<int> indexInCurrentCombo = new Bindable<int>();

        [Resolved(canBeNull: true)] // Can't really be null but required to handle potential of disposal before DI completes.
        private DrawableHitObject? drawableObject { get; set; }

        [Resolved]
        private ISkinSource skin { get; set; } = null!;

        public LegacyMainCirclePiece(string? priorityLookupPrefix = null, bool hasNumber = true)
        {
            this.priorityLookupPrefix = priorityLookupPrefix;
            this.hasNumber = hasNumber;

            Size = OsuHitObject.OBJECT_DIMENSIONS;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var drawableOsuObject = (DrawableOsuHitObject?)drawableObject;

            // if a base texture for the specified prefix exists, continue using it for subsequent lookups.
            // otherwise fall back to the default prefix "hitcircle".
            string circleName = (priorityLookupPrefix != null && skin.GetTexture(priorityLookupPrefix) != null) ? priorityLookupPrefix : @"hitcircle";

            Vector2 maxSize = OsuHitObject.OBJECT_DIMENSIONS * 2;

            // at this point, any further texture fetches should be correctly using the priority source if the base texture was retrieved using it.
            // the conditional above handles the case where a sliderendcircle.png is retrieved from the skin, but sliderendcircleoverlay.png doesn't exist.
            // expected behaviour in this scenario is not showing the overlay, rather than using hitcircleoverlay.png.
            InternalChildren = new[]
            {
                CircleSprite = new LegacyKiaiFlashingDrawable(() => new Sprite { Texture = skin.GetTexture(circleName)?.WithMaximumSize(maxSize) })
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                },
                OverlayLayer = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Child = OverlaySprite = new LegacyKiaiFlashingDrawable(() => new Sprite { Texture = skin.GetTexture(@$"{circleName}overlay")?.WithMaximumSize(maxSize) })
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
                }
            };

            if (hasNumber)
            {
                OverlayLayer.Add(hitCircleText = new SkinnableSpriteText(new OsuSkinComponentLookup(OsuSkinComponents.HitCircleText), _ => new OsuSpriteText
                {
                    Font = OsuFont.Numeric.With(size: 40),
                    UseFullGlyphHeight = false,
                }, confineMode: ConfineMode.NoScaling)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                });
            }

            bool overlayAboveNumber = skin.GetConfig<OsuSkinConfiguration, bool>(OsuSkinConfiguration.HitCircleOverlayAboveNumber)?.Value ?? true;

            if (overlayAboveNumber)
                OverlayLayer.ChangeChildDepth(OverlaySprite, float.MinValue);

            if (drawableOsuObject != null)
            {
                accentColour.BindTo(drawableOsuObject.AccentColour);
                indexInCurrentCombo.BindTo(drawableOsuObject.IndexInCurrentComboBindable);
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            accentColour.BindValueChanged(colour =>
            {
                Color4 objectColour = colour.NewValue;
                int add = Math.Max(25, 300 - (int)(objectColour.R * 255) - (int)(objectColour.G * 255) - (int)(objectColour.B * 255));

                var kiaiTintColour = new Color4(
                    (byte)Math.Min((byte)(objectColour.R * 255) + add, 255),
                    (byte)Math.Min((byte)(objectColour.G * 255) + add, 255),
                    (byte)Math.Min((byte)(objectColour.B * 255) + add, 255),
                    255);

                CircleSprite.Colour = LegacyColourCompatibility.DisallowZeroAlpha(colour.NewValue);
                OverlaySprite.KiaiGlowColour = CircleSprite.KiaiGlowColour = LegacyColourCompatibility.DisallowZeroAlpha(kiaiTintColour);
            }, true);

            if (hasNumber)
                indexInCurrentCombo.BindValueChanged(index => hitCircleText.Text = (index.NewValue + 1).ToString(), true);

            if (drawableObject != null)
            {
                drawableObject.ApplyCustomUpdateState += updateStateTransforms;
                updateStateTransforms(drawableObject, drawableObject.State.Value);
            }
        }

        private void updateStateTransforms(DrawableHitObject drawableHitObject, ArmedState state)
        {
            const double legacy_fade_duration = 240;

            using (BeginAbsoluteSequence(drawableObject.AsNonNull().HitStateUpdateTime))
            {
                switch (state)
                {
                    case ArmedState.Hit:
                        CircleSprite.FadeOut(legacy_fade_duration);
                        CircleSprite.ScaleTo(1.4f, legacy_fade_duration, Easing.Out);

                        OverlaySprite.FadeOut(legacy_fade_duration);
                        OverlaySprite.ScaleTo(1.4f, legacy_fade_duration, Easing.Out);

                        if (hasNumber)
                        {
                            decimal? legacyVersion = skin.GetConfig<SkinConfiguration.LegacySetting, decimal>(SkinConfiguration.LegacySetting.Version)?.Value;

                            if (legacyVersion > 1.0m)
                                // legacy skins of version 2.0 and newer only apply very short fade out to the number piece.
                                hitCircleText.FadeOut(legacy_fade_duration / 4);
                            else
                            {
                                // old skins scale and fade it normally along other pieces.
                                hitCircleText.FadeOut(legacy_fade_duration);
                                hitCircleText.ScaleTo(1.4f, legacy_fade_duration, Easing.Out);
                            }
                        }

                        break;
                }
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (drawableObject != null)
                drawableObject.ApplyCustomUpdateState -= updateStateTransforms;
        }
    }
}
