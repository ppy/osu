// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osuTK.Graphics;
using Box = osu.Framework.Graphics.Shapes.Box;

namespace osu.Game.Rulesets.Mania.Skinning.Argon
{
    public partial class ArgonHoldNoteHittingLayer : Box
    {
        public readonly Bindable<Color4> AccentColour = new Bindable<Color4>();
        public readonly Bindable<bool> IsHitting = new Bindable<bool>();

        public ArgonHoldNoteHittingLayer()
        {
            RelativeSizeAxes = Axes.Both;
            Blending = BlendingParameters.Additive;
            Alpha = 0;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AccentColour.BindValueChanged(colour =>
            {
                Colour = colour.NewValue.Opacity(0.2f);
            }, true);

            IsHitting.BindValueChanged(hitting =>
            {
                const float animation_length = 50;

                ClearTransforms();

                if (hitting.NewValue)
                {
                    // wait for the next sync point
                    double synchronisedOffset = animation_length * 2 - Time.Current % (animation_length * 2);

                    using (BeginDelayedSequence(synchronisedOffset))
                    {
                        this.FadeTo(1, animation_length).Then()
                            .FadeTo(0.5f, animation_length)
                            .Loop();
                    }
                }
                else
                {
                    this.FadeOut(animation_length);
                }
            });
        }

        public void Recycle()
        {
            ClearTransforms();
            Alpha = 0;
        }
    }
}
