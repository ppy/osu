// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Testing;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Mania.Skinning.Legacy
{
    public partial class LegacyBodyPiece : LegacyManiaColumnElement
    {
        private DrawableHoldNote holdNote = null!;

        private readonly IBindable<ScrollingDirection> direction = new Bindable<ScrollingDirection>();
        private readonly IBindable<bool> isHitting = new Bindable<bool>();

        /// <summary>
        /// Stores the start time of the fade animation that plays when any of the nested
        /// hitobjects of the hold note are missed.
        /// </summary>
        private readonly Bindable<double?> missFadeTime = new Bindable<double?>();

        private Drawable? bodySprite;

        private Drawable? lightContainer;

        private Drawable? light;
        private LegacyNoteBodyStyle? bodyStyle;

        public LegacyBodyPiece()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin, IScrollingInfo scrollingInfo, DrawableHitObject drawableObject)
        {
            holdNote = (DrawableHoldNote)drawableObject;

            string imageName = GetColumnSkinConfig<string>(skin, LegacyManiaSkinConfigurationLookups.HoldNoteBodyImage)?.Value
                               ?? $"mania-note{FallbackColumnIndex}L";

            string lightImage = GetColumnSkinConfig<string>(skin, LegacyManiaSkinConfigurationLookups.HoldNoteLightImage)?.Value
                                ?? "lightingL";

            float lightScale = GetColumnSkinConfig<float>(skin, LegacyManiaSkinConfigurationLookups.HoldNoteLightScale)?.Value
                               ?? 1;

            // Create a temporary animation to retrieve the number of frames, in an effort to calculate the intended frame length.
            // This animation is discarded and re-queried with the appropriate frame length afterwards.
            var tmp = skin.GetAnimation(lightImage, true, false);
            double frameLength = 0;
            if (tmp is IFramedAnimation tmpAnimation && tmpAnimation.FrameCount > 0)
                frameLength = Math.Max(1000 / 60.0, 170.0 / tmpAnimation.FrameCount);

            light = skin.GetAnimation(lightImage, true, true, frameLength: frameLength).With(d =>
            {
                if (d == null)
                    return;

                d.Origin = Anchor.Centre;
                d.Blending = BlendingParameters.Additive;
                d.Scale = new Vector2(lightScale);
            });

            if (light != null)
            {
                lightContainer = new HitTargetInsetContainer
                {
                    Alpha = 0,
                    Child = light
                };
            }

            bodyStyle = skin.GetConfig<ManiaSkinConfigurationLookup, LegacyNoteBodyStyle>(new ManiaSkinConfigurationLookup(LegacyManiaSkinConfigurationLookups.NoteBodyStyle))?.Value;

            var wrapMode = bodyStyle == LegacyNoteBodyStyle.Stretch ? WrapMode.ClampToEdge : WrapMode.Repeat;

            direction.BindTo(scrollingInfo.Direction);
            isHitting.BindTo(holdNote.IsHitting);

            bodySprite = skin.GetAnimation(imageName, wrapMode, wrapMode, true, true).With(d =>
            {
                if (d == null)
                    return;

                if (d is TextureAnimation animation)
                    animation.IsPlaying = false;

                d.Anchor = Anchor.TopCentre;
                d.RelativeSizeAxes = Axes.Both;
                d.Size = Vector2.One;
                // Todo: Wrap?
            });

            if (bodySprite != null)
                InternalChild = bodySprite;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            direction.BindValueChanged(onDirectionChanged, true);
            isHitting.BindValueChanged(onIsHittingChanged, true);
            missFadeTime.BindValueChanged(onMissFadeTimeChanged, true);

            holdNote.ApplyCustomUpdateState += applyCustomUpdateState;
            applyCustomUpdateState(holdNote, holdNote.State.Value);
        }

        private void applyCustomUpdateState(DrawableHitObject hitObject, ArmedState state)
        {
            switch (hitObject)
            {
                // Ensure that the hold note is also faded out when the head/tail/body is missed.
                // Importantly, we filter out unrelated objects like DrawableNotePerfectBonus.
                case DrawableHoldNoteTail:
                case DrawableHoldNoteHead:
                case DrawableHoldNoteBody:
                    if (state == ArmedState.Miss)
                        missFadeTime.Value ??= hitObject.HitStateUpdateTime;

                    break;
            }
        }

        private void onIsHittingChanged(ValueChangedEvent<bool> isHitting)
        {
            if (bodySprite is TextureAnimation bodyAnimation)
            {
                bodyAnimation.GotoFrame(0);
                bodyAnimation.IsPlaying = isHitting.NewValue;
            }

            if (lightContainer == null)
                return;

            if (isHitting.NewValue)
            {
                // Clear the fade out and, more importantly, the removal.
                lightContainer.ClearTransforms();

                // Only add the container if the removal has taken place.
                if (lightContainer.Parent == null)
                    Column.TopLevelContainer.Add(lightContainer);

                // The light must be seeked only after being loaded, otherwise a nullref occurs (https://github.com/ppy/osu-framework/issues/3847).
                if (light is TextureAnimation lightAnimation)
                    lightAnimation.GotoFrame(0);

                lightContainer.FadeIn(80);
            }
            else
            {
                lightContainer.FadeOut(120)
                              .OnComplete(d => Column.TopLevelContainer.Remove(d, false));
            }
        }

        private void onDirectionChanged(ValueChangedEvent<ScrollingDirection> direction)
        {
            if (direction.NewValue == ScrollingDirection.Up)
            {
                if (bodySprite != null)
                {
                    bodySprite.Origin = Anchor.TopCentre;
                    bodySprite.Anchor = Anchor.BottomCentre; // needs to be flipped due to scale flip in Update.
                }

                if (light != null)
                    light.Anchor = Anchor.TopCentre;
            }
            else
            {
                if (bodySprite != null)
                {
                    bodySprite.Origin = Anchor.TopCentre;
                    bodySprite.Anchor = Anchor.TopCentre;
                }

                if (light != null)
                    light.Anchor = Anchor.BottomCentre;
            }
        }

        private void onMissFadeTimeChanged(ValueChangedEvent<double?> missFadeTimeChange)
        {
            if (missFadeTimeChange.NewValue == null)
                return;

            // this update could come from any nested object of the hold note (or even from an input).
            // make sure the transforms are consistent across all affected parts.
            using (BeginAbsoluteSequence(missFadeTimeChange.NewValue.Value))
            {
                // colour and duration matches stable
                // transforms not applied to entire hold note in order to not affect hit lighting
                const double fade_duration = 60;

                holdNote.Head.FadeColour(Colour4.DarkGray, fade_duration);
                holdNote.Tail.FadeColour(Colour4.DarkGray, fade_duration);
                bodySprite?.FadeColour(Colour4.DarkGray, fade_duration);
            }
        }

        protected override void Update()
        {
            base.Update();

            if (holdNote.Body.HasHoldBreak)
                missFadeTime.Value = holdNote.Body.Result.TimeAbsolute;

            int scaleDirection = (direction.Value == ScrollingDirection.Down ? 1 : -1);

            // here we go...
            switch (bodyStyle)
            {
                case LegacyNoteBodyStyle.Stretch:
                    // this is how lazer works by default. nothing required.
                    if (bodySprite != null)
                        bodySprite.Scale = new Vector2(1, scaleDirection);
                    break;

                default:
                    // this is where things get fucked up.
                    // honestly there's three modes to handle here but they seem really pointless?
                    // let's wait to see if anyone actually uses them in skins.
                    if (bodySprite != null)
                    {
                        var sprite = bodySprite as Sprite ?? bodySprite.ChildrenOfType<Sprite>().Single();

                        bodySprite.FillMode = FillMode.Stretch;
                        // i dunno this looks about right??
                        bodySprite.Scale = new Vector2(1, scaleDirection * 32800 / sprite.DrawHeight);
                    }

                    break;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (holdNote.IsNotNull())
                holdNote.ApplyCustomUpdateState -= applyCustomUpdateState;

            lightContainer?.Expire();
        }
    }
}
