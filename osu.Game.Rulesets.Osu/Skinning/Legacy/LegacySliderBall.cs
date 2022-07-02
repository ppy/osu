// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Legacy
{
    public class LegacySliderBall : CompositeDrawable
    {
        private readonly Drawable animationContent;

        private readonly ISkin skin;

        [Resolved(canBeNull: true)]
        private DrawableHitObject? drawableObject { get; set; }

        private Sprite layerNd = null!;
        private Sprite layerSpec = null!;

        public LegacySliderBall(Drawable animationContent, ISkin skin)
        {
            this.animationContent = animationContent;
            this.skin = skin;

            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(DrawableHitObject dho)
        {
            var ballColour = skin.GetConfig<OsuSkinColour, Color4>(OsuSkinColour.SliderBall)?.Value ?? Color4.White;

            InternalChildren = new[]
            {
                layerNd = new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Texture = skin.GetTexture("sliderb-nd"),
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
                    Texture = skin.GetTexture("sliderb-spec"),
                    Blending = BlendingParameters.Additive,
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (drawableObject != null)
            {
                drawableObject.ApplyCustomUpdateState += updateStateTransforms;
                updateStateTransforms(drawableObject, drawableObject.State.Value);
            }
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            //undo rotation on layers which should not be rotated.
            float appliedRotation = Parent.Rotation;

            layerNd.Rotation = -appliedRotation;
            layerSpec.Rotation = -appliedRotation;
        }

        private void updateStateTransforms(DrawableHitObject obj, ArmedState _)
        {
            using (BeginAbsoluteSequence(drawableObject.AsNonNull().StateUpdateTime))
                this.FadeIn();

            using (BeginAbsoluteSequence(drawableObject.AsNonNull().HitStateUpdateTime))
                this.FadeOut();
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (drawableObject != null)
                drawableObject.ApplyCustomUpdateState -= updateStateTransforms;
        }
    }
}
