// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Legacy
{
    public partial class LegacySliderBall : CompositeDrawable
    {
        private readonly Drawable animationContent;

        private readonly ISkin skin;

        [Resolved(canBeNull: true)]
        private DrawableHitObject? parentObject { get; set; }

        public Color4 BallColour => animationContent.Colour;

        private Sprite layerNd = null!;
        private Sprite layerSpec = null!;

        public LegacySliderBall(Drawable animationContent, ISkin skin)
        {
            this.animationContent = animationContent;
            this.skin = skin;

            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var ballColour = skin.GetConfig<OsuSkinColour, Color4>(OsuSkinColour.SliderBall)?.Value ?? Color4.White;

            InternalChildren = new[]
            {
                layerNd = new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Texture = skin.GetTexture("sliderb-nd")?.WithMaximumSize(OsuLegacySkinTransformer.MAX_FOLLOW_CIRCLE_AREA_SIZE),
                    Colour = new Color4(5, 5, 5, 255),
                },
                LegacyColourCompatibility.ApplyWithDoubledAlpha(animationContent.With(d =>
                {
                    d.Anchor = Anchor.Centre;
                    d.Origin = Anchor.Centre;
                }), ballColour),
                layerSpec = new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Texture = skin.GetTexture("sliderb-spec")?.WithMaximumSize(OsuLegacySkinTransformer.MAX_FOLLOW_CIRCLE_AREA_SIZE),
                    Blending = BlendingParameters.Additive,
                },
            };
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
                    accentColour.BindValueChanged(a => animationContent.Colour = a.NewValue, true);
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
                parentObject.ApplyCustomUpdateState -= updateStateTransforms;
        }
    }
}
