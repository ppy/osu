// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Argon
{
    public partial class ArgonFollowCircle : FollowCircle
    {
        private readonly CircularContainer circleContainer;
        private readonly Box circleFill;

        private readonly IBindable<Color4> accentColour = new Bindable<Color4>();

        [Resolved(canBeNull: true)]
        private DrawableHitObject? parentObject { get; set; }

        public ArgonFollowCircle()
        {
            InternalChild = circleContainer = new CircularContainer
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                BorderThickness = 4,
                Blending = BlendingParameters.Additive,
                Child = circleFill = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0.3f,
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (parentObject != null)
                accentColour.BindTo(parentObject.AccentColour);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            accentColour.BindValueChanged(colour =>
            {
                circleContainer.BorderColour = ColourInfo.GradientVertical(colour.NewValue, colour.NewValue.Darken(0.5f));
                circleFill.Colour = ColourInfo.GradientVertical(colour.NewValue, colour.NewValue.Darken(0.5f));
            }, true);
        }

        protected override void OnSliderPress()
        {
            const float duration = 300f;

            if (Precision.AlmostEquals(0, Alpha))
                this.ScaleTo(1);

            this.ScaleTo(FollowAreaScale, duration, Easing.OutQuint)
                .FadeIn(duration, Easing.OutQuint);
        }

        protected override void OnSliderRelease()
        {
            const float duration = 150;

            this.ScaleTo(FollowAreaScale * 1.2f, duration, Easing.OutQuint)
                .FadeTo(0, duration, Easing.OutQuint);
        }

        protected override void OnSliderEnd()
        {
            const float duration = 300;

            this.ScaleTo(1, duration, Easing.OutQuint)
                .FadeOut(duration / 2, Easing.OutQuint);
        }

        protected override void OnSliderTick()
        {
            if (Scale.X >= FollowAreaScale * 0.98f)
            {
                this.ScaleTo(FollowAreaScale * 1.08f, 40, Easing.OutQuint)
                    .Then()
                    .ScaleTo(FollowAreaScale, 200f, Easing.OutQuint);
            }
        }

        protected override void OnSliderBreak()
        {
        }
    }
}
