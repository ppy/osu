// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Utils;
using osu.Game.Rulesets.Catch.Skinning.Default;
using osu.Game.Rulesets.Catch.UI;
using osuTK;

namespace osu.Game.Rulesets.Catch.Skinning.Argon
{
    internal partial class ArgonDropletPiece : CatchHitObjectPiece
    {
        protected override Drawable HyperBorderPiece => hyperBorderPiece;

        private Drawable hyperBorderPiece = null!;

        private Container layers = null!;

        private float rotationRandomness;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;

            const float droplet_scale_down = 0.7f;

            int largeBlobSeed = RNG.Next();

            InternalChildren = new[]
            {
                new Circle
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(20),
                },
                layers = new Container
                {
                    Scale = new Vector2(droplet_scale_down),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new CircularBlob
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Blending = BlendingParameters.Additive,
                            InnerRadius = 0.5f,
                            Alpha = 0.15f,
                            Seed = largeBlobSeed
                        },
                        new CircularBlob
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Blending = BlendingParameters.Additive,
                            InnerRadius = 0.4f,
                            Alpha = 0.5f,
                            Scale = new Vector2(0.7f),
                            Seed = RNG.Next()
                        },
                    }
                },
                hyperBorderPiece = new CircularBlob
                {
                    Scale = new Vector2(droplet_scale_down),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = Catcher.DEFAULT_HYPER_DASH_COLOUR,
                    RelativeSizeAxes = Axes.Both,
                    Blending = BlendingParameters.Additive,
                    InnerRadius = 0.5f,
                    Alpha = 0.15f,
                    Seed = largeBlobSeed
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AccentColour.BindValueChanged(colour =>
            {
                foreach (var sprite in layers)
                    sprite.Colour = colour.NewValue;
            }, true);

            rotationRandomness = RNG.NextSingle(0.2f, 1);
        }

        protected override void Update()
        {
            base.Update();

            // Note that droplets are rotated at a higher level, so this is mostly just to create more
            // random arrangements of the multiple layers than actually rotate.
            //
            // Because underlying rotation is always clockwise, we apply anti-clockwise resistance to avoid
            // making things spin too fast.
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].Rotation -=
                    (float)Clock.ElapsedFrameTime
                    * 0.4f * rotationRandomness
                    // Each layer should alternate rotation speed.
                    * (i % 2 == 1 ? 0.5f : 1);
            }
        }
    }
}
