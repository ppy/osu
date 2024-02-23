// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Skinning;
using osu.Game.Rulesets.Osu.Skinning.Default;
using osuTK;

namespace osu.Game.Rulesets.Osu.Skinning.Legacy
{
    public class OsuLegacySkinTransformer : LegacySkinTransformer
    {
        public override bool IsProvidingLegacyResources => base.IsProvidingLegacyResources || hasHitCircle.Value;

        private readonly Lazy<bool> hasHitCircle;

        /// <summary>
        /// On osu-stable, hitcircles have 5 pixels of transparent padding on each side to allow for shadows etc.
        /// Their hittable area is 128px, but the actual circle portion is 118px.
        /// We must account for some gameplay elements such as slider bodies, where this padding is not present.
        /// </summary>
        public const float LEGACY_CIRCLE_RADIUS = OsuHitObject.OBJECT_RADIUS - 5;

        /// <summary>
        /// The maximum allowed size of sprites that reside in the follow circle area of a slider.
        /// </summary>
        /// <remarks>
        /// The reason this is extracted out to a constant, rather than be inlined in the follow circle sprite retrieval,
        /// is that some skins will use `sliderb` elements to emulate a slider follow circle with slightly different visual effects applied
        /// (`sliderb` is always shown and doesn't pulsate; `sliderfollowcircle` isn't always shown and pulsates).
        /// </remarks>
        public static readonly Vector2 MAX_FOLLOW_CIRCLE_AREA_SIZE = OsuHitObject.OBJECT_DIMENSIONS * 3;

        public OsuLegacySkinTransformer(ISkin skin)
            : base(skin)
        {
            hasHitCircle = new Lazy<bool>(() => GetTexture("hitcircle") != null);
        }

        public override Drawable? GetDrawableComponent(ISkinComponentLookup lookup)
        {
            if (lookup is OsuSkinComponentLookup osuComponent)
            {
                switch (osuComponent.Component)
                {
                    case OsuSkinComponents.FollowPoint:
                        return this.GetAnimation("followpoint", true, true, true, startAtCurrentTime: false, maxSize: new Vector2(OsuHitObject.OBJECT_RADIUS * 2, OsuHitObject.OBJECT_RADIUS));

                    case OsuSkinComponents.SliderScorePoint:
                        return this.GetAnimation("sliderscorepoint", false, false, maxSize: OsuHitObject.OBJECT_DIMENSIONS);

                    case OsuSkinComponents.SliderFollowCircle:
                        var followCircleContent = this.GetAnimation("sliderfollowcircle", true, true, true, maxSize: MAX_FOLLOW_CIRCLE_AREA_SIZE);
                        if (followCircleContent != null)
                            return new LegacyFollowCircle(followCircleContent);

                        return null;

                    case OsuSkinComponents.SliderBall:
                        if (GetTexture("sliderb") != null || GetTexture("sliderb0") != null)
                            return new LegacySliderBall(this);

                        return null;

                    case OsuSkinComponents.SliderBody:
                        if (hasHitCircle.Value)
                            return new LegacySliderBody();

                        return null;

                    case OsuSkinComponents.SliderTailHitCircle:
                        if (hasHitCircle.Value)
                            return new LegacyMainCirclePiece("sliderendcircle", false);

                        return null;

                    case OsuSkinComponents.SliderHeadHitCircle:
                        if (hasHitCircle.Value)
                            return new LegacySliderHeadHitCircle();

                        return null;

                    case OsuSkinComponents.ReverseArrow:
                        if (hasHitCircle.Value)
                            return new LegacyReverseArrow();

                        return null;

                    case OsuSkinComponents.HitCircle:
                        if (hasHitCircle.Value)
                            return new LegacyMainCirclePiece();

                        return null;

                    case OsuSkinComponents.Cursor:
                        if (GetTexture("cursor") != null)
                            return new LegacyCursor(this);

                        return null;

                    case OsuSkinComponents.CursorTrail:
                        if (GetTexture("cursortrail") != null)
                            return new LegacyCursorTrail(this);

                        return null;

                    case OsuSkinComponents.CursorRipple:
                        if (GetTexture("cursor-ripple") != null)
                        {
                            var ripple = this.GetAnimation("cursor-ripple", false, false);

                            // In stable this element was scaled down to 50% and opacity 20%, but this makes the elements WAY too big and inflexible.
                            // If anyone complains about these not being applied, this can be uncommented.
                            //
                            // But if no one complains I'd rather fix this in lazer. Wiki documentation doesn't mention size,
                            // so we might be okay.
                            //
                            // if (ripple != null)
                            // {
                            //     ripple.Scale = new Vector2(0.5f);
                            //     ripple.Alpha = 0.2f;
                            // }

                            return ripple;
                        }

                        return null;

                    case OsuSkinComponents.CursorParticles:
                        if (GetTexture("star2") != null)
                            return new LegacyCursorParticles();

                        return null;

                    case OsuSkinComponents.CursorSmoke:
                        if (GetTexture("cursor-smoke") != null)
                            return new LegacySmokeSegment();

                        return null;

                    case OsuSkinComponents.HitCircleText:
                        if (!this.HasFont(LegacyFont.HitCircle))
                            return null;

                        const float hitcircle_text_scale = 0.8f;
                        return new LegacySpriteText(LegacyFont.HitCircle)
                        {
                            // stable applies a blanket 0.8x scale to hitcircle fonts
                            Scale = new Vector2(hitcircle_text_scale),
                            MaxSizePerGlyph = OsuHitObject.OBJECT_DIMENSIONS * 2 / hitcircle_text_scale,
                        };

                    case OsuSkinComponents.SpinnerBody:
                        bool hasBackground = GetTexture("spinner-background") != null;

                        if (GetTexture("spinner-top") != null && !hasBackground)
                            return new LegacyNewStyleSpinner();
                        else if (hasBackground)
                            return new LegacyOldStyleSpinner();

                        return null;

                    case OsuSkinComponents.ApproachCircle:
                        if (GetTexture(@"approachcircle") != null)
                            return new LegacyApproachCircle();

                        return null;

                    case OsuSkinComponents.HitMarkerLeft:
                        if (GetTexture(@"hitmarker-left") != null)
                            return new HitMarker(OsuAction.LeftButton);

                        return null;

                    case OsuSkinComponents.HitMarkerRight:
                        if (GetTexture(@"hitmarker-right") != null)
                            return new HitMarker(OsuAction.RightButton);

                        return null;

                    case OsuSkinComponents.AimMarker:
                        if (GetTexture(@"aimmarker") != null)
                            return new HitMarker();

                        return null;
                }
            }

            return base.GetDrawableComponent(lookup);
        }

        public override IBindable<TValue>? GetConfig<TLookup, TValue>(TLookup lookup)
        {
            switch (lookup)
            {
                case OsuSkinColour colour:
                    return base.GetConfig<SkinCustomColourLookup, TValue>(new SkinCustomColourLookup(colour));

                case OsuSkinConfiguration osuLookup:
                    switch (osuLookup)
                    {
                        case OsuSkinConfiguration.SliderPathRadius:
                            if (hasHitCircle.Value)
                                return SkinUtils.As<TValue>(new BindableFloat(LEGACY_CIRCLE_RADIUS));

                            break;

                        case OsuSkinConfiguration.HitCircleOverlayAboveNumber:
                            // See https://osu.ppy.sh/help/wiki/Skinning/skin.ini#%5Bgeneral%5D
                            // HitCircleOverlayAboveNumer (with typo) should still be supported for now.
                            return base.GetConfig<OsuSkinConfiguration, TValue>(OsuSkinConfiguration.HitCircleOverlayAboveNumber) ??
                                   base.GetConfig<OsuSkinConfiguration, TValue>(OsuSkinConfiguration.HitCircleOverlayAboveNumer);
                    }

                    break;
            }

            return base.GetConfig<TLookup, TValue>(lookup);
        }
    }
}
