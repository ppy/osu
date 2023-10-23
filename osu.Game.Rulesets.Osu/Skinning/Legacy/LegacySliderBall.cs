// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Legacy
{
    public partial class LegacySliderBall : CompositeDrawable
    {
        private readonly ISkin skin;

        [Resolved(canBeNull: true)]
        private DrawableHitObject? parentObject { get; set; }

        private Sprite layerNd = null!;
        private Sprite layerSpec = null!;

        private TextureAnimation ballAnimation = null!;
        private Texture[] ballTextures = null!;

        public Color4 BallColour => ballAnimation.Colour;

        public LegacySliderBall(ISkin skin)
        {
            this.skin = skin;

            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Vector2 maxSize = OsuLegacySkinTransformer.MAX_FOLLOW_CIRCLE_AREA_SIZE;

            var ballColour = skin.GetConfig<OsuSkinColour, Color4>(OsuSkinColour.SliderBall)?.Value ?? Color4.White;
            ballTextures = skin.GetTextures("sliderb", default, default, true, "", maxSize, out _);

            InternalChildren = new Drawable[]
            {
                layerNd = new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Texture = skin.GetTexture("sliderb-nd")?.WithMaximumSize(maxSize),
                    Colour = new Color4(5, 5, 5, 255),
                },
                ballAnimation = new LegacySkinExtensions.SkinnableTextureAnimation
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = ballColour,
                },
                layerSpec = new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Texture = skin.GetTexture("sliderb-spec")?.WithMaximumSize(maxSize),
                    Blending = BlendingParameters.Additive,
                },
            };

            if (parentObject != null)
                parentObject.HitObjectApplied += onHitObjectApplied;

            onHitObjectApplied(parentObject);
        }

        private readonly IBindable<Color4> accentColour = new Bindable<Color4>();

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (parentObject != null)
            {
                parentObject.ApplyCustomUpdateState += updateStateTransforms;
                updateStateTransforms(parentObject, parentObject.State.Value);

                if (skin.GetConfig<SkinConfiguration.LegacySetting, bool>(SkinConfiguration.LegacySetting.AllowSliderBallTint)?.Value == true)
                {
                    accentColour.BindTo(parentObject.AccentColour);
                    accentColour.BindValueChanged(a => ballAnimation.Colour = a.NewValue, true);
                }
            }
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            //undo rotation on layers which should not be rotated.
            float appliedRotation = Parent!.Rotation;

            layerNd.Rotation = -appliedRotation;
            layerSpec.Rotation = -appliedRotation;
        }

        private void onHitObjectApplied(DrawableHitObject? drawableObject = null)
        {
            if (drawableObject != null && drawableObject is not DrawableSlider)
                return;

            ballAnimation.ClearFrames();

            double frameDelay;

            if (drawableObject?.HitObject != null)
            {
                DrawableSlider drawableSlider = (DrawableSlider)drawableObject;

                // stable apparently calculates slider velocity in units of seconds rather than milliseconds.
                double stableSliderVelocity = drawableSlider.HitObject.Velocity * 1000;

                frameDelay = Math.Max(
                    150 / stableSliderVelocity * LegacySkinExtensions.SIXTY_FRAME_TIME,
                    LegacySkinExtensions.SIXTY_FRAME_TIME);
            }
            else
                frameDelay = LegacySkinExtensions.SIXTY_FRAME_TIME;

            foreach (var texture in ballTextures)
                ballAnimation.AddFrame(texture, frameDelay);
        }

        private void updateStateTransforms(DrawableHitObject drawableObject, ArmedState _)
        {
            // Gets called by slider ticks, tails, etc., leading to duplicated
            // animations which in this case have no visual impact (due to
            // instant fade) but may negatively affect performance
            if (drawableObject is not DrawableSlider)
                return;

            using (BeginAbsoluteSequence(drawableObject.StateUpdateTime))
                this.FadeIn();

            using (BeginAbsoluteSequence(drawableObject.HitStateUpdateTime))
                this.FadeOut();
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (parentObject != null)
            {
                parentObject.HitObjectApplied -= onHitObjectApplied;
                parentObject.ApplyCustomUpdateState -= updateStateTransforms;
            }
        }
    }
}
