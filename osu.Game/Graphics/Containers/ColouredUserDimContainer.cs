// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Bindables;
using osu.Game.Configuration;

namespace osu.Game.Graphics.Containers
{
    public partial class ColouredUserDimContainer : UserDimContainer, IColouredDimmableSource
    {
        private ColouredDimmableSharedData sharedData;

        private Bindable<double> userDimColour { get; set; }

        protected ColouredUserDimContainer()
        {
            sharedData = new ColouredDimmableSharedData();
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            userDimColour = config.GetBindable<double>(OsuSetting.DimColour);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            userDimColour.ValueChanged += _ => UpdateVisuals();
        }

        protected override DrawNode CreateDrawNode() => new ColouredUserDimContainerDrawNode(this, sharedData);

        protected virtual Colour4 DimColour
        {
            get
            {
                // This assumes that DimColour shouldn't be affected by DimWhenUserSettingsIgnored
                // (as it's used to dim things in song select only)
                if (IgnoreUserSettings.Value)
                    return Colour4.Black;

                float grayscaleDimColour = (float)userDimColour.Value;
                return new Colour4(grayscaleDimColour, grayscaleDimColour, grayscaleDimColour, 1.0f);
            }
        }

        // The atrocity below is necessary because FadeColour and TransformTo interpolate
        // colours in linear space, then convert them back to sRGB.
        // Which means that animating ColourOffset directly will NOT make it a perfect
        // complement to Colour, so adding them won't necessary give 1.0 (assuming
        // no external sources of dimming).
        // And using dimContent's Colour directly may include dimming caused by storyboard.
        // So this roundabout way is used instead, where AnimatedInvertedDimLevel emulates
        // dimContent's Colour
        protected virtual Colour4 InvertedDimLevel => new Colour4(
            1f - DimLevel,
            1f - DimLevel,
            1f - DimLevel,
            1f
        );

        private Colour4 animatedInvertedDimLevel = Colour4.White;

        protected Colour4 AnimatedInvertedDimLevel
        {
            get => animatedInvertedDimLevel;
            set
            {
                if (value.Equals(animatedInvertedDimLevel))
                    return;

                animatedInvertedDimLevel = value;
                Invalidate(Invalidation.DrawNode);
            }
        }

        private Colour4 animatedDimColour;

        protected Colour4 AnimatedDimColour
        {
            get => animatedDimColour;
            set
            {
                if (value.Equals(animatedDimColour))
                    return;

                animatedDimColour = value;
                Invalidate(Invalidation.DrawNode);
            }
        }

        protected Colour4 AnimatedDimLevel => new Colour4(
            1f - AnimatedInvertedDimLevel.R,
            1f - AnimatedInvertedDimLevel.G,
            1f - AnimatedInvertedDimLevel.B,
            1f
        );

        protected Colour4 ColourOffset => AnimatedDimColour * AnimatedDimLevel;

        // What coloured dimmable shader technically does is it interpolates
        // dimmable's colour toward DimColour using DimLevel:
        // TargetColour = mix(TextureColour, DimColour, DimLevel) = TextureColour * (1 - DimLevel) + DimColour * DimLevel
        // (interpolation happens in sRGB, as current dimming also happens in sRGB)
        //
        // This can be split into two parts:
        // Colour = TextureColour * (1 - DimLevel)
        // ColourOffset = DimColour * DimLevel
        // TargetColour = Colour + ColourOffset
        //
        // This target colour can be affected by parent's colour, and multiplication
        // happens in linear, before being converted back to sRGB:
        // FinalColour = (ParentColour.ToLinear() * TargetColour.ToLinear()).ToSRGB()
        //
        // However, shader just adds two values (Colour and ColourOffset), and
        // Colour is already properly multiplied by parent's colour by framework (DrawColour),
        // so what we need is:
        // DrawColour + DrawColourOffset = FinalColour
        // DrawColourOffset = FinalColour - DrawColour
        //
        // And DrawColour can be emulated through AnimatedInvertedDimLevel:
        // DrawColour = (ParentColour.ToLinear() * AnimatedInvertedDimLevel.ToLinear()).ToSRGB()
        public Colour4 DrawColourOffset
        {
            get
            {
                Colour4 parentColour = (Colour4)DrawColourInfo.Colour;
                if (parentColour == Colour4.White)
                    return ColourOffset;

                Colour4 emulatedColour = AnimatedInvertedDimLevel;

                Colour4 emulatedDrawColour = (parentColour.ToLinear() * emulatedColour.ToLinear()).ToSRGB();

                Colour4 targetColour = emulatedColour + ColourOffset;
                targetColour = new Colour4(targetColour.R, targetColour.G, targetColour.B, 1f);

                Colour4 finalColour = (parentColour.ToLinear() * targetColour.ToLinear()).ToSRGB();

                Colour4 drawColourOffset = finalColour - emulatedDrawColour;

                return new Colour4(drawColourOffset.R, drawColourOffset.G, drawColourOffset.B, 1f);
            }
        }

        public ColouredDimmableSharedData ColouredDimmableSharedData => sharedData;

        protected override void UpdateVisuals()
        {
            base.UpdateVisuals();

            this.TransformTo(nameof(AnimatedInvertedDimLevel), InvertedDimLevel, BACKGROUND_FADE_DURATION, Easing.OutQuint);
            this.TransformTo(nameof(AnimatedDimColour), DimColour, BACKGROUND_FADE_DURATION, Easing.OutQuint);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            sharedData.Dispose();
        }

        protected class ColouredUserDimContainerDrawNode : CompositeDrawableDrawNode
        {
            public new ColouredUserDimContainer Source => (ColouredUserDimContainer)base.Source;

            private ColouredDimmableSharedData sharedData;

            private Colour4 drawColourOffset;

            public ColouredUserDimContainerDrawNode(ColouredUserDimContainer source, ColouredDimmableSharedData sharedData)
                : base(source)
            {
                this.sharedData = sharedData;
            }

            public override void ApplyState()
            {
                base.ApplyState();

                drawColourOffset = Source.DrawColourOffset;
            }

            protected override void Draw(IRenderer renderer)
            {
                // Update shared data before drawing children that rely on it
                sharedData.SetDrawColourOffset(drawColourOffset);

                base.Draw(renderer);
            }
        }
    }
}
